@echo off
setlocal
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
if not exist "%CSC%" (
    echo Could not find csc.exe ^(.NET Framework 4.x^).
    exit /b 1
)

"%CSC%" /nologo /target:winexe /out:ErrorPopup.exe ^
    /reference:System.dll ^
    /reference:System.Drawing.dll ^
    /reference:System.Windows.Forms.dll ^
    Program.cs

if errorlevel 1 (
    echo Build FAILED.
    exit /b 1
)
echo Build OK -^> ErrorPopup.exe
endlocal
