@echo off
setlocal EnableDelayedExpansion

REM ===========================================================================
REM ResetTestSet.bat - wipe and regenerate the FTPRecovery test population.
REM
REM One click: removes every generated panel (queue files, dummy source files,
REM index/host files, recovery logs, backups) and builds a fresh set.
REM
REM Edit the variables below to change the shape of the test population.
REM ===========================================================================

REM ---- configuration --------------------------------------------------------
REM Optional arguments:  ResetTestSet.bat [panels] [dummyFileSizeKB]
REM   ResetTestSet.bat              ->  500 panels, 4 KB files   (normal testing)
REM   ResetTestSet.bat 5000 1       ->  5000 panels, 1 KB files  (scale testing)
REM 5000 panels is ~125,000 queue files and takes about 7 minutes to generate.
set "QUEUE=D:\Program\RVS\UploadQueue"
set "TEMPLATE=D:\Program\RVS\UploadQueueTemplate"
set "SRCROOT=E:\POCB\HEX\D994-CB-MP00_MATHON\08\19"
set "SCRIPT=%~dp0MakeTestQueues.ps1"

set "OLDPID=AAA"
set "COUNT=500"
set "SIZEKB=4"
if not "%~1"=="" set "COUNT=%~1"
if not "%~2"=="" set "SIZEKB=%~2"

REM Random scenario mix. Each panel gets one of six real-world states so the
REM recovery tool is exercised on all of them. Weights need not sum to 100.
REM   FRESH      no host/index at all, every queue file present
REM   PARTIAL    host has clean lines, those queue files gone (classic stall)
REM   DUP        host has clean lines, queue files STILL there (dedupe test)
REM   RETRY      host has " - failed" lines, queue files STILL there (retry test)
REM   ORPHANFAIL host has clean + failed lines, those queue files gone
REM   INCOMPLETE queue files vanished, nothing recorded (needs -force)
set "RANDOMIZE=1"
set "SEED=20260819"
set "P_FRESH=20"
set "P_PARTIAL=25"
set "P_DUP=15"
set "P_RETRY=20"
set "P_ORPHAN=10"
set "P_INCOMPLETE=10"
set "P_MISSINGSRC=15"

REM Used only when RANDOMIZE=0 - fixed seeding of a contiguous subset.
set "SEEDFROM=401"
set "SEEDCOUNT=100"
set "SEEDOK=18"
set "SEEDFAIL=2"

REM Set to 1 to also drop the original AAA sample from the queue folder.
REM It stays preserved in %TEMPLATE% either way.
set "REMOVE_AAA=0"
REM ---------------------------------------------------------------------------

REM ---- prerequisite checks (before the destructive prompt) ------------------
set "MISSING="

if not exist "%SCRIPT%" (
    echo MISSING: %SCRIPT%
    echo          Copy MakeTestQueues.ps1 into the same folder as this .bat.
    set "MISSING=1"
)
if not exist "%TEMPLATE%\*.txt" (
    echo MISSING: %TEMPLATE%
    echo          This folder must hold the 25 template queue .txt files
    echo          for one panel whose local PID is "%OLDPID%".
    set "MISSING=1"
)
if not exist "%QUEUE%\" (
    echo NOTE   : %QUEUE% does not exist yet - it will be created.
)

if defined MISSING (
    echo.
    echo Cannot continue. This tool needs, side by side:
    echo    ResetTestSet.bat
    echo    MakeTestQueues.ps1
    echo and the template folder at %TEMPLATE%
    echo.
    exit /b 1
)

