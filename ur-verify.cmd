@echo off
REM ur-verify.cmd | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
setlocal

set "SCRIPT_DIR=%~dp0"
set "UR_ROOT=%MONOCOQUE_UR_ROOT%"
if "%UR_ROOT%"=="" set "UR_ROOT=%SCRIPT_DIR%..\[monocoque.ur]"

echo [verify] dotnet build (Release)
dotnet build "%SCRIPT_DIR%Ur.Tool\Ur.Tool.csproj" -c Release
if errorlevel 1 exit /b 1

echo [verify] dotnet test (Release)
dotnet test --project "%SCRIPT_DIR%Ur.Tool\Ur.Tool.csproj" -c Release
if errorlevel 1 exit /b 1

echo [verify] run verifier
dotnet run --project "%SCRIPT_DIR%Ur.Tool\Ur.Tool.csproj" -c Release -- --app verify all --ur-root "%UR_ROOT%"
if errorlevel 1 exit /b 1

echo [verify] OK
exit /b 0
