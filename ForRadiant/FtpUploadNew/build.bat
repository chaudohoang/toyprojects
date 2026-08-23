@echo off
REM ==========================================================================
REM  build.bat - one-click build/publish for FtpUpload (self-contained, single file).
REM
REM     build.bat          publish self-contained single file -> .\publish  (default)
REM     build.bat run      ...then launch the manager window
REM     build.bat fdd      framework-dependent build (needs .NET 8 Desktop Runtime)
REM     build.bat clean    delete bin\ obj\ publish\ and exit
REM
REM  Requires the .NET 8 SDK (Windows Desktop workload). WPF will NOT build on a
REM  plain SDK or on Linux.
REM ==========================================================================
setlocal EnableDelayedExpansion
cd /d "%~dp0"

set "MODE=%~1"
set "OUT=publish"

if not exist "%~dp0FtpUpload.csproj" (
    echo ERROR: FtpUpload.csproj not found next to this script.
    pause
    exit /b 1
)

where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: 'dotnet' not found on PATH. Install the .NET 8 SDK ^(Windows Desktop workload^).
    pause
    exit /b 1
)

if /i "%MODE%"=="clean" (
    echo Cleaning bin\ obj\ %OUT%\ ...
    if exist "bin"   rmdir /s /q "bin"
    if exist "obj"   rmdir /s /q "obj"
    if exist "%OUT%" rmdir /s /q "%OUT%"
    echo Done.
    pause
    exit /b 0
)

REM --- stop anything that could hold the exe open --------------------------------
REM Disable + end the scheduled tasks first so the 5-minute keep-alive can't respawn
REM the worker mid-build. (These no-op silently on a dev box with no tasks installed.)
schtasks /change /tn "FTP Upload Worker" /disable          >nul 2>&1
schtasks /change /tn "FTP Upload Worker Keepalive" /disable >nul 2>&1
schtasks /end    /tn "FTP Upload Worker"                   >nul 2>&1
schtasks /end    /tn "FTP Upload Worker Keepalive"          >nul 2>&1

taskkill /f /im FtpUpload.exe >nul 2>&1

REM wait until the process is really gone so the file lock is released
for /L %%i in (1,1,15) do (
    tasklist /fi "imagename eq FtpUpload.exe" 2>nul | find /i "FtpUpload.exe" >nul || goto :released
    ping -n 2 127.0.0.1 >nul
)
:released

REM Delete the old single-file exe BEFORE publishing. Overwriting a single-file exe
REM in place is what produces the "rebuilt exe won't start" corruption - a fresh write
REM avoids it. This automates the manual delete.
if exist "%OUT%\FtpUpload.exe" del /f /q "%OUT%\FtpUpload.exe" >nul 2>&1
if exist "%OUT%\FtpUpload.exe" (
    echo.
    echo *** ERROR: FtpUpload.exe is still locked. ***
    echo Close it fully ^(tray -^> exit^) or stop the watchdog, then re-run.
    call :reenable
    pause
    exit /b 1
)

echo.
echo === Restoring + building (Release) ========================================
if /i "%MODE%"=="fdd" (
    echo Framework-dependent build ^(target PC needs .NET 8 Desktop Runtime^).
    dotnet publish -c Release --self-contained false -p:PublishSingleFile=true -o "%OUT%"
) else (
    echo Self-contained single-file build ^(nothing to install on the target PC^).
    dotnet publish -c Release -p:PublishSingleFile=true -o "%OUT%"
)
set "RC=%ERRORLEVEL%"

call :reenable

if %RC% NEQ 0 (
    echo.
    echo *** BUILD FAILED - see the messages above. ***
    pause
    exit /b 1
)

echo.
echo === Build OK ==============================================================

REM Seed a good starting config + recipe ONLY when they are missing (e.g. a fresh
REM publish, or after a full clean). Never overwrites an existing config/recipe, so
REM your edits and the in-app Settings changes are safe.
if not exist "%OUT%\config.json" (
    if exist "config.default.json" (
        copy /y "config.default.json" "%OUT%\config.json" >nul
        echo Seeded %OUT%\config.json from config.default.json ^(known-good values^).
    )
)
if not exist "%OUT%\allowed_filenames.txt" (
    if exist "allowed_filenames.txt" (
        copy /y "allowed_filenames.txt" "%OUT%\allowed_filenames.txt" >nul
        echo Seeded %OUT%\allowed_filenames.txt ^(upload recipe^).
    )
)

REM Keep the runnable helper scripts next to the exe/config so the copy you run is
REM always current (they read config.json from their own folder).
for %%S in (config.default.json gen_panels.ps1 gen_panels.bat gen_500.bat gen_5000.bat stress_500.bat stress_5000.bat clean_panels.ps1 clean_panels.bat clean_panels_full.bat _htmllog.ps1 _htmllog.bat _nghtmllog.ps1 _nghtmllog.bat _progress.ps1 run_watchdog.bat run_hidden.vbs install_task.bat uninstall_task.bat stop_worker.bat stop_worker.ps1) do (
    if exist "%%S" copy /y "%%S" "%OUT%\" >nul
)

echo Output: "%~dp0%OUT%\FtpUpload.exe"
echo The window header shows this exe's build time - check it matches to confirm
echo you are running the fresh build.
echo.

if /i "%MODE%"=="run" (
    echo Launching the manager window...
    start "" "%~dp0%OUT%\FtpUpload.exe" --show
)

pause
exit /b 0

:reenable
REM restore the scheduled tasks if they exist (no-op on a dev box)
schtasks /change /tn "FTP Upload Worker" /enable          >nul 2>&1
schtasks /change /tn "FTP Upload Worker Keepalive" /enable >nul 2>&1
goto :eof
