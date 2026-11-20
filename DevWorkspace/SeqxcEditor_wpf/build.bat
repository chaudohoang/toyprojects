@echo off
setlocal EnableDelayedExpansion
title SeqxcEditor Build (WPF)

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
    echo ERROR: MSBuild not found.
    pause & exit /b 1
)

echo.
echo  MSBuild : %MSBUILD%
echo  Building: SeqxcEditor.csproj  [Release / WPF / .NET 4.8]
echo.

"%MSBUILD%" SeqxcEditor.csproj /restore /p:Configuration=Release /t:Rebuild /nologo /m

if %ERRORLEVEL%==0 (
    echo.
    echo  ============================================================
    echo   Build successful
    echo   Run: bin\Release\net48\SeqxcEditor.exe
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
