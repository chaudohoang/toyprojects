@echo off
REM Double-click for today's NG-retry HTML report; or:  _nghtmllog.bat 20260816
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0_nghtmllog.ps1" %*
if errorlevel 1 pause
