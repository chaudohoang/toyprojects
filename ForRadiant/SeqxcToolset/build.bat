@echo off
setlocal EnableDelayedExpansion
title Seqxc Toolset Build

set MSBUILD=

set VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe
if exist "%VSWHERE%" (
    for /f "usebackq tokens=*" %%i in (
        `"%VSWHERE%" -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe 2^>nul`
    ) do set MSBUILD=%%i
)

if "%MSBUILD%"=="" if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" set MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe
if "%MSBUILD%"=="" if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"    set MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe
if "%MSBUILD%"=="" if exist "%ProgramFiles%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"   set MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe
if "%MSBUILD%"=="" if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" set MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe

if "%MSBUILD%"=="" (
    echo ERROR: MSBuild not found. Install Visual Studio or the Build Tools for Visual Studio.
    pause
    exit /b 1
)

echo.
echo  MSBuild : %MSBUILD%
echo  Building: SeqxcToolset.csproj  [Release / WPF / .NET 4.8]
echo.

"%MSBUILD%" SeqxcToolset.csproj /restore /p:Configuration=Release /t:Rebuild /nologo /m

if %ERRORLEVEL%==0 (
    echo.
    echo  ============================================================
    echo   Build successful  -^>  bin\Release\net48\Seqxc Toolset.exe
    echo  ============================================================
    echo.
) else (
    echo.
    echo  ============================================================
    echo   Build FAILED
    echo  ============================================================
    echo.
)

pause
