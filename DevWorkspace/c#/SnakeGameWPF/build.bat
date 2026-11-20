@echo off
setlocal enabledelayedexpansion
cd /d "%~dp0"

set "CONFIG=Release"
set "RUNTIME=win-x64"
set "PROJECT=SnakeGameWPF.csproj"
set "APPNAME=SnakeGameWPF"

REM NOTE: do not name this variable OUTDIR - MSBuild picks up OutDir from the
REM environment (env vars are case-insensitive) and redirects the whole build.
set "DISTDIR=dist"

echo ================================
echo  %APPNAME% - Build
echo ================================
echo.

where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK not found on PATH.
    echo Install it from https://dotnet.microsoft.com/download
    goto :fail
)

echo [1/3] Publishing self-contained single file...
echo.
dotnet publish "%PROJECT%" -c %CONFIG% -r %RUNTIME% --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none -p:SatelliteResourceLanguages=en
if errorlevel 1 goto :fail

echo.
echo [2/3] Locating output...
set "BUILT="
for /f "delims=" %%F in ('dir /b /s "bin\%CONFIG%\%APPNAME%.exe" 2^>nul ^| findstr /i "\\publish\\"') do set "BUILT=%%F"
if not defined BUILT (
    echo ERROR: could not find published %APPNAME%.exe under bin\%CONFIG%
    goto :fail
)

echo [3/3] Copying to %DISTDIR%\ ...
if not exist "%DISTDIR%" mkdir "%DISTDIR%"
copy /y "!BUILT!" "%DISTDIR%\%APPNAME%.exe" >nul
if errorlevel 1 goto :fail

set "SIZEMB=?"
for %%S in ("%DISTDIR%\%APPNAME%.exe") do set /a SIZEMB=%%~zS/1048576

echo.
echo ================================
echo  Build complete
echo  %~dp0%DISTDIR%\%APPNAME%.exe  (!SIZEMB! MB)
echo ================================
echo.
echo Self-contained - runs on any Windows x64 machine with no
echo .NET runtime installed. For a ~1 MB exe that requires the
echo .NET Desktop Runtime, change --self-contained true to false.
echo.

choice /c YN /n /m "Run it now? [Y/N] "
if errorlevel 2 goto :done
start "" "%DISTDIR%\%APPNAME%.exe"

:done
exit /b 0

:fail
echo.
echo ================================
echo  BUILD FAILED - see errors above
echo ================================
pause
exit /b 1
