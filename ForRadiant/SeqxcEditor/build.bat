@echo off
setlocal EnableDelayedExpansion
title SeqxcEditor Build

:: .NET Framework 4.x (v4.0 / 4.5 / 4.6 / 4.7 / 4.8) all use the same folder.
:: Check 64-bit first, then 32-bit.
set MSBUILD=

set VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe
if exist "%VSWHERE%" (
    for /f "usebackq tokens=*" %%i in (
        `"%VSWHERE%" -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe 2^>nul`
    ) do set MSBUILD=%%i
)

if "%MSBUILD%"=="" if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"  set MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe
if "%MSBUILD%"=="" if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"     set MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe
if "%MSBUILD%"=="" if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"    set MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe
if "%MSBUILD%"=="" if exist "%ProgramFiles%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"    set MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe
if "%MSBUILD%"=="" if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" set MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe
if "%MSBUILD%"=="" if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"   set MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe
if "%MSBUILD%"=="" if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"  set MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe

if "%MSBUILD%"=="" (
    echo.
    echo  ERROR: MSBuild not found.
    echo.
    pause
    exit /b 1
)

echo.
echo  MSBuild  : %MSBUILD%
echo  Building : SeqxcEditor.csproj  [Release / .NET 4.8]
echo.

:: /t:Rebuild forces full recompile every time (ignores up-to-date check)
"%MSBUILD%" SeqxcEditor.csproj /p:Configuration=Release /t:Rebuild /nologo /m

if %ERRORLEVEL%==0 (
    if exist bin\Release\SeqxcEditor.exe (
        copy /Y bin\Release\SeqxcEditor.exe SeqxcEditor.exe >nul
    )
    echo.
    echo  ============================================================
    echo   Build successful  -^>  SeqxcEditor.exe
    echo  ============================================================
    echo.
) else (
    echo.
    echo  ============================================================
    echo   Build FAILED  ^(see errors above^)
    echo  ============================================================
    echo.
)

pause
