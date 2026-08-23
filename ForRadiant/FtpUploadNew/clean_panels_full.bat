@echo off
REM ==========================================================================
REM  clean_panels_full.bat - FULL clean:
REM    * removes all synthetic TEST panels (TSN<digits>_<stamp>) from the
REM      queue + ingest backup, and wipes the scratch source tree
REM    * ALSO clears the app's jobs / logs / state / processed folders
REM      (this erases ALL of today's history, so _history starts empty and a
REM       re-gen of the same PIDs will upload fresh)
REM
REM  It never touches the publish folder, the exe, config.json, or the scripts
REM  (the script has a hard safety guard against clearing the exe folder).
REM
REM  Real production panels are never matched. Use -DryRun to preview.
REM ==========================================================================
echo.
echo  FULL CLEAN - this will erase today's jobs/logs/state history as well as
echo  the test panels. Real production panels are NOT touched.
echo.
choice /m "Proceed with full clean"
if errorlevel 2 (
    echo Cancelled.
    pause
    exit /b 0
)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0clean_panels.ps1" -Full %*
