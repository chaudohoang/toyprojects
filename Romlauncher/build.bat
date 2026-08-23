@echo off
setlocal
cd /d "%~dp0"

set CSC=
for %%V in (v4.0.30319) do (
    if exist "%WINDIR%\Microsoft.NET\Framework64\%%V\csc.exe" set CSC=%WINDIR%\Microsoft.NET\Framework64\%%V\csc.exe
    if not defined CSC if exist "%WINDIR%\Microsoft.NET\Framework\%%V\csc.exe" set CSC=%WINDIR%\Microsoft.NET\Framework\%%V\csc.exe
)

if not defined CSC (
    echo [ERROR] csc.exe not found. Install .NET Framework 4.x.
    exit /b 1
)

echo Using %CSC%

set ICON=
if exist "RomLauncher.ico" set ICON=/win32icon:RomLauncher.ico

"%CSC%" /nologo /target:winexe /platform:anycpu /optimize+ ^
  /out:RomLauncher.exe %ICON% ^
  /reference:System.dll ^
  /reference:System.Core.dll ^
  /reference:System.Drawing.dll ^
  /reference:System.Windows.Forms.dll ^
  RomLauncher.cs

if errorlevel 1 (
    echo.
    echo [FAILED] Build failed.
    exit /b 1
)

echo.
echo [OK] RomLauncher.exe built.
endlocal
