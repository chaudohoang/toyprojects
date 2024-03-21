@echo off
:: BatchGotAdmin
:-------------------------------------
REM  --> Check for permissions
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"

REM --> If error flag set, we do not have admin.
if '%errorlevel%' NEQ '0' (
    echo Requesting administrative privileges...
    goto UACPrompt
) else ( goto gotAdmin )

:UACPrompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    echo UAC.ShellExecute "%~s0", "", "", "runas", 1 >> "%temp%\getadmin.vbs"

    "%temp%\getadmin.vbs"
    del "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    pushd "%CD%"
    CD /D "%~dp0"
:--------------------------------------

@setlocal enableextensions

echo Deleting startup task...
schtasks /delete /TN autodelete /F
schtasks /delete /TN autodelete_restart /F
echo Done.

echo Modifying settingAutoDeleteData.txt...
(
    echo startminimized=false
    echo minimizedtotray=false
    echo monitorautomatically=false
) > "%~dp0settingAutoDeleteData.txt"
echo Done modifying settingAutoDeleteData.txt.

echo Killing previous instances of AutoDeleteData.exe...
taskkill /f /im AutoDeleteData.exe >nul 2>nul

for /l %%i in (3,-1,1) do (
    echo Closing in %%i seconds...
    timeout /t 1 >nul
)


