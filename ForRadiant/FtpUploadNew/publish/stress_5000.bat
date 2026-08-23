@echo off
REM ==========================================================================
REM  stress_5000.bat - one-shot REPEATABLE stress run.
REM
REM  Each double-click starts completely fresh:
REM    1) FULL clean (NO prompt): clears jobs / logs / state history + all test
REM       panels + the scratch tree. This is what lets you re-run repeatedly --
REM       the generator uses deterministic TSN PIDs, so without a full clean the
REM       same-day dedup marks them "already resolved" and they look instantly
REM       done. The clean is the reason each run actually uploads fresh.
REM    2) Stage 5000 mixed-scenario panels (FULL/MISSING/JUNK/RESUME/NOTREADY)
REM       into the config's real QueueFolder. Files are 1 KB each (~50 MB scratch).
REM       Panels are STAGED (-NoLaunch); start FtpUpload.exe yourself to watch it drain.
REM
REM  Does NOT modify config.json or the recipe. To also inject upload FAILURES
REM  (-> NG list -> recovery under load), set "SimulateFailurePercent" > 0 in
REM  config.json before running.
REM
REM  Override extras by appending args, e.g.:  stress_5000.bat -FileKB 4
REM  (Do NOT pass -Panels or the -Pct* switches here -- they are set below and
REM   would conflict. Edit this file if you want a different mix/count.)
REM ==========================================================================
echo.
echo === STRESS 5000 ==========================================================
echo Step 1/2: FULL clean (no prompt)...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0clean_panels.ps1" -Full
echo.
echo Step 2/2: staging 5000 panels (1 KB files)...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0gen_panels.ps1" -Panels 5000 -PctFull 35 -PctMissing 25 -PctJunk 10 -PctResume 25 -PctNotReady 5 -NoLaunch %*
echo.
echo Done. Start FtpUpload.exe (or press Auto Upload) to watch the run drain.
pause
