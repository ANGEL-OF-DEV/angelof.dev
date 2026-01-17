@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass ^
  -File "%~dp0agent-start.ps1" %*
exit /b %ERRORLEVEL%
