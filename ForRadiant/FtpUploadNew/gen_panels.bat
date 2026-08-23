@echo off
REM ==========================================================================
REM  Double-click to build 100 sample .panel handoffs (+ fake source files)
REM  across a mix of scenarios and launch the app to watch them ingest/upload.
REM  From a terminal you can pass args, e.g.:  gen_panels.bat -Panels 250 -FailPercent 50
REM  Add -NoLaunch to build the test set without starting FtpUpload.
REM ==========================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0gen_panels.ps1" %*
echo.
pause
