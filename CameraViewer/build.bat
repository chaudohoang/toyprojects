@echo off
setlocal
cd /d "%~dp0"

echo ============================================================
echo  Camera Viewer - build
echo ============================================================

where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] dotnet CLI not found. Install the .NET SDK from https://dot.net
    pause
    exit /b 1
)

dotnet build CameraViewer.csproj -c Release --nologo -v minimal
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [BUILD FAILED]  Check errors above.
    pause
    exit /b 1
)

echo.
echo Build OK.  Launching...
start "" "bin\Release\net48\CameraViewer.exe"
