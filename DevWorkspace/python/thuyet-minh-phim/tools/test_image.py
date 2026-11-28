"""Thử OCR (và đọc) trên một ảnh chụp màn hình có sẵn.

Ví dụ:
    python tools/test_image.py samples/motchill.webp --crop 300,720,1320,220 --speak
"""
import argparse
import sys
import time
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import cv2  # noqa: E402

from longtieng import config  # noqa: E402
from longtieng.engine import recognize_frame  # noqa: E402
from longtieng.ocr import create_ocr  # noqa: E402


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("image")
    ap.add_argument("--crop", help="left,top,width,height trong ảnh")
    ap.add_argument("--engine", help="windows | tesseract")
    ap.add_argument("--lang", help="ngôn ngữ OCR, vd vi hoặc en")
    ap.add_argument("--speak", action="store_true")
    ap.add_argument("--out", default="debug/test_image_ocr.png")
    a = ap.parse_args()

    cfg = config.load()
    if a.engine:
        cfg["ocr_engine"] = a.engine
    if a.lang:
        cfg["windows_ocr_language"] = a.lang
    img = cv2.imread(a.image)
    if img is None:
        sys.exit(f"Không đọc được ảnh: {a.image}")
    if a.crop:
        x, y, w, h = map(int, a.crop.split(","))
        img = img[y:y + h, x:x + w]

    ocr = create_ocr(cfg)
    t0 = time.perf_counter()
    raw, text, prepared = recognize_frame(img, ocr, cfg)
    ms = (time.perf_counter() - t0) * 1000
    Path(a.out).parent.mkdir(parents=True, exist_ok=True)
    cv2.imwrite(a.out, prepared)
    sys.stdout.reconfigure(encoding="utf-8")
    print(f"OCR: {ocr.name} ({getattr(ocr, 'language', '')}) — {ms:.0f} ms")
    print(f"Thô : {raw!r}")
    print(f"Sạch: {text!r}")

    if a.speak and text:
        from longtieng.tts import Speaker

        sp = Speaker(cfg)
        sp.say(text)
        time.sleep(1 + len(text) * 0.09)
        sp.close()


if __name__ == "__main__":
    main()
