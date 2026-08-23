@echo off
REM ---------------------------------------------------------------------------
REM Registers the FTP Upload Worker with Windows Task Scheduler.
REM
REM TWO tasks are created, giving two layers of protection:
REM
REM   "FTP Upload Worker"           at log on          - normal start
REM   "FTP Upload Worker Keepalive" every 5 minutes    - revives it if the
REM                                                       watchdog itself died
REM
REM The keep-alive is safe to fire while everything is healthy: the worker holds
REM a global mutex, so a redundant copy exits with code 2 and its watchdog loop
REM stops immediately without uploading anything.
REM
REM Run ONCE per inspection PC. Re-run after moving the folder.
REM Right-click "Run as administrator" if you get Access Denied.
REM ---------------------------------------------------------------------------
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
echo Done. The worker will:
echo   - start automatically at log on
echo   - be restarted within 5s by the watchdog loop if it crashes
echo   - be revived within 5 minutes even if the watchdog itself is killed
echo.
echo Start now without rebooting:  schtasks /run /tn "%TASKNAME%"
echo Stop it deliberately:         stop_worker.bat
echo Remove both tasks:            uninstall_task.bat
echo.
pause
exit /b 0

:failed
echo.
echo Failed to register tasks. Try running this file as administrator.
pause
exit /b 1
