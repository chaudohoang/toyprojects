@echo off
REM ==========================================================================
REM  stress_500.bat - one-shot REPEATABLE 500-panel run (quick pass).
REM
REM  Each double-click starts fresh:
REM    1) FULL clean (NO prompt): clears jobs / logs / state history + all test
REM       panels + scratch, so the deterministic TSN PIDs don't dedup against a
REM       previous run.
REM    2) Stage 500 mixed-scenario panels (1 KB files) into the config's real
REM       QueueFolder AND launch FtpUpload so you can watch them drain.
REM
REM  Does NOT modify config.json or the recipe. Set "SimulateFailurePercent" > 0
REM  in config.json first to also exercise the NG / recovery path.
REM
REM  Append args to override extras, e.g.:  stress_500.bat -FileKB 4
REM  (Do NOT pass -Panels / -Pct* here - they are set below and would conflict.)
REM ==========================================================================
echo.
echo === STRESS 500 ===========================================================
echo Step 1/2: FULL clean (no prompt)...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0clean_panels.ps1" -Full
echo.
echo Step 2/2: staging 500 panels (1 KB files) and launching...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0gen_panels.ps1" -Panels 500 -PctFull 35 -PctMissing 25 -PctJunk 10 -PctResume 25 -PctNotReady 5 %*
echo.
pause
