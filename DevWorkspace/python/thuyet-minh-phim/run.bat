@echo off
cd /d "%~dp0"
rem Python 3.13: ONNX Runtime (giọng VieNeu) chưa có cho Python 3.14+
if not exist .venv py -3.13 -m venv .venv
.venv\Scripts\python -m pip install -q -r requirements.txt
rem Giọng "chị Google" (gTTS, --no-deps: bỏ click cũ chỉ dùng cho dòng lệnh)
.venv\Scripts\python -c "import gtts" 2>nul || .venv\Scripts\python -m pip install -q --no-deps gTTS==2.5.4
rem VietOCR (tuỳ chọn, ~1 GB): đọc dấu tiếng Việt tốt hơn khi quét file phim. Lần đầu tải PyTorch + mô hình.
if not exist models\vgg_transformer.pth (
    .venv\Scripts\python -m pip install -q -r requirements-vietocr.txt
    .venv\Scripts\python -m pip install -q --no-deps vietocr
    .venv\Scripts\python tools\get_vietocr_model.py
)
rem Giọng VieNeu (offline): thư viện ~150 MB; mô hình ~520 MB app tự tải khi chọn giọng VieNeu lần đầu
.venv\Scripts\python -c "import vieneu" 2>nul || (
    .venv\Scripts\python -m pip install -q -r requirements-vieneu.txt
    .venv\Scripts\python -m pip install -q --no-deps vieneu==3.8.5
)
start "" .venv\Scripts\pythonw.exe main.py
