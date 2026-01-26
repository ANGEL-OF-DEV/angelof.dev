@echo off
REM prompts-verify.cmd | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
setlocal

set "SCRIPT_DIR=%~dp0"
set "PROMPTS_ROOT=%MONOCOQUE_PROMPTS_ROOT%"
if "%PROMPTS_ROOT%"=="" set "PROMPTS_ROOT=%SCRIPT_DIR%..\[monocoque.prompts]"

echo [verify] dotnet build (Release)
dotnet build "%SCRIPT_DIR%PromptPack.Verify\PromptPack.Verify.csproj" -c Release
if errorlevel 1 exit /b 1

echo [verify] dotnet test (Release)
dotnet test --project "%SCRIPT_DIR%PromptPack.Verify\PromptPack.Verify.csproj" -c Release
if errorlevel 1 exit /b 1

echo [verify] run verifier
dotnet run --project "%SCRIPT_DIR%PromptPack.Verify\PromptPack.Verify.csproj" -c Release -- --app verify-pack --prompts-root "%PROMPTS_ROOT%"
if errorlevel 1 exit /b 1

echo [verify] OK
exit /b 0
