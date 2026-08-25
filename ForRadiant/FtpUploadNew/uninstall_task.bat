@echo off
REM ---------------------------------------------------------------------------
REM Removes the scheduled tasks AND stops a running worker (including its watchdog,
REM so it is not relaunched). Auto-elevates to administrator (UAC prompt).
REM ---------------------------------------------------------------------------

REM --- self-elevate to administrator if we are not already ---
net session >nul 2>&1
if %errorlevel% NEQ 0 (
    echo Requesting administrator privileges...
    powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    exit /b
)

setlocal
set "TASKNAME=FTP Upload Worker"
set "KEEPNAME=FTP Upload Worker Keepalive"

echo Removing scheduled tasks...
schtasks /delete /tn "%TASKNAME%" /f
schtasks /delete /tn "%KEEPNAME%" /f

echo.
echo Stopping the running worker (and its watchdog)...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0_killworker.ps1"

echo.
echo Done. Tasks removed and the worker is stopped.
echo (Any "ERROR: task does not exist" above just means it was not registered.)
pause
