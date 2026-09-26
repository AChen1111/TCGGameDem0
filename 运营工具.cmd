@echo off
setlocal
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0Tools\OperationsCli\Start.ps1" %*
set "launcherResult=%errorlevel%"
if not "%launcherResult%"=="0" pause
exit /b %launcherResult%
