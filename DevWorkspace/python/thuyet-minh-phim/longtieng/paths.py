"""Đường dẫn dùng chung, chạy được cả từ mã nguồn lẫn bản đã đóng gói (PyInstaller).

- APP_DIR: tài nguyên đi kèm (tessdata, tesseract/) — chỉ đọc.
- DATA_DIR: nơi ghi được (config.json, cache giọng, ảnh debug, mô hình VieNeu tải về).
  Chạy từ mã nguồn: thư mục dự án.
  Bản cài (Program Files không ghi được): %APPDATA%\\ThuyetMinhPhim.
  Bản portable (có file portable.txt cạnh .exe): thư mục data\\ cạnh .exe, để mang cả thư mục sang máy khác / USB.
"""
from __future__ import annotations

import os
import sys
from pathlib import Path

FROZEN = bool(getattr(sys, "frozen", False))
_BESIDE_EXE = Path(sys.executable).parent if FROZEN else Path(__file__).resolve().parent.parent


def _portable_dir() -> Path | None:
    if not (FROZEN and (_BESIDE_EXE / "portable.txt").is_file()):
        return None
    d = _BESIDE_EXE / "data"
    try:  # giải nén vào chỗ không ghi được (vd Program Files) -> dùng %APPDATA% như bản cài
        d.mkdir(parents=True, exist_ok=True)
        probe = d / ".write-test"
        probe.write_bytes(b"")
        probe.unlink()
        return d
    except OSError:
        return None


if FROZEN:
    APP_DIR = Path(getattr(sys, "_MEIPASS", _BESIDE_EXE))
    PORTABLE_DIR = _portable_dir()
    DATA_DIR = PORTABLE_DIR or Path(os.environ.get("APPDATA") or Path.home()) / "ThuyetMinhPhim"
else:
    APP_DIR = _BESIDE_EXE
    PORTABLE_DIR = None
    DATA_DIR = APP_DIR

CONFIG_PATH = DATA_DIR / "config.json"
CACHE_DIR = DATA_DIR / "cache"
DEBUG_DIR = DATA_DIR / "debug"
TESSDATA_DIR = APP_DIR / "tessdata"
# Bản đóng gói: tesseract/ và models/ nằm cạnh file .exe
BUNDLED_TESSERACT = _BESIDE_EXE / "tesseract" / "tesseract.exe"
# Mô hình VietOCR (tools/get_vietocr_model.py): models/vgg_transformer.pth + .yml
MODELS_DIR = _BESIDE_EXE / "models"

DATA_DIR.mkdir(parents=True, exist_ok=True)
