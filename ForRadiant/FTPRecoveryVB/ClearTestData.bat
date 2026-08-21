@echo off
setlocal

REM ===========================================================================
REM ClearTestData.bat - remove ALL generated test data and run output.
REM
REM Deletes everything the test generator and the recovery tool have produced,
REM leaving a clean machine. Does NOT create a new test set - use
REM ResetTestSet.bat for that.
REM
REM What it does NOT touch:
REM   - the template folder (D:\Program\RVS\UploadQueueTemplate)
REM   - the program itself, or allowed/denied_filenames.txt
REM   - any real (non-TSTPID) queue files
REM ===========================================================================

set "QUEUE=D:\Program\RVS\UploadQueue"
set "SRCROOT=E:\POCB\HEX\D994-CB-MP00_MATHON\08\19"
set "BIN=%~dp0bin"

echo.
echo  Queue folder : %QUEUE%
echo  Source root  : %SRCROOT%
echo  Program dir  : %BIN%
echo.
echo  This removes ALL generated test panels, dummy images, logs, reports
echo  and backups. Real (non-TSTPID) queue files are left alone.
echo.
choice /C YN /M "Delete all test data"
if errorlevel 2 goto :eof

echo.
echo [1/5] generated queue files ...
del /q "%QUEUE%\TSTPID*.txt" 2>nul

echo [2/5] dummy images and per-panel folders ...
for /d %%D in ("%SRCROOT%\TSTPID*") do rd /s /q "%%D" 2>nul

echo [3/5] backups and fail counters ...
rd /s /q "%QUEUE%\Backedup Recovery Queue" 2>nul
rd /s /q "%QUEUE%\Backedup Succeed Queue"  2>nul
rd /s /q "%QUEUE%\Backedup Failed Queue"   2>nul
rd /s /q "%BIN%\Backedup Recovery Queue"   2>nul
del /q "%QUEUE%\Fail Count\TSTPID*.txt" 2>nul
del /q "%QUEUE%\Fail Count\IndexHost\TSTPID*.txt" 2>nul

echo [4/5] logs and reports ...
rd /s /q "%BIN%\Log"            2>nul
rd /s /q "%QUEUE%\Log\Recovery" 2>nul
for /d %%D in ("%QUEUE%\Log\WinSCPLog\TSTPID*") do rd /s /q "%%D" 2>nul

echo [5/5] learned filename cache ...
REM allowed_filenames.txt / denied_filenames.txt are yours - left alone.
del /q "%BIN%\known_filenames.txt"   2>nul
del /q "%QUEUE%\known_filenames.txt" 2>nul

echo.
echo ==========================================================
echo  CLEARED
echo ==========================================================
for /f %%C in ('dir /b "%QUEUE%\TSTPID*.txt" 2^>nul ^| find /c /v ""') do echo  test queue files left : %%C
for /f %%C in ('dir /b /ad "%SRCROOT%\TSTPID*" 2^>nul ^| find /c /v ""') do echo  test panel folders    : %%C
for /f %%C in ('dir /b "%QUEUE%\*.txt" 2^>nul ^| find /c /v ""') do echo  other .txt in queue   : %%C
echo.
echo  Run ResetTestSet.bat to build a fresh test set.
echo.
endlocal
