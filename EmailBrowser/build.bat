@echo off
echo ================================
echo   Building Email Browser...
echo ================================

dotnet build EmailBrowser.csproj -c Release

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Build failed!
    pause
    exit /b 1
)

echo.
echo ================================
echo   Build Successful!
echo ================================
echo Output: bin\Release\net48\EmailBrowser.exe
echo.

:: Copy mail_data.csv and icon to output folder if they exist
if exist mail_data.csv (
    copy /Y mail_data.csv bin\Release\net48\mail_data.csv >nul
    echo Copied mail_data.csv to output folder.
)

echo.
echo Done! Press any key to exit.
pause >nul
