@echo off
REM ---------------------------------------------------------------------------
REM Watchdog launcher for the FTP Upload Worker (spec 1 - auto-start + restart).
REM
REM Registered by install_task.bat; not meant to be run directly in production.
REM
REM The loop IS the watchdog: whenever FtpUpload.exe exits it is relaunched,
REM except for these deliberate cases signalled by its exit code:
REM     2 = another worker instance is already running (this loop is redundant)
REM     3 = STOP command was issued (intentional shutdown)
REM
REM NOTE: the pause uses ping, not "timeout". timeout fails instantly when stdout
REM is redirected - which is what Task Scheduler does - and the loop would then
REM spin at full CPU instead of waiting.
REM ---------------------------------------------------------------------------
setlocal
cd /d "%~dp0"

if not exist "%~dp0FtpUpload.exe" (
    echo [%date% %time%] FATAL: FtpUpload.exe not found in "%~dp0" - not looping.
    exit /b 1
)

:loop
echo [%date% %time%] starting FtpUpload.exe
FtpUpload.exe --hidden
set "CODE=%ERRORLEVEL%"

if "%CODE%"=="2" (
    echo [%date% %time%] another instance is already running - this watchdog exits
    goto :eof
)
if "%CODE%"=="3" (
    echo [%date% %time%] stop requested - this watchdog exits
    goto :eof
)

echo [%date% %time%] worker exited with code %CODE% - restarting in 5s
ping -n 6 127.0.0.1 >nul
goto loop
