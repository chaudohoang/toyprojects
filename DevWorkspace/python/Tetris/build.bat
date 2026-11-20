@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ================================
echo  Tetris - Build Script
echo ================================
echo.

where python >nul 2>&1
if errorlevel 1 (
    echo ERROR: python not found on PATH.
    goto :fail
)

echo [1/4] Installing dependencies...
python -m pip install -r requirements.txt -q || goto :fail

echo.
echo [2/4] Smoke test...
python smoke_test.py || goto :fail

echo.
echo [3/4] Building exe...
python -m PyInstaller --onefile --windowed --name "Tetris" --noconfirm tetris.py || goto :fail

echo.
echo [4/4] Cleaning up...
rmdir /s /q build 2>nul
rmdir /s /q __pycache__ 2>nul
del /q Tetris.spec 2>nul

if not exist "dist\Tetris.exe" (
    echo ERROR: dist\Tetris.exe was not produced.
    goto :fail
)

for %%S in ("dist\Tetris.exe") do set /a SIZEMB=%%~zS/1048576

echo.
echo ================================
echo  Build complete
echo  %~dp0dist\Tetris.exe  (%SIZEMB% MB)
echo ================================
echo.

choice /c YN /n /m "Run it now? [Y/N] "
if errorlevel 2 goto :done
start "" "dist\Tetris.exe"

:done
exit /b 0

:fail
echo.
echo ================================
echo  BUILD FAILED - see errors above
echo ================================
pause
exit /b 1