echo.
echo  Queue folder : %QUEUE%
echo  Template     : %TEMPLATE%
echo  Source root  : %SRCROOT%
if "%RANDOMIZE%"=="1" (
    echo  Panels       : %COUNT%  random scenario mix, seed %SEED%
) else (
    echo  Panels       : %COUNT%  ^(%SEEDCOUNT% stalled from #%SEEDFROM%^)
)
echo.
echo  This DELETES all generated test data and rebuilds it.
echo.
choice /C YN /M "Proceed"
if errorlevel 2 goto :eof

echo.
echo [1/2] Cleaning previous test data ...

REM generated queue files
del /q "%QUEUE%\TSTPID*.txt" 2>nul

REM dummy source files + index/host files created during recovery runs
for /d %%D in ("%SRCROOT%\TSTPID*") do rd /s /q "%%D" 2>nul

REM per-panel WinSCP session logs
for /d %%D in ("%QUEUE%\Log\WinSCPLog\TSTPID*") do rd /s /q "%%D" 2>nul

REM recovery logs, reports and queue backups from earlier runs
rd /s /q "%QUEUE%\Log\Recovery" 2>nul
rd /s /q "%QUEUE%\Backedup Recovery Queue" 2>nul
rd /s /q "%QUEUE%\Backedup Succeed Queue" 2>nul
rd /s /q "%QUEUE%\Backedup Failed Queue" 2>nul

REM fail counters left behind by FTPUploaderVB
del /q "%QUEUE%\Fail Count\TSTPID*.txt" 2>nul
del /q "%QUEUE%\Fail Count\IndexHost\TSTPID*.txt" 2>nul

if "%REMOVE_AAA%"=="1" (
    echo       removing original AAA sample from queue folder
    del /q "%QUEUE%\%OLDPID%*.txt" 2>nul
)

echo       done.

echo.
if "%RANDOMIZE%"=="1" (
    echo [2/2] Generating %COUNT% panels with a random scenario mix ...
    powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT%" ^
        -Template "%TEMPLATE%" -Root "%QUEUE%" -OldPid %OLDPID% ^
        -StartIndex 1 -Count %COUNT% -FileSizeKB %SIZEKB% ^
        -Random -RandomSeed %SEED% ^
        -PctFresh %P_FRESH% -PctPartial %P_PARTIAL% -PctDup %P_DUP% ^
        -PctRetry %P_RETRY% -PctOrphanFail %P_ORPHAN% -PctIncomplete %P_INCOMPLETE% ^
        -PctMissingSource %P_MISSINGSRC% -Go
    if errorlevel 1 goto :failed
    goto :report
)

echo [1/2] Generating %COUNT% fresh panels ...
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT%" ^
    -Template "%TEMPLATE%" -Root "%QUEUE%" -OldPid %OLDPID% ^
    -StartIndex 1 -Count %COUNT% -FileSizeKB %SIZEKB% -Go
if errorlevel 1 goto :failed

echo.
if "%SEEDCOUNT%"=="0" (
    echo [2/2] No stalled subset requested - all panels are fresh.
) else (
    echo [2/2] Converting panels #%SEEDFROM%..  into stalled state ...
    powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT%" ^
        -Template "%TEMPLATE%" -Root "%QUEUE%" -OldPid %OLDPID% ^
        -StartIndex %SEEDFROM% -Count %SEEDCOUNT% -FileSizeKB %SIZEKB% ^
        -SeedRecorded %SEEDOK% -SeedFailed %SEEDFAIL% -Go
    if errorlevel 1 goto :failed
)

:report

echo.
echo ==========================================================
echo  TEST SET READY
echo ==========================================================
for /f %%C in ('dir /b "%QUEUE%\TSTPID*.txt" 2^>nul ^| find /c /v ""') do echo  queue files  : %%C
for /f %%C in ('dir /b /ad "%SRCROOT%\TSTPID*" 2^>nul ^| find /c /v ""') do echo  panel folders: %%C
echo.
echo  Next:  FTPRecovery.exe -root "%QUEUE%"          (dry run)
echo         FTPRecovery.exe -root "%QUEUE%" -go      (execute)
echo.
goto :eof

:failed
echo.
echo GENERATION FAILED - see messages above.
exit /b 1
