@echo off
setlocal
chcp 65001 >nul
title Android Debug Console
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0Tools\AndroidDebug\Start.ps1" %*
set "launcherResult=%errorlevel%"
if not "%launcherResult%"=="0" pause
exit /b %launcherResult%
