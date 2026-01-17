param(
  [Parameter(Mandatory = $true)]
  [string]$Model,
  [string]$LogDir
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptDir "..")).Path

if (-not $LogDir) {
  $LogDir = Join-Path $repoRoot "logs/startup"
}

New-Item -ItemType Directory -Force -Path $LogDir | Out-Null

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$logPath = Join-Path $LogDir "startup-$timestamp.log"
$latestPath = Join-Path $LogDir "latest.log"
$statePath = Join-Path $LogDir "state.json"
$statusPath = Join-Path $LogDir "status.json"

$errors = New-Object System.Collections.Generic.List[string]
$warnings = New-Object System.Collections.Generic.List[string]

function Write-Log {
  param([string]$Message)
  $stamp = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss.fffK")
  $line = "$stamp $Message"
  Add-Content -Path $logPath -Value $line
  Write-Output $line
}

function Add-Error {
  param([string]$Message)
  $errors.Add($Message)
  Write-Log "ERROR: $Message"
}

function Add-Warning {
  param([string]$Message)
  $warnings.Add($Message)
  Write-Log "WARN: $Message"
}

function Invoke-LoggedCommand {
  param(
    [string]$Label,
    [string[]]$Command,
    [switch]$Critical
  )
  Write-Log "COMMAND: $Label"
  $commandList = @($Command)
  $commandName = $commandList[0]
  $commandArgs = @()
  if ($commandList.Count -gt 1) {
    $commandArgs = @($commandList[1..($commandList.Count - 1)])
  }
  try {
    $output = & $commandName @commandArgs 2>&1
  } catch {
    $output = @($_)
  }
  $exitCode = $LASTEXITCODE
  if ($null -eq $exitCode) {
    $exitCode = 127
  }
  if ($output) {
    foreach ($line in $output) {
      Write-Log "OUTPUT: $([string]$line)"
    }
  }
  Write-Log "STATUS: $exitCode"
  if ($exitCode -ne 0) {
    $message = "$Label failed with exit code $exitCode."
    if ($Critical) {
      Add-Error $message
    } else {
      Add-Warning $message
    }
  }
  return @{
    ExitCode = $exitCode
    Output = $output
  }
}

function Get-RelativePath {
  param([string]$Root, [string]$Path)
  $rootPath = $Root.TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar
  ) + [System.IO.Path]::DirectorySeparatorChar
  $rootUri = New-Object System.Uri($rootPath)
  $pathUri = New-Object System.Uri($Path)
  $relative = $rootUri.MakeRelativeUri($pathUri).ToString()
  return [System.Uri]::UnescapeDataString(
    $relative.Replace("/", [System.IO.Path]::DirectorySeparatorChar)
  )
}

Write-Log "STEP 1: Workspace context"
Write-Log "Repo root: $repoRoot"
Write-Log "OS: $([System.Environment]::OSVersion.VersionString)"
Write-Log "Platform: $([System.Environment]::OSVersion.Platform)"
Write-Log "User: $env:USERNAME"
Write-Log "Shell: $env:ComSpec"
Write-Log "Host: $($Host.Name)"
Write-Log "PowerShell: $($PSVersionTable.PSVersion)"
Write-Log "TERM: $env:TERM"
Write-Log "TERM_PROGRAM: $env:TERM_PROGRAM"
Write-Log "WT_SESSION: $env:WT_SESSION"
Write-Log "VSCODE_PID: $env:VSCODE_PID"

try {
  $allItems = Get-ChildItem -Path $repoRoot -Force -Recurse
  $dirs = $allItems | Where-Object { $_.PSIsContainer }
  $files = $allItems | Where-Object { -not $_.PSIsContainer }
  Write-Log "Workspace directories ($($dirs.Count)):"
  foreach ($dir in $dirs) {
    Write-Log "DIR: $(Get-RelativePath $repoRoot $dir.FullName)"
  }
  Write-Log "Workspace files ($($files.Count)):"
  foreach ($file in $files) {
    Write-Log "FILE: $(Get-RelativePath $repoRoot $file.FullName)"
  }
} catch {
  Add-Error "Failed to enumerate workspace: $($_.Exception.Message)"
}

Write-Log "STEP 2: Preload agent tasks"
$ffrwdCmd = Join-Path $repoRoot "ffrwd.cmd"
if (-not (Test-Path $ffrwdCmd)) {
  Add-Error "ffrwd.cmd not found at repo root."
}

$dotnetCmd = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnetCmd) {
  $candidateDotnet = @()
  if ($env:DOTNET_INSTALL_DIR) {
    $candidateDotnet += (Join-Path $env:DOTNET_INSTALL_DIR "dotnet.exe")
  }
  if ($env:ProgramFiles) {
    $candidateDotnet += (Join-Path $env:ProgramFiles "dotnet\dotnet.exe")
  }
  if (${env:ProgramFiles(x86)}) {
    $candidateDotnet += (Join-Path ${env:ProgramFiles(x86)} "dotnet\dotnet.exe")
  }
  if ($env:USERPROFILE) {
    $candidateDotnet += (Join-Path $env:USERPROFILE ".dotnet\dotnet.exe")
  }
  $dotnetCmd = $candidateDotnet | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
}

