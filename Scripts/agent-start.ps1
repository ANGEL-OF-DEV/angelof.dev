param(
  [Parameter(Mandatory = $true)]
  [string]$Model,
  [Parameter(ValueFromRemainingArguments = $true)]
  [string[]]$Command
)

if ($Command.Count -gt 0 -and $Command[0] -eq "--") {
  $Command = $Command[1..($Command.Count - 1)]
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptDir "..")).Path

Set-Location $repoRoot

dotnet build "Sources/ffrwd/ffrwd.csproj" | Out-Null
$worktreePath = & dotnet run --no-build --project `
  "Sources/ffrwd/ffrwd.csproj" -- agent start $Model --emit=path

if ([string]::IsNullOrWhiteSpace($worktreePath)) {
  Write-Error "Error: failed to resolve worktree path."
  exit 1
}

Set-Location $worktreePath

if ($Command.Count -gt 0) {
  & $Command
  exit $LASTEXITCODE
}
