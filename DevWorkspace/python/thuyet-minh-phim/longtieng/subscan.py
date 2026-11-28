"""Quét cả file phim để lấy phụ đề in cứng (hardsub) thành file .srt."""
from __future__ import annotations

import re
import threading
import time
from concurrent.futures import Future, ThreadPoolExecutor
from dataclasses import dataclass, field
from typing import Callable

import numpy as np

from .ocr import create_ocr
from .preprocess import mask_fill_ratio, masks_differ, prepare_for_ocr, text_mask
from .srt import Cue
from .textproc import clean_text, fix_common_ocr, is_vietnamese_syllable, merge_ocr, similarity, vote
from .video import grab_frame, iter_frames, probe, zone_rows


@dataclass
class _Segment:
    start: float
    end: float = 0.0
    frames: int = 1
    futures: list[Future] = field(default_factory=list)
    viet: Future | None = None  # VietOCR (1 khung), ghép với kết quả Tesseract


DEFAULT_ZONES = {"bottom": [2 / 3, 1.0], "top": [0.0, 0.4]}
_BRACKETED = re.compile(r"\[[^\[\]]*(?:\]|$)")


def side_parts(text: str) -> list[str]:
    """Phụ đề phụ (vùng trên: biển hiệu, địa danh, lời qua điện thoại...) hay nằm trong [ngoặc vuông].

    - Có "[" -> chỉ giữ các cụm trong ngoặc ("nem [Thâm Uyên Vô Gian]" -> ["[Thâm Uyên Vô Gian]"]).
    - Chỉ có "]" mà không có "[" -> mẩu cắt dở của hiệu ứng chữ chạy -> bỏ.
    - Cả câu phải có ít nhất 2 âm tiết tiếng Việt (loại rác ngắn như "tà", "NI", "1 Mu").
    """
    if "[" in text:
        parts = [p.strip() for p in _BRACKETED.findall(text) if len(p.strip()) > 2]
    elif "]" in text:
        return []
    else:
        parts = [text]
    return parts if _syllables(" ".join(parts)) >= 2 else []


def tidy_side_text(text: str) -> str:
    return " ".join(side_parts(text))


def _words(text: str) -> list[str]:
    return [w for w in (t.strip("[].,!?…:;\"'-") for t in text.split()) if w]


def _syllables(text: str) -> int:
    return sum(1 for w in _words(text) if is_vietnamese_syllable(w))


def _quality(text: str) -> tuple:
    """Bản nào "sạch" hơn: tỉ lệ âm tiết hợp lệ, ngoặc đóng đủ, rồi mới tới độ dài."""
    words = _words(text)
    return (round(_syllables(text) / max(len(words), 1), 2), text.count("[") == text.count("]"), len(text))


def _key(part: str) -> str:
    return " ".join(part.lower().strip("[]").split())