$dotnetPath = $null
if ($dotnetCmd -is [System.Management.Automation.CommandInfo]) {
  $dotnetPath = $dotnetCmd.Source
} else {
  $dotnetPath = $dotnetCmd
}

$buildResult = $null
$buildOk = $false
if (-not $dotnetPath) {
  Add-Error "dotnet not found. Run Scripts/install-dotnet.ps1."
} else {
  Write-Log "dotnet: $dotnetPath"
  $buildResult = Invoke-LoggedCommand -Label "dotnet build ffrwd" -Command @(
    $dotnetPath,
    "build",
    "Sources/ffrwd/ffrwd.csproj"
  ) -Critical
  if ($buildResult.ExitCode -eq 0) {
    $buildOk = $true
  }
}

$worktreePath = ""
$startExitCode = $null
if ($buildOk -and $dotnetPath -and (Test-Path $ffrwdCmd)) {
  $startResult = Invoke-LoggedCommand -Label "ffrwd agent start" -Command @(
    $ffrwdCmd,
    "agent",
    "start",
    $Model,
    "--emit=path"
  ) -Critical
  $startExitCode = $startResult.ExitCode
  if ($startExitCode -eq 0 -and $startResult.Output) {
    $text = [string]::Join("`n", @($startResult.Output))
    $lines = $text -split '\r?\n' | Where-Object { $_ -and $_.Trim() }
    if ($lines.Count -gt 0) {
      $worktreePath = $lines[-1].Trim()
    }
  }
  if ($startExitCode -eq 0 -and [string]::IsNullOrWhiteSpace($worktreePath)) {
    Add-Error "Failed to resolve worktree path from ffrwd output."
  } elseif (-not [string]::IsNullOrWhiteSpace($worktreePath)) {
    Write-Log "Worktree path: $worktreePath"
  }
} elseif (-not $buildOk) {
  Add-Warning "Skipping ffrwd agent start because build failed."
}

Write-Log "STEP 3: Refresh doctrine, playbook, and TODO"
$doctrineRoot = Join-Path $repoRoot "Doctrine"
$playbookRoot = Join-Path $repoRoot "Playbook"
$todoIndex = Join-Path $doctrineRoot "TODO.yml.md"
$requiredFiles = @(
  (Join-Path $repoRoot "AGENTS.yml.md"),
  (Join-Path $doctrineRoot "Startup.yml.md"),
  (Join-Path $doctrineRoot "Playbook.md"),
  $todoIndex
)

foreach ($req in $requiredFiles) {
  if (-not (Test-Path $req)) {
    Add-Error "Required file missing: $(Get-RelativePath $repoRoot $req)"
  } else {
    Write-Log "Required file present: $(Get-RelativePath $repoRoot $req)"
  }
}

$doctrineFiles = @()
if (Test-Path $doctrineRoot) {
  $doctrineFiles = Get-ChildItem -Path $doctrineRoot -Recurse -File
}
$playbookFiles = @()
if (Test-Path $playbookRoot) {
  $playbookFiles = Get-ChildItem -Path $playbookRoot -Recurse -File
}

$trackedFiles = @($doctrineFiles + $playbookFiles) | Where-Object { $_ }

$state = @{
  timestamp = (Get-Date).ToUniversalTime().ToString("o")
  files = @()
}

foreach ($file in $trackedFiles) {
  try {
    $null = Get-Content -Path $file.FullName -Raw
    $relative = Get-RelativePath $repoRoot $file.FullName
    $state.files += @{
      path = $relative
      mtime_utc = $file.LastWriteTimeUtc.ToString("o")
      size = $file.Length
    }
  } catch {
    Add-Error "Failed to read file: $(Get-RelativePath $repoRoot $file.FullName)"
  }
}

if (Test-Path $statePath) {
  try {
    $previous = Get-Content -Path $statePath -Raw | ConvertFrom-Json
    $prevMap = @{}
    foreach ($entry in $previous.files) {
      $prevMap[$entry.path] = $entry
    }
    $changes = 0
    foreach ($entry in $state.files) {
      if (-not $prevMap.ContainsKey($entry.path)) {
        Write-Log "CHANGE: new file $($entry.path)"
        $changes++
        continue
      }
      $prev = $prevMap[$entry.path]
      if ($prev.mtime_utc -ne $entry.mtime_utc -or $prev.size -ne $entry.size) {
        Write-Log "CHANGE: updated file $($entry.path)"
        $changes++
      }
      $prevMap.Remove($entry.path) | Out-Null
    }
    foreach ($missing in $prevMap.Keys) {
      Write-Log "CHANGE: missing file $missing"
      $changes++
    }
    if ($changes -eq 0) {
      Write-Log "CHANGE: no changes detected"
    }
  } catch {
    Add-Warning "Failed to compare previous state: $($_.Exception.Message)"
  }
} else {
  Write-Log "CHANGE: no prior state found"
}

