@echo off
setlocal
rem ============================================================
rem  Build MultiBranchSwitcher.exe with the .NET Framework
rem  compiler that ships with Windows - no Visual Studio needed.
rem ============================================================

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo [ERROR] csc.exe not found. Install .NET Framework 4.x.
    exit /b 1
)

cd /d "%~dp0"

if exist "MultiBranchSwitcher.exe" del /q "MultiBranchSwitcher.exe"

set "ICON="
if exist "MultiBranchSwitcher.ico" (
    set "ICON=/win32icon:MultiBranchSwitcher.ico"
) else (
    echo [WARN] MultiBranchSwitcher.ico not found - building without an icon.
)

"%CSC%" /nologo /target:winexe /optimize+ /platform:anycpu %ICON% ^
    /out:MultiBranchSwitcher.exe ^
    /reference:System.dll ^
    /reference:System.Core.dll ^
    /reference:System.Drawing.dll ^
    /reference:System.Windows.Forms.dll ^
    MultiBranchSwitcher.cs

if errorlevel 1 (
    echo.
    echo [FAILED] Build error.
    exit /b 1
)

echo.
echo [OK] Built: %~dp0MultiBranchSwitcher.exe
endlocal
