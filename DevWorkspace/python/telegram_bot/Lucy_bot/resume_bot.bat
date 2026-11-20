@echo off
echo ========================================
echo       KHOI DONG / RESUME TRO LY LUCY
echo ========================================
echo.

echo [1/3] Kiem tra moi truong ao (.venv)...
if not exist ".venv" (
    echo       - Khong tim thay .venv. Dang tao moi...
    python -m venv .venv
) else (
    echo       - Da co san moi truong ao.
)

echo [2/3] Dang kich hoat moi truong ao...
call .\.venv\Scripts\activate.bat

echo [3/3] Dang cai dat/cap nhat thu vien tu requirements.txt...
pip install -r requirements.txt

echo.
echo ========================================
echo DONE! Moi thu da san sang.
echo Sep co the go "python bot.py" de chay bot ngay bay gio!
echo ========================================
cmd /k
