"""Các engine OCR: Tesseract + dữ liệu vie (mặc định) và Windows OCR (chỉ dùng được nếu Windows có gói OCR cho ngôn ngữ đó)."""
from __future__ import annotations

import asyncio
import os
import re
import shutil
import subprocess
from pathlib import Path

import cv2
import numpy as np


from .paths import BUNDLED_TESSERACT, TESSDATA_DIR
_NO_WINDOW = getattr(subprocess, "CREATE_NO_WINDOW", 0)
_LETTER = re.compile(r"[^\W\d_]", re.UNICODE)


class OcrError(RuntimeError):
    pass


class WindowsOcr:
    """Dùng Windows.Media.Ocr. Lưu ý: Windows hiện không có gói OCR tiếng Việt."""

    name = "windows"

    def __init__(self, language: str = "vi"):
        from winrt.windows.globalization import Language
        from winrt.windows.media.ocr import OcrEngine

        available = [l.language_tag for l in OcrEngine.available_recognizer_languages]
        tag = next((t for t in available if t.lower().startswith(language.lower())), None)
        if tag is None:
            raise OcrError(
                f"Windows OCR chưa có ngôn ngữ '{language}'. Đang có: {available}. "
                "Windows không có gói OCR tiếng Việt, hãy dùng ocr_engine = tesseract."
            )
        self.language = tag
        self._engine = OcrEngine.try_create_from_language(Language(tag))
        self._max_dim = OcrEngine.max_image_dimension
        self._loop = asyncio.new_event_loop()

    def _to_bitmap(self, gray: np.ndarray):
        from winrt.windows.graphics.imaging import BitmapPixelFormat, SoftwareBitmap
        from winrt.windows.storage.streams import DataWriter

        h, w = gray.shape[:2]
        if max(h, w) > self._max_dim:
            s = self._max_dim / max(h, w)
            gray = cv2.resize(gray, (int(w * s), int(h * s)), interpolation=cv2.INTER_AREA)
            h, w = gray.shape[:2]
        bgra = cv2.cvtColor(gray, cv2.COLOR_GRAY2BGRA)
        writer = DataWriter()
        writer.write_bytes(bgra.tobytes())
        return SoftwareBitmap.create_copy_from_buffer(writer.detach_buffer(), BitmapPixelFormat.BGRA8, w, h)

    def recognize(self, gray: np.ndarray) -> str:
        bitmap = self._to_bitmap(gray)
        result = self._loop.run_until_complete(self._engine.recognize_async(bitmap))
        return "\n".join(line.text for line in result.lines)


class TesseractOcr:
    """Dùng tesseract.exe với dữ liệu 'vie'."""

    name = "tesseract"

    def __init__(self, language: str = "vie", exe: str | None = None, model: str = "fast"):
        candidates = [exe, str(BUNDLED_TESSERACT), shutil.which("tesseract"), r"C:\Program Files\Tesseract-OCR\tesseract.exe"]
        self.exe = next((c for c in candidates if c and os.path.isfile(c)), None)
        if not self.exe:
            raise OcrError("Không tìm thấy tesseract.exe. Cài Tesseract (xem README) hoặc đặt 'tesseract_path' trong config.")
        self.language = language
        # Ưu tiên dữ liệu trong thư mục dự án: tessdata/fast hoặc tessdata/best
        local = TESSDATA_DIR / model
        self.tessdata = str(local) if (local / f"{language}.traineddata").is_file() else None
        self.model = model if self.tessdata else "system"
        base = [self.exe] + (["--tessdata-dir", self.tessdata] if self.tessdata else [])
        langs = subprocess.run(base + ["--list-langs"], capture_output=True, text=True,
                               creationflags=_NO_WINDOW).stdout
        if language not in langs.split():
            raise OcrError(f"Tesseract chưa có dữ liệu ngôn ngữ '{language}' (đặt {language}.traineddata vào tessdata/{model}/).")
        self._cmd = base + ["stdin", "stdout", "-l", language, "--oem", "1", "--psm"]

    def _run(self, png: bytes, psm: str) -> str:
        out = subprocess.run(self._cmd + [psm], input=png, capture_output=True, creationflags=_NO_WINDOW)
        return out.stdout.decode("utf-8", errors="replace").strip()

    def recognize(self, gray: np.ndarray) -> str:
        ok, png = cv2.imencode(".png", gray)
        if not ok:
            return ""
        png = png.tobytes()
        text = self._run(png, "6")  # khối văn bản nhiều dòng
        if len(_LETTER.findall(text)) <= 3:
            # Câu rất ngắn ("Ừ.", "Hả?"): chế độ 1 từ đọc dấu chuẩn hơn
            word = self._run(png, "8")
            if _LETTER.search(word):
                return word
        return text


def create_ocr(cfg: dict):
    engine = cfg.get("ocr_engine", "auto")
    errors = []
    order = ["tesseract", "windows"] if engine == "auto" else [engine]
    for name in order:
        try:
            if name == "windows":
                return WindowsOcr(cfg.get("windows_ocr_language", "vi"))
            if name == "tesseract":
                return TesseractOcr(cfg.get("tesseract_language", "vie"), cfg.get("tesseract_path"),
                                    cfg.get("tesseract_model", "fast"))
            errors.append(f"Engine không hợp lệ: {name}")
        except Exception as e:  # noqa: BLE001
            errors.append(f"[{name}] {e}")
    raise OcrError("Không khởi tạo được OCR:\n" + "\n".join(errors))
