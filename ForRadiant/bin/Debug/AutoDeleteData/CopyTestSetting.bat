@echo off
setlocal enabledelayedexpansion
set sourceFolder="%~dp0TestSetting"
set destinationFolder="C:\Radiant Vision Systems Data\TrueTest\UserData\AutoDelete\"
xcopy /Y /S /E %sourceFolder% %destinationFolder%

for /l %%i in (3,-1,1) do (
    echo Closing in %%i seconds...
    timeout /t 1 >nul
)