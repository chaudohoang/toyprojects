@echo off
setlocal

echo ============================================
echo  Credential Manager -- Release Build
echo ============================================
echo.

:: Find dotnet on PATH
where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: dotnet CLI not found on PATH.
    echo Install the .NET SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

:: Clean previous output
echo [1/3] Cleaning previous build...
dotnet clean CredentialManager.csproj -c Release -v quiet
if errorlevel 1 ( echo Clean failed. & pause & exit /b 1 )

:: Restore NuGet packages
echo [2/3] Restoring packages...
dotnet restore CredentialManager.csproj -v quiet
if errorlevel 1 ( echo Restore failed. & pause & exit /b 1 )

:: Build release
echo [3/3] Building Release...
dotnet publish CredentialManager.csproj ^
    -c Release ^
    --no-restore ^
    -o publish\
if errorlevel 1 ( echo Build failed. & pause & exit /b 1 )

echo.
echo ============================================
echo  Done!  Output is in: publish\
echo ============================================
echo.

:: Open the output folder
start "" "publish\"
pause
