@echo off
REM ==========================================================================
REM  Build 5000 sample .panel handoffs across the mixed scenario set
REM  (FULL / MISSING / JUNK / RESUME / NOTREADY) into the config's real
REM  QueueFolder. This is a STRESS set -- it defaults to -NoLaunch so the
REM  files are staged first; start FtpUpload yourself when ready, or pass
REM  -Launch... (append your own args) to override.
REM
REM  Weighting leans toward the recovery-relevant cases (partial manifests,
REM  missing files), similar to the situations FTPRecovery handled.
REM
REM  Does NOT modify config.json or the recipe. To also simulate upload
REM  FAILURES (-> NG list -> recovery), set "SimulateFailurePercent" in
REM  config.json before running.
REM
REM  Note: 5000 panels x ~10 files each is ~50,000 fake files in the scratch
REM  tree (D:\FtpUploadDemo\src). Default file size is 1 KB (~50 MB total); pass
REM  -FileKB 4 (etc.) to make them bigger.
REM ==========================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0gen_panels.ps1" -Panels 5000 -PctFull 35 -PctMissing 25 -PctJunk 10 -PctResume 25 -PctNotReady 5 -NoLaunch %*
echo.
pause
