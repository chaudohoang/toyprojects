@echo off
REM ==========================================================================
REM  test_rollover.bat - day-rollover regression test (see test_rollover.ps1).
REM
REM  Unlike stress_500.bat this drip-feeds panels INTO a running app so each
REM  simulated midnight happens with a file genuinely in flight - the condition
REM  the LGD "stops uploading after 00:00" stall needed.
REM
REM  Takes about (Days + 1.5) * DaySeconds to run; the default is ~3 minutes.
REM  Prints PASS / FAIL at the end and leaves the app running for inspection.
REM
REM  Override anything, e.g.:  test_rollover.bat -Days 5 -PanelsPerWave 25
REM ==========================================================================
echo.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0test_rollover.ps1" %*
pause
