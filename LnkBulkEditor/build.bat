@echo off
setlocal

echo ============================================================
echo  LNK Bulk Editor — build
echo ============================================================

:: Try dotnet CLI first (VS 2022 / .NET SDK)
where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] dotnet CLI not found.
    echo         Install the .NET SDK from https://dot.net
    pause
    exit /b 1
)

echo Building Release...
dotnet build LnkBulkEditor.csproj -c Release --nologo -v minimal

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [BUILD FAILED]  Check errors above.
    pause
    exit /b 1
)

echo.
echo Build OK.  Launching...
echo.
start "" "bin\Release\net48\LnkBulkEditor.exe"
