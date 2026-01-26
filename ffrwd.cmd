@echo off
setlocal
set "repo_root=%~dp0"
pushd "%repo_root%" >nul

dotnet build "Sources/ffrwd/ffrwd.csproj" >nul
if errorlevel 1 (
  echo Error: dotnet build failed. 1>&2
  popd >nul
  exit /b 1
)

dotnet run --no-build --project "Sources/ffrwd/ffrwd.csproj" -- %*
set "exit_code=%ERRORLEVEL%"
popd >nul
exit /b %exit_code%
