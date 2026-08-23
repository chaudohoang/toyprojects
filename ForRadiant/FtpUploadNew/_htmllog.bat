@echo off
REM ==========================================================================
REM  Double-click to build an HTML report of TODAY's upload log and open it.
REM  From a terminal you can pass a day:  _htmllog.bat 20260816
REM  It reads config.json (next to this file) to find the log folder.
REM ==========================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0_htmllog.ps1" %*
if errorlevel 1 pause
