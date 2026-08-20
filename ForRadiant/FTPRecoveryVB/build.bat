@echo off
setlocal

REM ---------------------------------------------------------------------------
REM Build FTPRecovery.exe (console) and FTPRecoveryGUI.exe (WinForms).
REM Both share the same engine in FTPRecovery.vb - only the entry point differs.
REM Expects WinSCPnet.dll in .\lib\  (copy it from FTPUploaderVB\lib\)
REM ---------------------------------------------------------------------------

set VBC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\vbc.exe
set FWDIR=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319
if not exist "%VBC%" (
    set VBC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\vbc.exe
    set FWDIR=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
)
if not exist "%VBC%" (
    echo ERROR: vbc.exe not found.
    exit /b 1
)

REM WPF assemblies are not in the framework root - they live in a WPF subfolder.
if not exist "%FWDIR%\WPF\PresentationFramework.dll" (
    echo ERROR: PresentationFramework.dll not found under %FWDIR%\WPF
    exit /b 1
)

if not exist "lib\WinSCPnet.dll" (
    echo ERROR: lib\WinSCPnet.dll missing.
    echo        Copy it from FTPUploaderVB\lib\WinSCPnet.dll
    exit /b 1
)

if not exist "bin" mkdir "bin"

REM Icon is optional - build still works without it.
set ICON=
if exist "FTPRecovery.ico" set ICON=/win32icon:FTPRecovery.ico

echo [1/2] Console build ...
"%VBC%" /nologo /target:exe /optionstrict+ /main:Program %ICON% ^
    /out:bin\FTPRecovery.exe ^
    /reference:lib\WinSCPnet.dll ^
    /reference:System.dll /reference:System.Core.dll ^
    FTPRecovery.vb
if errorlevel 1 goto :failed

echo [2/2] WPF GUI build ...
"%VBC%" /nologo /target:winexe /optionstrict+ /main:WpfProgram %ICON% ^
    /out:bin\FTPRecoveryGUI.exe ^
    /libpath:"%FWDIR%\WPF" ^
    /reference:lib\WinSCPnet.dll ^
    /reference:System.dll /reference:System.Core.dll /reference:System.Xml.dll ^
    /reference:PresentationFramework.dll /reference:PresentationCore.dll ^
    /reference:WindowsBase.dll /reference:System.Xaml.dll ^
    /reference:System.Windows.Forms.dll ^
    FTPRecovery.vb FTPRecoveryWpf.vb
if errorlevel 1 goto :failed

copy /y "lib\WinSCPnet.dll" "bin\WinSCPnet.dll" >nul

REM Rule files ship with the exe. Only seed them if absent, so local edits in
REM bin\ are never overwritten by a rebuild.
if not exist "bin\allowed_filenames.txt" copy /y "allowed_filenames.sample.txt" "bin\allowed_filenames.txt" >nul
if not exist "bin\denied_filenames.txt"  copy /y "denied_filenames.sample.txt"  "bin\denied_filenames.txt"  >nul

echo.
echo BUILD OK
echo   bin\FTPRecovery.exe      (command line)
echo   bin\FTPRecoveryGUI.exe   (double-click)
echo.
echo bin\ is a ready-to-ship bundle. Copy these to any machine:
echo   FTPRecoveryGUI.exe  WinSCPnet.dll
echo   allowed_filenames.txt  denied_filenames.txt
echo.
echo WinSCP.exe is located via line 3 of each queue file - no need to copy it.
goto :eof

:failed
echo.
echo BUILD FAILED
exit /b 1
