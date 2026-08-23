@echo off
REM ---------------------------------------------------------------------------
REM Stops the worker DELIBERATELY, so the watchdog does not immediately restart it.
REM
REM Writes a STOP command into the StateFolder read from config.json (next to the
REM exe); the worker finishes what it is doing, exits with code 3, and its watchdog
REM loop ends quietly.
REM
REM NOTE: the keep-alive task will start it again within 5 minutes. To keep it
REM stopped for maintenance, run uninstall_task.bat as well.
REM ---------------------------------------------------------------------------
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0stop_worker.ps1" %*
pause
