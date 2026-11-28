"""Đọc video bằng ffmpeg (bản đi kèm gói imageio-ffmpeg, không cần cài riêng)."""
from __future__ import annotations

import re
import subprocess
from dataclasses import dataclass
from typing import Iterator

import numpy as np

_NO_WINDOW = getattr(subprocess, "CREATE_NO_WINDOW", 0)


def ffmpeg_exe() -> str:
    import imageio_ffmpeg

    return imageio_ffmpeg.get_ffmpeg_exe()


@dataclass
class VideoInfo:
    duration: float
    width: int
    height: int
    has_audio: bool
    audio_mono: bool = False


def probe(path: str) -> VideoInfo:
    err = subprocess.run([ffmpeg_exe(), "-hide_banner", "-i", path], capture_output=True, text=True,
                         encoding="utf-8", errors="replace", creationflags=_NO_WINDOW).stderr
    m = re.search(r"Duration:\s*(\d+):(\d+):(\d+(?:\.\d+)?)", err)
    if not m:
        raise RuntimeError(f"Không đọc được video: {path}\n{err[-500:]}")
    duration = int(m.group(1)) * 3600 + int(m.group(2)) * 60 + float(m.group(3))
    v = re.search(r"Stream #[^\n]*Video:[^\n]*?(\d{2,5})x(\d{2,5})", err)
    if not v:
        raise RuntimeError("Không tìm thấy luồng hình trong file.")
    a = re.search(r"Stream #[^\n]*Audio:[^\n]*", err)
    return VideoInfo(duration, int(v.group(1)), int(v.group(2)), bool(a), bool(a and ", mono," in a.group(0)))


def iter_bottom_frames(path: str, fps: float, fraction: float, info: VideoInfo,
                       max_width: int = 1920) -> Iterator[tuple[float, np.ndarray]]:
    """Trả về (thời điểm giây, ảnh BGR của phần dưới `fraction` khung hình), `fps` ảnh mỗi giây."""
    scale = min(1.0, max_width / info.width)
    out_w = int(info.width * scale) // 2 * 2
    crop_h = int(info.height * fraction) // 2 * 2
    out_h = int(crop_h * scale) // 2 * 2
    vf = f"fps={fps},crop=iw:{crop_h}:0:ih-{crop_h}"
    if scale < 1.0:
        vf += f",scale={out_w}:{out_h}"
    else:
        out_w, out_h = info.width, crop_h
    cmd = [ffmpeg_exe(), "-hide_banner", "-loglevel", "error", "-threads", "0", "-i", path,
           "-an", "-sn", "-vf", vf, "-f", "rawvideo", "-pix_fmt", "bgr24", "-"]
    proc = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, creationflags=_NO_WINDOW)
    size = out_w * out_h * 3
    i = 0
    try:
        while True:
            buf = proc.stdout.read(size)
            if len(buf) < size:
                break
            yield i / fps, np.frombuffer(buf, np.uint8).reshape(out_h, out_w, 3)
            i += 1
    finally:
        proc.kill()
        proc.wait()


def _out_size(info: VideoInfo, max_width: int) -> tuple[int, int]:
    scale = min(1.0, max_width / info.width)
    if scale >= 1.0:
        return info.width, info.height
    return int(info.width * scale) // 2 * 2, int(info.height * scale) // 2 * 2


def zone_rows(height: int, zones: dict[str, tuple[float, float]]) -> dict[str, tuple[int, int]]:
    """{"bottom": (2/3, 1.0)} -> {"bottom": (y0, y1)} theo số dòng ảnh."""
    return {k: (int(height * a), int(height * b)) for k, (a, b) in zones.items()}


def iter_frames(path: str, fps: float, info: VideoInfo,
                max_width: int = 1920) -> Iterator[tuple[float, np.ndarray]]:
    """Trả về (thời điểm giây, cả khung hình BGR), `fps` ảnh mỗi giây (cắt vùng bằng numpy ở phía gọi)."""
    out_w, out_h = _out_size(info, max_width)
    vf = f"fps={fps}" + (f",scale={out_w}:{out_h}" if (out_w, out_h) != (info.width, info.height) else "")
    cmd = [ffmpeg_exe(), "-hide_banner", "-loglevel", "error", "-threads", "0", "-i", path,
           "-an", "-sn", "-vf", vf, "-f", "rawvideo", "-pix_fmt", "bgr24", "-"]
    proc = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, creationflags=_NO_WINDOW)
    size = out_w * out_h * 3
    i = 0
    try:
        while True:
            buf = proc.stdout.read(size)
            if len(buf) < size:
                break
            yield i / fps, np.frombuffer(buf, np.uint8).reshape(out_h, out_w, 3)
            i += 1
    finally:
        proc.kill()
        proc.wait()


def grab_frame(path: str, t: float, info: VideoInfo, max_width: int = 1920) -> np.ndarray | None:
    """Một khung hình tại giây t (tua nhanh bằng -ss trước -i)."""
    out_w, out_h = _out_size(info, max_width)
    cmd = [ffmpeg_exe(), "-hide_banner", "-loglevel", "error", "-ss", f"{t:.2f}", "-i", path, "-an", "-sn",
           "-frames:v", "1", "-vf", f"scale={out_w}:{out_h}", "-f", "rawvideo", "-pix_fmt", "bgr24", "-"]
    buf = subprocess.run(cmd, capture_output=True, creationflags=_NO_WINDOW).stdout
    if len(buf) < out_w * out_h * 3:
        return None
    return np.frombuffer(buf[:out_w * out_h * 3], np.uint8).reshape(out_h, out_w, 3)
