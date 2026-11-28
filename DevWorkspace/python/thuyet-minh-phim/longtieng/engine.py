"""Vòng lặp chính: chụp vùng phụ đề -> phát hiện câu mới -> OCR -> đọc."""
from __future__ import annotations

import threading
import time
from pathlib import Path
from typing import Callable

import cv2
import numpy as np

from .ocr import create_ocr
from .preprocess import mask_fill_ratio, masks_differ, prepare_for_ocr, text_mask
from .screen import bottom_of, monitor_of, occluders, window_rect
from .textproc import clean_text, similarity
from .tts import Speaker

from .paths import DEBUG_DIR


def new_sct():
    import mss

    return (getattr(mss, "MSS", None) or mss.mss)()


def grab_region(sct, region) -> np.ndarray:
    left, top, width, height = region
    shot = sct.grab({"left": left, "top": top, "width": width, "height": height})
    return np.asarray(shot)[:, :, :3]  # BGRA -> BGR


def grab_window_region(sct, region, hwnd=None) -> np.ndarray:
    """Chụp vùng phụ đề; phần bị cửa sổ khác đè lên cửa sổ phim thì bôi đen (không đọc chữ của app khác)."""
    frame = grab_region(sct, region)
    if hwnd:
        frame = frame.copy()
        for x, y, w, h in occluders(hwnd, region):
            frame[y:y + h, x:x + w] = 0
    return frame


def recognize_frame(bgr: np.ndarray, ocr, cfg: dict) -> tuple[str, str, np.ndarray]:
    """OCR một khung hình. Trả về (văn bản thô, văn bản sạch, ảnh đã xử lý)."""
    mask = text_mask(bgr, cfg)
    prepared = prepare_for_ocr(mask)
    raw = ocr.recognize(prepared)
    return raw, clean_text(raw, cfg.get("blacklist", []), cfg.get("min_chars", 2)), prepared


class Reader:
    def __init__(self, cfg: dict,
                 on_text: Callable[[str], None] | None = None,
                 on_status: Callable[[str], None] | None = None):
        self.cfg = cfg
        self.on_text = on_text or (lambda t: None)
        self.on_status = on_status or (lambda s: None)
        self._stop = threading.Event()
        self._thread: threading.Thread | None = None
        self.speaker: Speaker | None = None
        self.target = None    # cửa sổ đang chiếu phim (HWND) hoặc None = đọc thẳng vùng màn hình
        self.follow = False   # True: vùng đọc = phần dưới cửa sổ phim, đi theo khi cửa sổ di chuyển/đổi cỡ

    @property
    def running(self) -> bool:
        return self._thread is not None and self._thread.is_alive()

    def start(self):
        if self.running:
            return
        if not self.cfg.get("region"):
            raise RuntimeError("Chưa chọn vùng phụ đề.")
        self._stop.clear()
        self._thread = threading.Thread(target=self._run, daemon=True, name="reader")
        self._thread.start()

    def stop(self):
        self._stop.set()
        if self._thread:
            self._thread.join(timeout=2)
        self._thread = None
        if self.speaker:
            self.speaker.close()
            self.speaker = None

    def _run(self):
        try:
            ocr = create_ocr(self.cfg)
        except Exception as e:  # noqa: BLE001
            self.on_status(str(e))
            return
        self.speaker = Speaker(self.cfg, on_error=self.on_status, on_info=self.on_status)
        self.on_status(f"Đang chạy — OCR: {ocr.name} ({getattr(ocr, 'language', '')})")

        prev_mask = None        # mặt nạ khung trước
        ocr_mask = None         # mặt nạ lần OCR gần nhất
        stable = 0
        last_spoken = ""
        empty_since: float | None = None
        target, follow = self.target, self.follow
        hidden = False          # cửa sổ phim đang đóng/thu nhỏ
        occ: list = []          # phần vùng đọc bị cửa sổ khác đè
        win_checked = 0.0

        with new_sct() as sct:
            while not self._stop.is_set():
                t0 = time.perf_counter()
                cfg = self.cfg  # cho phép GUI chỉnh trực tiếp khi đang chạy
                try:
                    if target and time.monotonic() - win_checked > 0.5:
                        win_checked = time.monotonic()
                        rect = window_rect(target)
                        if rect is None:
                            if not hidden:
                                self.on_status("Cửa sổ phim đang thu nhỏ hoặc đã đóng — tạm ngừng đọc")
                            hidden = True
                        else:
                            if hidden:
                                self.on_status("Đọc tiếp cửa sổ phim")
                            hidden = False
                            if follow:
                                region = bottom_of(rect, monitor_of(target), cfg.get("auto_region_fraction", 1 / 3))
                                if region != cfg["region"]:
                                    cfg["region"] = region
                                    prev_mask, ocr_mask, stable = None, None, 0
                            occ = occluders(target, cfg["region"])
                    if hidden:
                        self._stop.wait(0.3)
                        continue
                    frame = grab_region(sct, cfg["region"])
                    if occ:
                        frame = frame.copy()
                        for x, y, w, h in occ:
                            frame[y:y + h, x:x + w] = 0
                    mask = text_mask(frame, cfg)

                    if mask_fill_ratio(mask) < cfg["min_fill_ratio"]:
                        # Không có phụ đề
                        prev_mask, ocr_mask, stable = None, None, 0
                        empty_since = empty_since or time.time()
                        if last_spoken and time.time() - empty_since > cfg["empty_reset_seconds"]:
                            last_spoken = ""
                    else:
                        empty_since = None
                        if masks_differ(mask, prev_mask, cfg["change_threshold"]):
                            stable = 0
                        else:
                            stable += 1
                        prev_mask = mask

                        if stable == cfg["stable_frames"] and masks_differ(mask, ocr_mask, cfg["change_threshold"]):
                            ocr_mask = mask
                            prepared = prepare_for_ocr(mask)
                            raw = ocr.recognize(prepared)
                            text = clean_text(raw, cfg.get("blacklist", []), cfg.get("min_chars", 2))
                            if cfg.get("save_debug_images"):
                                DEBUG_DIR.mkdir(exist_ok=True)
                                stamp = time.strftime("%H%M%S") + f"_{int(time.time() * 1000) % 1000:03d}"
                                cv2.imwrite(str(DEBUG_DIR / f"{stamp}_frame.png"), frame)
                                cv2.imwrite(str(DEBUG_DIR / f"{stamp}_ocr.png"), prepared)
                            if text and similarity(text, last_spoken) < cfg["similarity_threshold"]:
                                last_spoken = text
                                self.on_text(text)
                                self.speaker.say(text)
                except Exception as e:  # noqa: BLE001
                    self.on_status(f"Lỗi: {e}")
                    time.sleep(0.5)

                delay = 1.0 / max(cfg.get("fps", 6), 1) - (time.perf_counter() - t0)
                if delay > 0:
                    self._stop.wait(delay)
        self.on_status("Đã dừng")
