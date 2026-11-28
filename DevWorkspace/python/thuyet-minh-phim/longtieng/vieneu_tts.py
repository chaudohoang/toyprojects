"""Giọng VieNeu-TTS v3 Turbo: offline, chạy CPU bằng ONNX Runtime, 25 giọng có sẵn (48 kHz).

- Mô hình (~520 MB) tải lần đầu từ HuggingFace về %APPDATA%\\ThuyetMinhPhim\\models\\vieneu
  (hoặc đặt sẵn ở models\\vieneu cạnh file .exe). Chỉ tải phần cần cho giọng có sẵn (fp32),
  không tải phần nhân bản giọng.
- Nhanh gấp ~1,8 lần thời gian thực trên CPU 6 nhân trở lên, nên chậm hơn giọng Windows nhiều.
- Không có chỉnh tốc độ/cao độ: tốc độ làm bằng cách co giãn âm thanh sau khi tạo (ffmpeg atempo).
"""
from __future__ import annotations

import importlib.util
import json
import os
import subprocess
import threading
from pathlib import Path
from typing import Callable

import numpy as np

from .paths import DATA_DIR, MODELS_DIR

PREFIX = "vieneu:"
SAMPLE_RATE_OUT = 24000  # = tts.SAMPLE_RATE

# Ghim đúng phiên bản mô hình đã thử với app (repo HuggingFace thay đổi thì app vẫn tải bản này)
_BACKBONE_REPO, _BACKBONE_REV = "pnnbao-ump/VieNeu-TTS-v3-Turbo", "61b85e3d937fbbacb387714180e8182823512523"
_CODEC_REPO, _CODEC_REV = "OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX", "ceff0d0749bfb3fa2d61149794ec6feef0d1e1ae"
# (đường dẫn trong repo, đường dẫn trên máy, dung lượng byte)
_FILES = [
    (_BACKBONE_REPO, _BACKBONE_REV, "config.json", "config.json", 1553),
    (_BACKBONE_REPO, _BACKBONE_REV, "onnx_update/config.json", "onnx_update/config.json", 2152),
    (_BACKBONE_REPO, _BACKBONE_REV, "onnx_update/tokenizer.json", "onnx_update/tokenizer.json", 22320),
    (_BACKBONE_REPO, _BACKBONE_REV, "onnx_update/vieneu_prefill.onnx", "onnx_update/vieneu_prefill.onnx", 324499),
    (_BACKBONE_REPO, _BACKBONE_REV, "onnx_update/vieneu_decode_step.onnx", "onnx_update/vieneu_decode_step.onnx", 306134),
    (_BACKBONE_REPO, _BACKBONE_REV, "onnx_update/vieneu_acoustic_cached.onnx", "onnx_update/vieneu_acoustic_cached.onnx", 7207223),
    (_BACKBONE_REPO, _BACKBONE_REV, "onnx_update/vieneu_v3_heads.npz", "onnx_update/vieneu_v3_heads.npz", 52219622),
    (_BACKBONE_REPO, _BACKBONE_REV, "onnx_update/vieneu_backbone_shared.data", "onnx_update/vieneu_backbone_shared.data", 415319040),
    (_CODEC_REPO, _CODEC_REV, "codec_browser_onnx_meta.json", "codec/codec_browser_onnx_meta.json", 17036),
    (_CODEC_REPO, _CODEC_REV, "moss_audio_tokenizer_decode_full.onnx", "codec/moss_audio_tokenizer_decode_full.onnx", 681902),
    (_CODEC_REPO, _CODEC_REV, "moss_audio_tokenizer_decode_step.onnx", "codec/moss_audio_tokenizer_decode_step.onnx", 351400),
    (_CODEC_REPO, _CODEC_REV, "moss_audio_tokenizer_decode_shared.data", "codec/moss_audio_tokenizer_decode_shared.data", 44198912),
]
TOTAL_BYTES = sum(f[4] for f in _FILES)

