param(
  [string]$Version,
  [string]$InstallDir
)

function Get-SdkVersionFromGlobalJson {
  param([string]$Path)
  if (-not (Test-Path $Path)) {
    return $null
  }

  try {
    $json = Get-Content $Path -Raw | ConvertFrom-Json
    return $json.sdk.version
  } catch {
    return $null
  }
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptDir "..")).Path
$globalJson = Join-Path $repoRoot "global.json"

if (-not $Version) {
  $Version = Get-SdkVersionFromGlobalJson -Path $globalJson
}

if (-not $Version) {
  Write-Error "Error: .NET SDK version not provided."
  exit 2
}

if (-not $InstallDir) {
  $InstallDir = $env:DOTNET_INSTALL_DIR
}

if (-not $InstallDir) {
  $InstallDir = Join-Path $HOME ".dotnet"
}

$installScript = Join-Path $env:TEMP "dotnet-install.ps1"
Write-Output "Installing .NET SDK $Version to $InstallDir"

Invoke-WebRequest `
  -Uri "https://dot.net/v1/dotnet-install.ps1" `
  -OutFile $installScript `
  -UseBasicParsing

& powershell -NoProfile -ExecutionPolicy Bypass `
  -File $installScript `
  -Version $Version `
  -InstallDir $InstallDir

$pathEntries = $env:PATH -split ';'
if ($pathEntries -notcontains $InstallDir) {
  Write-Output "Add to PATH: setx PATH `"$InstallDir;$env:PATH`""
}

$dotnetExe = Join-Path $InstallDir "dotnet.exe"
Write-Output "dotnet version:"
if (Test-Path $dotnetExe) {
  & $dotnetExe --version
} else {
  Write-Error "dotnet not found in $InstallDir."
  exit 4
}
