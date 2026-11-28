"""Giọng "chị Google" (giọng đọc của Google Dịch) qua thư viện gTTS.

- Online, miễn phí, không cần khoá API; chỉ có một giọng nữ tiếng Việt.
- Nhanh: ~0,2–0,4 s/câu (gửi văn bản lên Google, nhận về mp3).
- Google chỉ có tốc độ thường/chậm, nên tốc độ và cao độ được chỉnh sau bằng ffmpeg
  (hạ cao độ thì thành giọng trầm hơn).
- Đây là dịch vụ không chính thức của Google Dịch: gửi quá nhiều câu cùng lúc có thể bị chặn tạm thời.
"""
from __future__ import annotations

import io
import subprocess

import numpy as np

VOICE = "google"
LABEL = "Chị Google (nữ, online)"
SAMPLE_RATE = 24000
_NO_WINDOW = getattr(subprocess, "CREATE_NO_WINDOW", 0)


def is_google(voice: str) -> bool:
    return voice == VOICE


def synth(text: str, volume: int = 0, timeout: float = 10.0) -> np.ndarray:
    """Văn bản -> PCM int16 mono 24 kHz ở tốc độ gốc. Lỗi mạng / bị chặn thì ném ngoại lệ."""
    import miniaudio
    from gtts import gTTS

    buf = io.BytesIO()
    gTTS(text, lang="vi", timeout=timeout).write_to_fp(buf)
    if not buf.getvalue():
        raise RuntimeError("Google không trả âm thanh")
    decoded = miniaudio.decode(buf.getvalue(), output_format=miniaudio.SampleFormat.SIGNED16,
                               nchannels=1, sample_rate=SAMPLE_RATE)
    pcm = np.frombuffer(decoded.samples, dtype=np.int16)
    # Chuẩn hoá đỉnh như giọng VieNeu (giọng Google gốc khá nhỏ), rồi áp âm lượng người dùng chọn
    peak = int(np.abs(pcm.astype(np.int32)).max()) if len(pcm) else 0
    gain = (0.89 * 32767 / peak if peak else 1.0) * max(0.0, 1 + volume / 100)
    return np.clip(pcm.astype(np.float32) * gain, -32768, 32767).astype(np.int16)


def _atempo(tempo: float) -> list[str]:
    """atempo chỉ nhận 0.5–2.0 ở ffmpeg cũ -> tách thành nhiều bước."""
    parts = []
    while tempo > 2.0:
        parts.append("atempo=2.0")
        tempo /= 2.0
    while tempo < 0.5:
        parts.append("atempo=0.5")
        tempo /= 0.5
    if abs(tempo - 1.0) >= 0.005:
        parts.append(f"atempo={tempo:.4f}")
    return parts


def adjust(pcm: np.ndarray, tempo: float = 1.0, pitch: int = 0) -> np.ndarray:
    """Đổi tốc độ (giữ cao độ) và cao độ (% , âm = trầm hơn) bằng ffmpeg."""
    tempo = max(0.5, min(tempo, 3.0))
    factor = max(0.5, min(1 + pitch / 100, 2.0))
    if (abs(tempo - 1.0) < 0.01 and abs(factor - 1.0) < 0.005) or len(pcm) == 0:
        return pcm
    from .video import ffmpeg_exe

    if abs(factor - 1.0) >= 0.005:
        # Phát nhanh/chậm cả tiếng (đổi cao độ) rồi kéo dài/co lại cho đúng tốc độ mong muốn
        chain = [f"asetrate={SAMPLE_RATE * factor:.0f}", f"aresample={SAMPLE_RATE}", *_atempo(tempo / factor)]
    else:
        chain = _atempo(tempo)
    cmd = [ffmpeg_exe(), "-hide_banner", "-loglevel", "error", "-f", "s16le", "-ar", str(SAMPLE_RATE), "-ac", "1",
           "-i", "pipe:0", "-af", ",".join(chain), "-f", "s16le", "-ar", str(SAMPLE_RATE), "pipe:1"]
    r = subprocess.run(cmd, input=pcm.astype(np.int16).tobytes(), capture_output=True, creationflags=_NO_WINDOW)
    if r.returncode != 0:
        raise RuntimeError(f"ffmpeg lỗi: {r.stderr.decode('utf-8', 'replace')[-300:]}")
    return np.frombuffer(r.stdout, dtype=np.int16).copy()