_NO_WINDOW = getattr(subprocess, "CREATE_NO_WINDOW", 0)


# ---------- danh sách giọng ----------
def installed() -> bool:
    return importlib.util.find_spec("vieneu") is not None and importlib.util.find_spec("onnxruntime") is not None


def voices() -> dict[str, str]:
    """{"vieneu:Thiện Minh": "VieNeu · Thiện Minh (nam Bắc, kể chuyện, offline)", ...}; rỗng nếu chưa cài vieneu.

    Đọc thẳng file danh sách giọng của gói vieneu, không import (import mất vài giây)."""
    spec = importlib.util.find_spec("vieneu") if installed() else None
    if spec is None or not spec.origin:
        return {}
    path = Path(spec.origin).parent / "assets" / "voices_v3_turbo.json"
    try:
        presets = json.loads(path.read_text(encoding="utf-8")).get("presets", {})
    except (OSError, ValueError):
        return {}
    items = []
    for name, v in presets.items():
        # "Nam · Bắc · Phong cách kể chuyện" -> "nam Bắc, kể chuyện"
        parts = [p.strip() for p in str(v.get("description", "")).split("·")]
        desc = ""
        if len(parts) >= 3:
            style = parts[2].replace("Phong cách ", "").replace("Giọng đọc ", "")
            desc = f"{parts[0].lower()} {parts[1]}, {style}, "
        # Giọng nam trước (thuyết minh thường dùng giọng nam), trong mỗi nhóm giữ thứ tự "nổi bật" của VieNeu
        rank = (0 if v.get("gender") == "male" else 1, v.get("featured") or 99)
        items.append((rank, PREFIX + name, f"VieNeu · {name} ({desc}offline)"))
    return {k: label for _, k, label in sorted(items)}


def is_vieneu(voice: str) -> bool:
    return str(voice).startswith(PREFIX)


# ---------- mô hình ----------
def model_dir() -> Path:
    bundled = MODELS_DIR / "vieneu"
    if all((bundled / f[3]).is_file() for f in _FILES):
        return bundled
    return DATA_DIR / "models" / "vieneu"


def missing_bytes() -> int:
    d = model_dir()
    return sum(f[4] for f in _FILES if not ((d / f[3]).is_file() and (d / f[3]).stat().st_size == f[4]))


def download(progress: Callable[[int, int], None] | None = None, stop: threading.Event | None = None) -> Path:
    """Tải các file mô hình còn thiếu. progress(đã tải, tổng) tính bằng byte."""
    import requests

    d = DATA_DIR / "models" / "vieneu"
    done = TOTAL_BYTES - missing_bytes()
    for repo, rev, src, dst, size in _FILES:
        out = d / dst
        if out.is_file() and out.stat().st_size == size:
            continue
        out.parent.mkdir(parents=True, exist_ok=True)
        part = out.with_name(out.name + ".part")
        url = f"https://huggingface.co/{repo}/resolve/{rev}/{src}"
        with requests.get(url, stream=True, timeout=30) as r:
            r.raise_for_status()
            with open(part, "wb") as fh:
                for chunk in r.iter_content(1 << 20):
                    if stop is not None and stop.is_set():
                        raise RuntimeError("Đã huỷ tải mô hình VieNeu.")
                    fh.write(chunk)
                    done += len(chunk)
                    if progress:
                        progress(done, TOTAL_BYTES)
        if part.stat().st_size != size:
            part.unlink(missing_ok=True)
            raise RuntimeError(f"Tải {src} không đủ dung lượng, thử lại sau.")
        os.replace(part, out)
    return d


def ensure_models(log: Callable[[str], None] | None = None, stop: threading.Event | None = None) -> None:
    need = missing_bytes()
    if not need:
        return
    log = log or (lambda m: None)
    log(f"Đang tải mô hình giọng VieNeu ({need / 1e6:.0f} MB, chỉ tải lần đầu)...")
    last = [-1]

    def progress(done, total):
        pct = int(done * 100 / total)
        if pct // 10 != last[0]:
            last[0] = pct // 10
            log(f"  tải mô hình VieNeu {pct}%")

    download(progress, stop)
    log("Đã tải xong mô hình VieNeu.")


