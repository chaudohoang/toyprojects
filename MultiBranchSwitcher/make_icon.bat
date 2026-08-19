@echo off
setlocal
rem ============================================================
rem  Regenerates MultiBranchSwitcher.ico from make_icon.py.
rem  Only needed to change the icon design - the .ico is shipped
rem  with the source, and build.bat embeds whatever it finds.
rem ============================================================

cd /d "%~dp0"

rem Prefer the py launcher, fall back to python on PATH.
set "PY="
where py >nul 2>&1 && set "PY=py"
if not defined PY (
    where python >nul 2>&1 && set "PY=python"
)
if not defined PY (
    echo [ERROR] Python was not found on this machine.
    echo         Install it from https://www.python.org/downloads/ and tick
    echo         "Add python.exe to PATH", then run this again.
    echo.
    echo         The existing MultiBranchSwitcher.ico still works - you only need
    echo         Python to change the icon design.
    goto :end
)

echo Using %PY%
%PY% -c "import PIL" >nul 2>&1
if errorlevel 1 (
    echo Pillow not found - installing it now...
    %PY% -m pip install --user pillow
    if errorlevel 1 (
        echo.
        echo [ERROR] Could not install Pillow. Try manually:
        echo         %PY% -m pip install --user pillow
        goto :end
    )
)

echo.
%PY% "%~dp0make_icon.py"
if errorlevel 1 (
    echo.
    echo [FAILED] Icon generation reported an error above.
    goto :end
)

echo.
echo [OK] Icon regenerated. Run build.bat to embed it into the exe.

:end
echo.
pause
endlocal
