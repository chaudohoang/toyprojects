@echo off
:: ─────────────────────────────────────────────────────────────
::  build.bat  —  Compile MultiRemoteTool.exe
::  Requires .NET Framework 4.x (ships with Windows 7+)
::  Run from the folder containing MultiRemoteTool.cs
:: ─────────────────────────────────────────────────────────────

setlocal

:: Find the newest csc.exe available on this machine
set CSC=
for %%v in (4.0.30319) do (
    if exist "%WINDIR%\Microsoft.NET\Framework64\v%%v\csc.exe" (
        set "CSC=%WINDIR%\Microsoft.NET\Framework64\v%%v\csc.exe"
    ) else if exist "%WINDIR%\Microsoft.NET\Framework\v%%v\csc.exe" (
        set "CSC=%WINDIR%\Microsoft.NET\Framework\v%%v\csc.exe"
    )
)

:: Fall back to vswhere / VS Build Tools if present
if "%CSC%"=="" (
    for /f "tokens=*" %%i in (
        '"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe 2^>nul'
    ) do set "MSBUILD=%%i"
)

if "%CSC%"=="" (
    echo ERROR: csc.exe not found.
    echo Make sure .NET Framework 4.x is installed, or use Visual Studio to compile.
    pause
    exit /b 1
)

echo Using compiler: %CSC%
echo.

:: Optional: embed app.ico if it exists in the same folder
set ICON_ARG=
if exist "app.ico" set "ICON_ARG=/win32icon:app.ico"

"%CSC%" ^
    /target:winexe ^
    /platform:anycpu ^
    /optimize+ ^
    /out:MultiRemoteTool.exe ^
    /r:System.dll ^
    /r:System.Windows.Forms.dll ^
    /r:System.Drawing.dll ^
    /r:System.Core.dll ^
    /r:System.Linq.dll ^
    %ICON_ARG% ^
    MultiRemoteTool.cs

if %ERRORLEVEL%==0 (
    echo.
    echo =========================================================
    echo  Build SUCCEEDED!  -^>  MultiRemoteTool.exe
    echo =========================================================
) else (
    echo.
    echo  Build FAILED. See errors above.
)

pause
