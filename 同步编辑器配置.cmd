@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Tools\EditorConfig\Sync.ps1"
if errorlevel 1 echo Synchronization failed. Check the error above.
pause
