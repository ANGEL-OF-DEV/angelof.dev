@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass ^
  -File "%~dp0agent-startup.ps1" %*
exit /b %ERRORLEVEL%
