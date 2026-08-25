@echo off
REM ==========================================================================
REM  Double-click -> opens a CALENDAR; the days that have a log are shown in
REM  bold (like the in-app view). Pick a day to build + open its HTML report.
REM  From a terminal you can still pass a day:  _htmllog.bat 20260816
REM  It reads config.json (next to this file) to find the log folder.
REM ==========================================================================
if "%~1"=="" (
    start "" powershell -NoProfile -ExecutionPolicy Bypass -STA -WindowStyle Hidden -File "%~dp0_logpick.ps1" -Kind day
) else (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0_htmllog.ps1" %*
    if errorlevel 1 pause
)
