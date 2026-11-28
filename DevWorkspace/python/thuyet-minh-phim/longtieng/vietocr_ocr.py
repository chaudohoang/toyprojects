"""VietOCR: mô hình học sâu đọc chữ Việt (dùng kèm Tesseract khi quét file phim).

Đo trên 150 câu phim thật: VietOCR đứng riêng không hơn Tesseract, nhưng hai bên sai ở chữ khác nhau
(Tesseract hay mất dấu thanh trên chữ có mũ: lần→lân, tiền→tiên; VietOCR hay nhầm loại dấu: sớm→sóm).
Ghép hai kết quả (xem textproc.merge_ocr) giảm lỗi dấu khoảng một nửa.

Cần: torch, torchvision, einops, vietocr (cài --no-deps) và models/vgg_transformer.pth + .yml
(tools/get_vietocr_model.py). Thiếu thứ nào thì is_available() trả về lý do, app chỉ dùng Tesseract.
"""
from __future__ import annotations

import os
import threading

import cv2
import numpy as np

from .paths import MODELS_DIR
from .preprocess import split_lines


def model_files(model: str = "vgg_transformer"):
    return MODELS_DIR / f"{model}.yml", MODELS_DIR / f"{model}.pth"


def is_available(model: str = "vgg_transformer") -> str | None:
    """None nếu dùng được, ngược lại là lý do."""
    yml, pth = model_files(model)
    if not (yml.is_file() and pth.is_file()):
        return f"chưa có mô hình {pth.name} (chạy tools/get_vietocr_model.py)"
    try:
        import torch  # noqa: F401
        import vietocr.tool.translate  # noqa: F401
    except Exception as e:  # noqa: BLE001
        return f"chưa cài PyTorch/VietOCR ({e})"
    return None


class VietOcr:
    name = "vietocr"

    def __init__(self, model: str = "vgg_transformer", threads: int = 0):
        import torch
        import yaml
        # Chỉ dùng phần mô hình của vietocr (không qua vietocr.tool.predictor/config vì chúng kéo theo gdown,
        # tqdm và tải cấu hình từ mạng)
        from vietocr.tool.translate import build_model

        yml, pth = model_files(model)
        cfg = yaml.safe_load(yml.read_text(encoding="utf-8"))
        cfg["device"] = "cpu"
        cfg["cnn"]["pretrained"] = False
        torch.set_num_threads(threads or max(1, min(6, (os.cpu_count() or 4) // 3)))
        net, vocab = build_model(cfg)
        net.load_state_dict(torch.load(str(pth), map_location="cpu", weights_only=True))
        net.eval()
        self._net, self._vocab, self._cfg = net, vocab, cfg
        self._lock = threading.Lock()

    def _read_line(self, gray: np.ndarray) -> str:
        import torch
        from PIL import Image
        from vietocr.tool.translate import process_input, translate

        d = self._cfg["dataset"]
        img = Image.fromarray(cv2.cvtColor(gray, cv2.COLOR_GRAY2RGB))
        x = process_input(img, d["image_height"], d["image_min_width"], d["image_max_width"])
        with torch.inference_mode():
            s, _prob = translate(x, self._net)
        return self._vocab.decode(s[0].tolist())

    def recognize_mask(self, mask: np.ndarray) -> str:
        """Mặt nạ chữ (255 = chữ) -> văn bản, mỗi dòng một hàng (để clean_text bỏ gạch đầu dòng hội thoại)."""
        out = []
        with self._lock:  # mô hình dùng chung -> đọc lần lượt
            for y0, y1 in split_lines(mask):
                sub = mask[y0:y1]
                xs = np.nonzero(sub.any(axis=0))[0]
                if len(xs) == 0:
                    continue
                x0, x1 = max(xs.min() - 6, 0), min(xs.max() + 7, mask.shape[1])
                ya, yb = max(y0 - 4, 0), min(y1 + 4, mask.shape[0])
                line = cv2.copyMakeBorder(255 - mask[ya:yb, x0:x1], 6, 6, 8, 8, cv2.BORDER_CONSTANT, value=255)
                out.append(self._read_line(line))
        return "\n".join(t for t in out if t.strip())
