@echo off
rem Tạo bộ cài (dist\setup) và bản portable (dist\portable: thư mục chạy ngay + file .zip)
rem build.bat -WithVieNeuModel: kèm sẵn mô hình giọng VieNeu (~520 MB) cho máy không có mạng
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1" %*
pause
