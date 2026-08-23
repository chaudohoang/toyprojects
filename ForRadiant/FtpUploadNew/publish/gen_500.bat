@echo off
REM ==========================================================================
REM  Build 500 sample .panel handoffs across the mixed scenario set
REM  (FULL / MISSING / JUNK / RESUME / NOTREADY) into the config's real
REM  QueueFolder, then launch the app to watch them ingest/upload.
REM
REM  Weighting leans toward the recovery-relevant cases (partial manifests,
REM  missing files) so the NG / recovery path gets exercised, similar to the
REM  situations FTPRecovery was built to handle.
REM
REM  Does NOT modify config.json or the recipe. To also simulate upload
REM  FAILURES (-> NG list -> recovery), set "SimulateFailurePercent" in
REM  config.json before running (it is a test-only knob, not in the UI).
REM
REM  Pass extra args to override, e.g.:  gen_500.bat -NoLaunch
REM ==========================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0gen_panels.ps1" -Panels 500 -PctFull 35 -PctMissing 25 -PctJunk 10 -PctResume 25 -PctNotReady 5 %*
echo.
pause
