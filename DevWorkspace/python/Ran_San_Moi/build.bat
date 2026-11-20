@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ================================
echo  Ran San Moi - Build Script
echo ================================
echo.

where python >nul 2>&1
if errorlevel 1 (
    echo ERROR: python not found on PATH.
    goto :fail
)

echo [1/3] Installing dependencies...
python -m pip install -r requirements.txt -q || goto :fail

echo.
echo [2/3] Building exe...
python -m PyInstaller --onefile --windowed --name "Ran_San_Moi" --noconfirm snake_game.py || goto :fail

echo.
echo [3/3] Cleaning up...
rmdir /s /q build 2>nul
rmdir /s /q __pycache__ 2>nul
del /q Ran_San_Moi.spec 2>nul

if not exist "dist\Ran_San_Moi.exe" (
    echo ERROR: dist\Ran_San_Moi.exe was not produced.
    goto :fail
)

for %%S in ("dist\Ran_San_Moi.exe") do set /a SIZEMB=%%~zS/1048576

echo.
echo ================================
echo  Build complete
echo  %~dp0dist\Ran_San_Moi.exe  (%SIZEMB% MB)
echo ================================
echo.

choice /c YN /n /m "Run it now? [Y/N] "
if errorlevel 2 goto :done
start "" "dist\Ran_San_Moi.exe"

:done
exit /b 0

:fail
echo.
echo ================================
echo  BUILD FAILED - see errors above
echo ================================
pause
exit /b 1
