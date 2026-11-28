"""Tách chữ phụ đề (trắng, viền đen) ra khỏi nền phim để OCR chính xác hơn."""
from __future__ import annotations

import cv2
import numpy as np


def text_mask(bgr: np.ndarray, cfg: dict, exclude: np.ndarray | None = None) -> np.ndarray:
    """Trả về mặt nạ nhị phân (255 = pixel chữ phụ đề).

    - Chữ phụ đề thường rất sáng (V cao) và gần trắng (S thấp).
    - Watermark kiểu "PHIM ĐƯỢC CẬP NHẬT..." là chữ xám mờ -> bị loại vì không đủ sáng.
    - require_outline: chỉ giữ pixel sáng nằm sát pixel tối (viền đen của phụ đề),
      giúp loại các mảng nền trắng (tường, trời, áo trắng...).
    - exclude: vùng chữ đứng yên suốt phim (logo kênh...) -> bỏ trước khi xét.
    """
    hsv = cv2.cvtColor(bgr, cv2.COLOR_BGR2HSV)
    s, v = hsv[..., 1], hsv[..., 2]
    mask = ((v >= cfg.get("min_brightness", 190)) & (s <= cfg.get("max_saturation", 70))).astype(np.uint8) * 255
    if exclude is not None and exclude.shape == mask.shape:
        mask[exclude] = 0

    n, labels, stats, _ = cv2.connectedComponentsWithStats(mask, connectivity=8)
    if n <= 1:
        return np.zeros_like(mask)
    areas = stats[:, cv2.CC_STAT_AREA]
    h, w = mask.shape
    keep = (areas >= 4) & (stats[:, cv2.CC_STAT_HEIGHT] < h * 0.9) & (stats[:, cv2.CC_STAT_WIDTH] < w * 0.5)

    if cfg.get("require_outline", True):
        # Xét theo từng nét chữ: phần viền của nét phải nằm sát pixel tối.
        dark = (v <= cfg.get("outline_max_brightness", 90)).astype(np.uint8) * 255
        near_dark = cv2.dilate(dark, np.ones((3, 3), np.uint8), iterations=2) > 0
        edge = (mask > 0) & ~(cv2.erode(mask, np.ones((3, 3), np.uint8)) > 0)
        edge_total = np.bincount(labels[edge], minlength=n)
        edge_dark = np.bincount(labels[edge & near_dark], minlength=n)
        ratio = edge_dark / np.maximum(edge_total, 1)
        keep &= ratio >= cfg.get("outline_ratio", 0.6)

    keep[0] = False
    mask = np.where(keep[labels], 255, 0).astype(np.uint8)
    ratio = cfg.get("line_height_ratio", 0.6)
    return keep_main_lines(mask, ratio) if ratio > 0 else mask


def _row_bands(mask: np.ndarray, join: int) -> list[tuple[int, int]]:
    rows = cv2.dilate(mask, np.ones((join, 1), np.uint8)).any(axis=1)
    bands, y, n = [], 0, len(rows)
    while y < n:
        if rows[y]:
            y0 = y
            while y < n and rows[y]:
                y += 1
            bands.append((y0, y))
        y += 1
    return bands


def keep_main_lines(mask: np.ndarray, ratio: float = 0.6) -> np.ndarray:
    """Chỉ giữ các dòng chữ có cỡ gần bằng dòng to nhất (= phụ đề).

    Loại chữ nhỏ như thời gian/nút của trình phát khi tạm dừng, watermark, logo nhỏ.
    Dấu thanh tách thành dòng riêng ngay phía trên (hoặc dấu nặng ngay phía dưới) vẫn được giữ.
    """
    n, labels, stats, _ = cv2.connectedComponentsWithStats(mask, connectivity=8)
    if n <= 2:
        return mask
    heights = stats[1:, cv2.CC_STAT_HEIGHT]
    centers = stats[1:, cv2.CC_STAT_TOP] + heights / 2
    bands = _row_bands(mask, join=7)
    sizes = []
    for y0, y1 in bands:
        hs = heights[(centers >= y0) & (centers < y1)]
        sizes.append(float(np.percentile(hs, 90)) if len(hs) else 0.0)
    ref = max(sizes)
    if ref <= 0:
        return mask
    main = [s >= ratio * ref for s in sizes]
    keep_band = list(main)
    for i, (y0, y1) in enumerate(bands):
        if main[i]:
            continue
        # dấu thanh phía trên dòng chính
        if i + 1 < len(bands) and main[i + 1] and bands[i + 1][0] - y1 < 0.5 * ref and sizes[i] < 0.5 * ref:
            keep_band[i] = True
        # dấu nặng / dấu chấm phía dưới dòng chính
        if i > 0 and main[i - 1] and y0 - bands[i - 1][1] < 0.3 * ref and (y1 - y0) < 0.25 * ref:
            keep_band[i] = True
    row_keep = np.zeros(mask.shape[0], dtype=bool)
    for (y0, y1), k in zip(bands, keep_band):
        if k:
            row_keep[y0:y1] = True
    comp_keep = np.zeros(n, dtype=bool)
    comp_keep[1:] = row_keep[np.clip(centers.astype(int), 0, mask.shape[0] - 1)]
    return np.where(comp_keep[labels], 255, 0).astype(np.uint8)


