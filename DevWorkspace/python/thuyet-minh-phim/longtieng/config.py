"""Đọc/ghi cấu hình (config.json: thư mục dự án, hoặc %APPDATA%/ThuyetMinhPhim với bản cài)."""
from __future__ import annotations

import json
from pathlib import Path

from .paths import CONFIG_PATH

DEFAULTS: dict = {
    # Vùng phụ đề trên màn hình (pixel thật): [left, top, width, height]
    "region": None,
    "auto_region": True,           # tự lấy phần dưới màn hình đang xem mỗi lần bấm Bắt đầu / Thử
    "auto_region_fraction": 0.3334,  # 1/3
    "monitor": 0,                  # 0 = tự động (màn hình đang xem phim); 1, 2, 3... = cố định (đánh số trái → phải)
    "fps": 8,                      # số lần chụp mỗi giây
    "stable_frames": 1,            # phụ đề phải đứng yên bao nhiêu khung hình trước khi OCR
    "empty_reset_seconds": 1.0,    # phụ đề biến mất bao lâu thì cho phép đọc lại câu giống hệt
    "min_fill_ratio": 0.0004,      # tỉ lệ pixel chữ tối thiểu để coi là có phụ đề (câu "Ừ." ~0,12%)
    "change_threshold": 0.15,      # mức thay đổi mặt nạ chữ để coi là câu mới
    "similarity_threshold": 0.8,   # độ giống văn bản để coi là trùng câu cũ

    # Tách chữ
    "min_brightness": 190,
    "max_saturation": 70,
    "require_outline": True,
    "outline_max_brightness": 90,
    "outline_ratio": 0.6,
    "line_height_ratio": 0.6,      # bỏ dòng chữ nhỏ hơn 60% dòng to nhất (nút trình phát, watermark); 0 = tắt

    # OCR
    "ocr_engine": "auto",          # auto | windows | tesseract
    "windows_ocr_language": "vi",
    "tesseract_language": "vie",
    "tesseract_path": None,
    "tesseract_model": "fast",     # fast | best (thư mục tessdata/<model>/)

    # Lọc chữ rác / watermark
    "blacklist": ["motchill", "phim được cập nhật", "cập nhật nhanh nhất", "www.", ".com", ".net", ".so"],
    "min_chars": 2,

    # TTS
    "voice": "windows",            # windows (Microsoft An, offline) | vi-VN-NamMinhNeural | vi-VN-HoaiMyNeural | google (chị Google, online)
                                   # | "vieneu:<tên>" (VieNeu-TTS, offline, vd "vieneu:Thiện Minh")
    "dub_voice": "",               # giọng khi xuất file ở tab File phim ("" = giống "voice")
    "vieneu_threads": 0,           # số luồng CPU cho VieNeu (0 = tự chọn, tối đa 8)
    "rate": 5,                     # % tốc độ tối thiểu (-50..+100)
    "pitch": -20,                  # % cao độ (-50..+50); -20 = giọng trầm kiểu thuyết minh
    "auto_speed": True,            # tự tăng tốc theo nhịp phụ đề (đọc xong trước khi câu sau hiện)
    "max_rate": 100,               # % tốc độ tối đa khi tự tăng tốc
    "pace_fill": 0.85,             # đọc xong trong ~85% thời gian phụ đề hiện
    "finish_grace": 0.6,           # câu cũ còn ≤ 0,6 s thì đọc nốt rồi mới sang câu mới
    "volume": 0,                   # % âm lượng (-50..+50)
    "interrupt": True,             # câu mới ngắt câu đang đọc (giữ khớp hình)
    "max_backlog": 1,
    "edge_deadline": 2.0,          # giây chờ Edge TTS; quá hạn/lỗi thì đọc bằng giọng Windows
    "edge_fallback": True,

    # Chế độ file phim (quét phụ đề -> .srt -> thuyết minh -> xuất video)
    "last_video": "",
    "last_tab": "live",
    "scan_fps": 5,                 # số khung/giây khi quét (độ chính xác thời gian ~ ±0,1 s)
    # Vùng quét (tỉ lệ chiều cao, từ trên xuống): 1/3 dưới = thoại, 40% trên = phụ đề diễn giải/chú thích
    "scan_zones": {"bottom": [0.6666, 1.0], "top": [0.0, 0.4]},
    "scan_vietocr": True,          # đọc thêm bằng VietOCR rồi ghép với Tesseract (nếu đã cài, xem README)
    "vietocr_model": "vgg_transformer",
    "vietocr_threads": 0,          # số luồng CPU cho VietOCR (0 = tự chọn)
    "scan_ignore_static": True,    # bỏ chữ đứng yên suốt phim (logo kênh, watermark)
    "scan_workers": 0,             # số tiến trình Tesseract song song (0 = tự chọn theo số nhân CPU)
    "scan_ocr_variants": [[48, 3], [64, 3], [48, 0]],  # (cao chữ, độ dày nét) mỗi khung; gộp bằng bỏ phiếu
    "scan_vote_frames": 3,         # số khung OCR mỗi câu để bỏ phiếu
    "scan_min_agreement": 0.77,    # bỏ câu mà các lần OCR không thống nhất (đo trên phim thật: rác ≤ 0,74; thoại ≥ 0,80)
    "scan_tesseract_model": "best",
    "scan_min_chars": 1,
    "merge_gap": 0.5,
    "merge_similarity": 0.85,
    "min_cue_seconds": 0.3,
    "dub_max_rate": 80,            # % tốc độ tối đa khi phải đọc nhanh cho vừa câu
    "dub_concurrency": 6,          # số câu tạo giọng song song
    "google_concurrency": 3,       # số câu gửi Google cùng lúc khi xuất file (nhiều quá dễ bị chặn tạm thời)
    "batch_scan_workers": 2,       # "Cả thư mục": số tập quét cùng lúc (2 nhanh hơn ~1,5 lần; 3 không nhanh thêm)
    "dub_edge_retries": 6,         # Edge lỗi thì thử lại bấy nhiêu lần trước khi dùng giọng Windows
    "dub_gap": 0.08,               # khoảng nghỉ tối thiểu trước câu kế (giây)
    "original_volume": 0.3,        # âm lượng tiếng gốc khi đang có lời thuyết minh (0..1)
    "delete_source": False,        # tab File phim: xuất xong thì chuyển phim gốc + .srt vào Thùng rác
    "duck_original": True,         # True: chỉ nhỏ tiếng gốc lúc có lời; False: nhỏ suốt phim

    "hotkey": "f9",                # bật/tắt đọc
    "test_hotkey": "f2",           # đọc thử câu phụ đề đang hiện
    "monitor_hotkey": "f4",        # đổi màn hình: tự động → 1 → 2 → ... → tự động
    "region_hotkey": "f8",         # kéo tay vùng phụ đề (tắt chế độ tự động)
    "save_debug_images": False,
}


# Giá trị mặc định của các bản trước (bản cũ lưu cả mặc định vào config.json).
# Gặp đúng giá trị này thì coi như người dùng chưa chỉnh -> dùng mặc định mới.
_LEGACY_DEFAULTS = {"fps": 6, "min_fill_ratio": 0.002, "voice": "vi-VN-HoaiMyNeural", "rate": 15}


def load() -> dict:
    cfg = dict(DEFAULTS)
    if CONFIG_PATH.exists():
        try:
            saved = json.loads(CONFIG_PATH.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError):
            saved = {}
        for k, v in saved.items():
            if k in DEFAULTS and not (k in _LEGACY_DEFAULTS and _LEGACY_DEFAULTS[k] == v):
                cfg[k] = v
    return cfg


def save(cfg: dict) -> None:
    """Chỉ lưu các giá trị khác mặc định, để bản cập nhật sau đổi mặc định vẫn có tác dụng."""
    changed = {k: v for k, v in cfg.items() if k in DEFAULTS and DEFAULTS[k] != v}
    CONFIG_PATH.write_text(json.dumps(changed, ensure_ascii=False, indent=2), encoding="utf-8")