def static_masks(path: str, info, cfg: dict, rows: dict[str, tuple[int, int]],
                 samples: int = 36) -> dict[str, np.ndarray | None]:
    """Tìm chữ đứng yên gần như suốt phim (logo kênh "Motchillv.so", watermark...) để bỏ khi quét.

    Lấy ~36 khung rải đều cả phim; pixel nào là "chữ" ở >= 40% số khung thì coi là logo.
    Phụ đề thật đổi liên tục nên không pixel nào đạt mức đó.
    """
    import cv2

    lo, hi = info.duration * 0.03, info.duration * 0.97
    n = int(min(samples, max(0, (hi - lo) // 10)))
    if n < 6:
        return {k: None for k in rows}
    times = [lo + (hi - lo) * (i + 0.5) / n for i in range(n)]
    with ThreadPoolExecutor(max_workers=4) as ex:
        frames = [f for f in ex.map(lambda t: grab_frame(path, t, info), times) if f is not None]
    if len(frames) < 6:
        return {k: None for k in rows}
    loose = dict(cfg, line_height_ratio=0)
    out: dict[str, np.ndarray | None] = {}
    for zone, (y0, y1) in rows.items():
        count = sum((text_mask(f[y0:y1], loose) > 0).astype(np.uint16) for f in frames)
        static = (count >= max(3, 0.4 * len(frames))).astype(np.uint8)
        out[zone] = cv2.dilate(static, np.ones((7, 7), np.uint8)) > 0 if static.any() else None
    return out


def scan_video(path: str, cfg: dict,
               progress: Callable[[float, int], None] | None = None,
               stop: threading.Event | None = None, debug: list | None = None,
               log: Callable[[str], None] | None = None) -> list[Cue]:
    """Trả về danh sách câu phụ đề (thời gian bắt đầu/kết thúc theo giây), gộp các vùng và xếp theo thời gian.

    Quét riêng từng vùng (mặc định 1/3 dưới = thoại, 1/3 trên = phụ đề diễn giải / chú thích).
    Mỗi khi "mặt nạ chữ" của vùng đổi -> bắt đầu đoạn mới. Mỗi đoạn OCR vài khung (khung 2, 4, 6...) x vài cách
    xử lý ảnh, rồi "bỏ phiếu" từng chữ: mỗi khung có nền/nhiễu nén khác nhau nên Tesseract sai khác nhau, số đông
    thường đúng (đo trên phim thật: 1 lần OCR đúng 8/10 câu, bỏ phiếu 9/10).
    OCR chạy song song nhiều tiến trình Tesseract trong lúc vẫn tiếp tục giải mã video.
    Có VietOCR thì đọc thêm 1 khung mỗi câu bằng VietOCR rồi ghép (textproc.merge_ocr): giảm khoảng nửa lỗi dấu.
    """
    import os

    log = log or (lambda m: None)

    info = probe(path)
    fps = float(cfg.get("scan_fps", 5))
    half = 0.5 / fps  # thời điểm thật nằm giữa 2 khung đã lấy
    # Không cần nhanh như chế độ trực tiếp -> dùng model chính xác hơn
    ocr = create_ocr(dict(cfg, tesseract_model=cfg.get("scan_tesseract_model", "best")))
    default_workers = max(2, min(8, (os.cpu_count() or 4) - 1))
    workers = int(cfg.get("scan_workers") or default_workers) if ocr.name == "tesseract" else 1
    variants = [tuple(v) for v in cfg.get("scan_ocr_variants", [[48, 3], [64, 3], [48, 0]])]
    vote_frames = int(cfg.get("scan_vote_frames", 3))

    zones = {k: tuple(v) for k, v in (cfg.get("scan_zones") or DEFAULT_ZONES).items()}
    first = next(iter_frames(path, 1, info), None)
    if first is None:
        raise RuntimeError("Không đọc được khung hình nào.")
    rows = zone_rows(first[1].shape[0], zones)
    del first
    statics = static_masks(path, info, cfg, rows) if cfg.get("scan_ignore_static", True) else {}

    viet = _load_vietocr(cfg, log)
    pool = ThreadPoolExecutor(max_workers=workers)
    viet_pool = ThreadPoolExecutor(max_workers=1) if viet else None
    segments: dict[str, list[_Segment]] = {z: [] for z in rows}
    cur: dict[str, _Segment | None] = {z: None for z in rows}
    prev: dict[str, np.ndarray | None] = {z: None for z in rows}
    last_report = 0.0
    t = 0.0

    def close(zone: str, at: float):
        if cur[zone] is not None:
            cur[zone].end = at
            segments[zone].append(cur[zone])

    try:
        for t, frame in iter_frames(path, fps, info):
            if stop is not None and stop.is_set():
                break
            for zone, (y0, y1) in rows.items():
                mask = text_mask(frame[y0:y1], cfg, statics.get(zone))
                seg = cur[zone]
                if mask_fill_ratio(mask) < cfg["min_fill_ratio"]:
                    close(zone, t - half)
                    cur[zone], prev[zone] = None, None
                elif seg is None or masks_differ(mask, prev[zone], cfg["change_threshold"]):
                    close(zone, t - half)
                    cur[zone], prev[zone] = _Segment(start=max(t - half, 0.0)), mask
                else:
                    seg.frames += 1
                    prev[zone] = mask
                    # OCR khung thứ 2, 4, 6... của đoạn (khung đầu có thể đang hiện dần)
                    if seg.frames % 2 == 0 and seg.frames // 2 <= vote_frames:
                        for h, k in variants:
                            seg.futures.append(pool.submit(ocr.recognize, prepare_for_ocr(mask, h, k)))
                        if viet_pool is not None and seg.viet is None:
                            seg.viet = viet_pool.submit(viet.recognize_mask, mask)
            if progress and time.monotonic() - last_report > 0.25:
                last_report = time.monotonic()
                progress(min(t / max(info.duration, 0.1), 1.0), sum(map(len, segments.values())))
        for zone in rows:
            close(zone, t + half)
    finally:
        cancelled = stop is not None and stop.is_set()
        pool.shutdown(wait=True, cancel_futures=cancelled)
        if viet_pool is not None:
            if not cancelled:
                log("Đang đọc nốt bằng VietOCR...")
            viet_pool.shutdown(wait=True, cancel_futures=cancelled)

    cues: list[Cue] = []
    for zone, segs in segments.items():
        cues += _segments_to_cues(segs, cfg, zone, debug)
    cues.sort(key=lambda c: (c.start, c.end))
    if progress:
        progress(1.0, len(cues))
    return cues


def _load_vietocr(cfg: dict, log: Callable[[str], None]):
    if not cfg.get("scan_vietocr", True):
        return None
    from .vietocr_ocr import VietOcr, is_available

    model = cfg.get("vietocr_model", "vgg_transformer")
    why = is_available(model)
    if why:
        log(f"Chỉ dùng Tesseract: VietOCR {why}.")
        return None
    try:
        v = VietOcr(model, int(cfg.get("vietocr_threads", 0)))
    except Exception as e:  # noqa: BLE001
        log(f"Chỉ dùng Tesseract: không nạp được VietOCR ({e}).")
        return None
    log("Đọc chữ bằng Tesseract + VietOCR.")
    return v


def _side_cues(cues: list[Cue], cfg: dict) -> list[Cue]:
    """Làm gọn phụ đề phụ.

    - Biển hiệu lặp lại ("[Công an]" ở mỗi cảnh xe cảnh sát, "[Quán net Bôn Đăng]" dính kèm câu khác)
      chỉ đọc 1 lần trong `side_repeat_seconds` giây.
    - Chữ hiện dần / mờ dần tạo nhiều bản gần giống nhau liền nhau -> gộp, giữ bản sạch nhất.
    """
    window = float(cfg.get("side_repeat_seconds", 60))
    recent: dict[str, float] = {}
    out: list[Cue] = []

    def seen(key: str, start: float) -> bool:
        return any(start - t < window and (similarity(key, k) >= 0.85 or k.startswith(key))
                   for k, t in recent.items())

    for c in cues:
        parts = side_parts(c.text)
        if not parts:
            continue
        text = " ".join(parts)
        if out and c.start - out[-1].end <= 1.5 and similarity(text, out[-1].text) >= 0.75:
            out[-1].end = c.end  # cùng câu, chỉ khác do hiệu ứng -> gộp
            if _quality(text) > _quality(out[-1].text):
                out[-1].text = text
        else:
            fresh = [p for p in parts if not seen(_key(p), c.start)]
            if fresh:
                out.append(Cue(c.start, c.end, " ".join(fresh)))
            elif out and c.start - out[-1].end <= 5 and similarity(text, out[-1].text) >= 0.75                     and _quality(text) > _quality(out[-1].text):
                out[-1].text = text  # cùng chữ hiện lại rõ hơn ("[Imhâm..." -> "[Thâm Uyên Vô Gian]")
        for p in parts:
            recent[_key(p)] = c.end
    return out


def _segments_to_cues(segments: list[_Segment], cfg: dict, zone: str, debug: list | None) -> list[Cue]:
    blacklist, min_chars = cfg.get("blacklist", []), cfg.get("scan_min_chars", 1)  # giữ câu 1 chữ như "Ừ."
    cues: list[Cue] = []
    for seg in segments:
        if not seg.futures:
            continue  # chỉ 1 khung (hiệu ứng mờ dần / nhiễu)
        cands = []
        for fut in seg.futures:
            try:
                cands.append(clean_text(fut.result(), blacklist, min_chars))
            except Exception:  # noqa: BLE001
                cands.append("")
        if sum(1 for c in cands if c) * 2 < len(cands):
            continue  # đa số là rác (chữ Hán, logo...) -> bỏ
        text = vote(cands)
        if not text:
            continue
        if zone == "bottom" and "]" in text and "[" not in text and len(text.split()) <= 2:
            continue  # mẩu cuối của chữ chú thích bị cắt ("he]")
        # Độ đồng thuận: phụ đề thật -> các lần OCR gần như giống hệt; chữ Hán/hiệu ứng -> mỗi lần một kiểu
        filled = [c for c in cands if c]
        agreement = sum(similarity(c, text) for c in filled) / len(filled)
        if debug is not None:
            debug.append((seg.start, round(agreement, 2), text) if zone == "bottom" else
                         (seg.start, round(agreement, 2), text, zone))
        if agreement < cfg.get("scan_min_agreement", 0.75):
            continue
        if seg.viet is not None and not seg.viet.cancelled():
            try:
                v = clean_text(seg.viet.result(), blacklist, min_chars)
            except Exception:  # noqa: BLE001
                v = ""
            if v:
                text = fix_common_ocr(merge_ocr(text, v, filled))
        # Gộp với câu trước nếu cùng nội dung (phụ đề chớp / cảnh chuyển làm mặt nạ đổi nhẹ)
        if cues and seg.start - cues[-1].end <= cfg.get("merge_gap", 0.5) \
                and similarity(text, cues[-1].text) >= cfg.get("merge_similarity", 0.85):
            if len(text) > len(cues[-1].text):
                cues[-1].text = text
            cues[-1].end = seg.end
            continue
        cues.append(Cue(seg.start, seg.end, text))
    if zone != "bottom":
        cues = _side_cues(cues, cfg)
    min_dur = cfg.get("min_cue_seconds", 0.3)
    return [c for c in cues if c.end - c.start >= min_dur]
