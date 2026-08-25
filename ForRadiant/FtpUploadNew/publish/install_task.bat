@echo off
REM ---------------------------------------------------------------------------
REM Registers the FTP Upload Worker with Windows Task Scheduler, then (re)starts it.
REM Auto-elevates to administrator (UAC prompt) - schtasks needs admin rights.
REM
REM TWO tasks are created:
REM   "FTP Upload Worker"           at log on          - normal start
REM   "FTP Upload Worker Keepalive" every 5 minutes    - revives it if the watchdog died
REM
REM Re-run after copying a new exe: it stops the old worker + watchdog and starts fresh.
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
set "TARGET=%~dp0run_hidden.vbs"

if not exist "%~dp0FtpUpload.exe" (
    echo ERROR: FtpUpload.exe not found next to this script.
    echo Copy the deployment scripts into the same folder as the published exe.
    pause
    exit /b 1
)

echo Registering "%TASKNAME%" ^(at log on^)...
schtasks /create /tn "%TASKNAME%" /tr "wscript.exe \"%TARGET%\"" /sc onlogon /f
if %ERRORLEVEL% NEQ 0 goto :failed

echo Registering "%KEEPNAME%" ^(every 5 minutes^)...
schtasks /create /tn "%KEEPNAME%" /tr "wscript.exe \"%TARGET%\"" /sc minute /mo 5 /f
if %ERRORLEVEL% NEQ 0 goto :failed

echo.
echo Stopping any worker that is already running (and its watchdog)...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0_killworker.ps1"
REM give the old instance a moment to release its single-instance mutex before starting fresh
ping -n 3 127.0.0.1 >nul

echo Starting the worker now (VISIBLE this first time - it runs hidden from the next log on)...
REM First start is deliberately NOT hidden so you can see it came up. Launch the exe with a window
REM (no --hidden). It holds the single-instance mutex, so the hidden log-on/keepalive tasks will
REM just exit while it runs; after a reboot the log-on task starts it hidden as normal.
start "" "%~dp0FtpUpload.exe"

echo.
echo Done. The worker is now RUNNING (a window should have opened), and will:
echo   - start automatically at log on (hidden, to the tray)
echo   - be restarted within 5s by the watchdog loop if it crashes
echo   - be revived within 5 minutes even if the watchdog itself is killed
echo.
echo Stop it deliberately:         stop_worker.bat
echo Remove both tasks + stop it:  uninstall_task.bat
echo.
pause
exit /b 0

:failed
echo.
echo Failed to register tasks.
pause
exit /b 1
