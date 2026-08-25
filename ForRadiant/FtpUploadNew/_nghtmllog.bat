@echo off
REM ==========================================================================
REM  Double-click -> opens a CALENDAR; the days that have an NG-retry log are
REM  shown in bold. Pick a day to build + open its HTML NG report.
REM  From a terminal you can still pass a day:  _nghtmllog.bat 20260816
REM  It reads config.json (next to this file) to find the log folder.
REM ==========================================================================
if "%~1"=="" (
    start "" powershell -NoProfile -ExecutionPolicy Bypass -STA -WindowStyle Hidden -File "%~dp0_logpick.ps1" -Kind ng
) else (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0_nghtmllog.ps1" %*
    if errorlevel 1 pause
)