def mask_fill_ratio(mask: np.ndarray) -> float:
    return float(np.count_nonzero(mask)) / mask.size


def prepare_for_ocr(mask: np.ndarray, target_text_height: int = 48, thicken: int = 0) -> np.ndarray:
    """Chữ đen trên nền trắng, cắt sát vùng chữ, phóng to để OCR đọc dấu tiếng Việt tốt hơn."""
    ys, xs = np.nonzero(mask)
    if len(ys) == 0:
        return np.full((32, 32), 255, np.uint8)
    pad = 12
    y0, y1 = max(ys.min() - pad, 0), min(ys.max() + pad, mask.shape[0])
    x0, x1 = max(xs.min() - pad, 0), min(xs.max() + pad, mask.shape[1])
    crop = mask[y0:y1, x0:x1]

    # Ước lượng chiều cao dòng chữ từ các thành phần liên thông để chọn tỉ lệ phóng
    n, _, stats, _ = cv2.connectedComponentsWithStats(crop, connectivity=8)
    heights = [stats[i][3] for i in range(1, n) if stats[i][4] > 15]
    line_h = float(np.median(heights)) if heights else 20.0
    scale = float(np.clip(target_text_height / max(line_h, 1.0), 1.0, 4.0))
    if scale > 1.05:
        crop = cv2.resize(crop, None, fx=scale, fy=scale, interpolation=cv2.INTER_CUBIC)
        _, crop = cv2.threshold(crop, 127, 255, cv2.THRESH_BINARY)
    if thicken > 0:
        # Nét dày hơn một chút giúp Tesseract đọc đúng dấu chồng (ồ, ầ, ẫ, ả) với font phụ đề mảnh
        crop = cv2.dilate(crop, np.ones((thicken, thicken), np.uint8))

    out = 255 - crop
    return cv2.copyMakeBorder(out, 20, 20, 20, 20, cv2.BORDER_CONSTANT, value=255)


def masks_differ(a: np.ndarray | None, b: np.ndarray | None, threshold: float = 0.15) -> bool:
    """So sánh hai mặt nạ chữ: True nếu phụ đề đã đổi."""
    if a is None or b is None or a.shape != b.shape:
        return True
    small_a = cv2.resize(a, (a.shape[1] // 4 or 1, a.shape[0] // 4 or 1), interpolation=cv2.INTER_AREA)
    small_b = cv2.resize(b, (b.shape[1] // 4 or 1, b.shape[0] // 4 or 1), interpolation=cv2.INTER_AREA)
    ba, bb = small_a > 64, small_b > 64
    union = np.count_nonzero(ba | bb)
    if union == 0:
        return False
    inter = np.count_nonzero(ba & bb)
    return (1.0 - inter / union) > threshold


def split_lines(mask: np.ndarray) -> list[tuple[int, int]]:
    """Tách mặt nạ chữ thành từng dòng (y0, y1), mỗi dòng kèm dấu thanh phía trên / dấu nặng phía dưới của nó.

    Dùng cho OCR chỉ đọc được 1 dòng một lần (VietOCR).
    """
    rows = mask.any(axis=1)
    bands, y = [], 0
    while y < len(rows):
        if rows[y]:
            y0 = y
            while y < len(rows) and rows[y]:
                y += 1
            if bands and y0 - bands[-1][1] <= 2:
                bands[-1][1] = y  # khe hở 1-2 px trong cùng một dòng
            else:
                bands.append([y0, y])
        y += 1
    if not bands:
        return []
    hmax = max(b - a for a, b in bands)
    main = [b for b in bands if b[1] - b[0] >= 0.55 * hmax]
    lines = [list(b) for b in main]
    for b in bands:
        if b in main:
            continue
        # gán vào dòng chính gần nhất; cách đều thì gán dòng dưới (dấu thanh nằm trên chữ)
        i = min(range(len(main)), key=lambda k: (max(main[k][0] - b[1], b[0] - main[k][1], 0), -k))
        lines[i][0], lines[i][1] = min(lines[i][0], b[0]), max(lines[i][1], b[1])
    return [(a, b) for a, b in lines]
