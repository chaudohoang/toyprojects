@echo off
REM Removes the scheduled task. Does NOT stop a worker that is already running --
REM end FtpUpload.exe (and the cmd.exe running run_watchdog.bat) in Task Manager,
REM or reboot.
setlocal
set "TASKNAME=FTP Upload Worker"
set "KEEPNAME=FTP Upload Worker Keepalive"

schtasks /delete /tn "%TASKNAME%" /f
schtasks /delete /tn "%KEEPNAME%" /f

echo.
echo Tasks removed ^(errors above just mean they were not registered^).
echo A worker that is already running is NOT stopped by this - run stop_worker.bat
echo first, or end FtpUpload.exe in Task Manager.
pause