$state | ConvertTo-Json -Depth 4 | Set-Content -Path $statePath

$ymlMdFiles = $trackedFiles | Where-Object { $_.Name -like "*.yml.md" }
foreach ($file in $ymlMdFiles) {
  $jsonPath = "$($file.FullName).json"
  if (-not (Test-Path $jsonPath)) {
    Add-Warning "Missing JSON cache: $(Get-RelativePath $repoRoot $jsonPath)"
    continue
  }
  $jsonItem = Get-Item $jsonPath
  if ($jsonItem.LastWriteTimeUtc -lt $file.LastWriteTimeUtc) {
    Add-Warning "Stale JSON cache: $(Get-RelativePath $repoRoot $jsonPath)"
  }
}

Write-Log "STEP 4: Confirm formatting and response rules"
$formatFiles = @(
  (Join-Path $repoRoot "AGENTS.yml.md"),
  (Join-Path $doctrineRoot "Playbook.md"),
  (Join-Path $doctrineRoot "Prindiples-And-Protocols.yml.md")
)
$formatFound = $false
$maxLengthFound = $false
foreach ($file in $formatFiles) {
  if (-not (Test-Path $file)) {
    Add-Warning "Formatting rules file missing: $(Get-RelativePath $repoRoot $file)"
    continue
  }
  $text = Get-Content -Path $file -Raw
  $startWith = $text -match "start_with_numbered_list:\s*true"
  $numberedOnly = $text -match "numbered_lists_only:\s*true"
  $maxMatch = [regex]::Match($text, "list_item_max_length:\s*(\d+)")
  $maxValue = if ($maxMatch.Success) { $maxMatch.Groups[1].Value } else { "" }
  if ($startWith -or $numberedOnly -or $maxMatch.Success) {
    $formatFound = $true
  }
  if ($maxMatch.Success) {
    $maxLengthFound = $true
  }
  Write-Log "Format rules in $(Get-RelativePath $repoRoot $file):"
  Write-Log "start_with_numbered_list=$startWith"
  Write-Log "numbered_lists_only=$numberedOnly"
  if ($maxMatch.Success) {
    Write-Log "list_item_max_length=$maxValue"
  }
}

if (-not $formatFound) {
  Add-Error "No formatting rules found in doctrine/playbook."
}
if (-not $maxLengthFound) {
  Add-Warning "No line length rule found in doctrine/playbook."
}

Write-Log "STEP 5: Initialize tool mappings"
$toolsRoot = Join-Path $doctrineRoot "Tools"
if (-not (Test-Path $toolsRoot)) {
  Add-Warning "Tools directory missing: $(Get-RelativePath $repoRoot $toolsRoot)"
} else {
  $toolFiles = Get-ChildItem -Path $toolsRoot -Recurse -File -Filter "*.yml.md"
  foreach ($tool in $toolFiles) {
    $toolText = Get-Content -Path $tool.FullName -Raw
    $toolName = $null
    $titleMatch = [regex]::Match($toolText, "title:\s*""Tool:\s*([^""]+)""")
    if ($titleMatch.Success) {
      $toolName = $titleMatch.Groups[1].Value.Trim()
    } else {
      $headingMatch = [regex]::Match($toolText, "^#\s*Tool:\s*(.+)$", "Multiline")
      if ($headingMatch.Success) {
        $toolName = $headingMatch.Groups[1].Value.Trim()
      }
    }
    if (-not $toolName) {
      $toolName = $tool.BaseName
    }
    Write-Log "Tool mapping: $toolName => $(Get-RelativePath $repoRoot $tool.FullName)"
  }
}

if ($buildOk -and $dotnetPath -and (Test-Path $ffrwdCmd)) {
  $verifyResult = Invoke-LoggedCommand -Label "ffrwd --help" -Command @(
    $ffrwdCmd,
    "--help"
  )
  if ($verifyResult.ExitCode -ne 0) {
    Add-Warning "ffrwd help check failed."
  }
}

Write-Log "STEP 6: Validate startup state"
if ($errors.Count -gt 0) {
  Write-Log "Startup validation failed."
  foreach ($err in $errors) {
    Write-Log "ERROR: $err"
  }
} else {
  Write-Log "Startup validation passed."
}

$status = "success"
if ($errors.Count -gt 0) {
  $status = "errors"
} elseif ($warnings.Count -gt 0) {
  $status = "warnings"
}

Write-Log "STEP 7: Write status"
$statusPayload = @{
  timestamp = (Get-Date).ToUniversalTime().ToString("o")
  status = $status
  warnings = $warnings
  errors = $errors
  worktree_path = $worktreePath
}
$statusPayload | ConvertTo-Json -Depth 4 | Set-Content -Path $statusPath
Write-Log "Status: $status"
Write-Log "Status file: $statusPath"

Write-Log "STEP 8: Await user input"
Write-Log "Startup complete; ready for next action."

Copy-Item -Path $logPath -Destination $latestPath -Force

if ($errors.Count -gt 0) {
  exit 1
}