# ---------- tổng hợp ----------
_engine = None
_engine_lock = threading.Lock()


def _load(threads: int = 0):
    global _engine
    with _engine_lock:
        if _engine is not None:
            return _engine
        if not installed():
            raise RuntimeError("Bản này chưa có thư viện VieNeu-TTS (onnxruntime, vieneu).")
        if missing_bytes():
            raise RuntimeError("Chưa tải mô hình giọng VieNeu (~520 MB). Chọn lại giọng VieNeu trong tab Trực tiếp để tải.")
        d = model_dir()
        os.environ.setdefault("HF_HUB_OFFLINE", "1")  # mọi thứ đã có trên máy, không gọi mạng
        from vieneu import Vieneu
        from vieneu._v3_turbo_engine import onnx_runtime_lite as orl

        # vieneu không cho truyền thư mục bộ giải mã âm thanh -> trỏ nó về bản đã tải
        orig_fetch = orl.OnnxV3LiteEngine._fetch

        def fetch(repo, files, subfolder):
            return d / "codec" if repo == _CODEC_REPO else orig_fetch(repo, files, subfolder)

        orl.OnnxV3LiteEngine._fetch = staticmethod(fetch)
        _engine = Vieneu("v3turbo", backbone_repo=str(d), onnx_dir=str(d / "onnx_update"),
                         precision="fp32", threads=int(threads))
        return _engine


class VieNeuVoice:
    """Tổng hợp một câu -> PCM 16-bit mono 24 kHz ở tốc độ tự nhiên (đọc lần lượt, dùng chung một mô hình)."""

    def __init__(self, threads: int = 0):
        self._tts = _load(threads)
        self._lock = threading.Lock()

    def synth(self, text: str, voice: str, volume: int = 0) -> np.ndarray:
        import soxr

        name = voice[len(PREFIX):] if is_vieneu(voice) else voice
        with self._lock:
            wav = np.asarray(self._tts.infer(text, voice=name, apply_watermark=False), dtype=np.float32)
        wav = soxr.resample(wav, self._tts.sample_rate, SAMPLE_RATE_OUT)
        # Cân âm lượng cho ngang giọng Windows (đỉnh ~ -1 dB), rồi theo thanh "Âm lượng"
        peak = float(np.max(np.abs(wav))) if len(wav) else 0.0
        if peak > 1e-4:
            wav = wav * (0.89 / peak)
        wav = wav * max(0.0, 1 + volume / 100)
        return (np.clip(wav, -1, 1) * 32767).astype(np.int16)


def time_stretch(pcm: np.ndarray, tempo: float) -> np.ndarray:
    """Đọc nhanh/chậm hơn mà không đổi cao độ (ffmpeg atempo). pcm: int16 mono 24 kHz."""
    if abs(tempo - 1.0) < 0.01 or len(pcm) == 0:
        return pcm
    from .video import ffmpeg_exe

    tempo = max(0.5, min(tempo, 3.0))
    cmd = [ffmpeg_exe(), "-hide_banner", "-loglevel", "error", "-f", "s16le", "-ar", str(SAMPLE_RATE_OUT), "-ac", "1",
           "-i", "pipe:0", "-af", f"atempo={tempo:.4f}", "-f", "s16le", "pipe:1"]
    r = subprocess.run(cmd, input=pcm.astype(np.int16).tobytes(), capture_output=True, creationflags=_NO_WINDOW)
    if r.returncode != 0:
        raise RuntimeError(f"ffmpeg atempo lỗi: {r.stderr.decode('utf-8', 'replace')[-300:]}")
    return np.frombuffer(r.stdout, dtype=np.int16).copy()
