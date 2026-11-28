"""Thuyết minh cả file phim từ danh sách phụ đề: tạo giọng từng câu -> ghép track thuyết minh -> xuất video."""
from __future__ import annotations

import asyncio
import hashlib
import os
import subprocess
import threading
import wave
from pathlib import Path
from typing import Callable

import numpy as np

from . import google_tts, vieneu_tts
from .srt import Cue
from .tts import SAMPLE_RATE, WINDOWS_VOICE, WindowsVoice, edge_synth, trim_silence
from .video import ffmpeg_exe, probe

from .paths import CACHE_DIR as _CACHE_ROOT

CACHE_DIR = _CACHE_ROOT / "tts"
_NO_WINDOW = getattr(subprocess, "CREATE_NO_WINDOW", 0)


def speakable(text: str) -> str:
    """Bỏ ngoặc vuông của phụ đề chú thích ("[Công an Hoàng Thủy]") trước khi đọc."""
    return " ".join(text.replace("[", " ").replace("]", ", ").split()).strip(" ,") or text


def _cache_path(voice: str, rate: int, volume: int, text: str, pitch: int = 0) -> Path:
    key = hashlib.sha1(f"{voice}|{rate}|{volume}|{pitch}|{text}".encode("utf-8")).hexdigest()[:20]
    return CACHE_DIR / f"{key}.wav"


def _save_wav(path: Path, pcm: np.ndarray) -> None:
    # Ghi ra file tạm rồi đổi tên: hai câu giống nhau tạo cùng lúc không làm hỏng file cache của nhau
    path.parent.mkdir(parents=True, exist_ok=True)
    tmp = path.with_name(f"{path.stem}.{os.getpid()}.{threading.get_ident()}.tmp")
    with wave.open(str(tmp), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SAMPLE_RATE)
        w.writeframes(pcm.astype(np.int16).tobytes())
    try:
        os.replace(tmp, path)
    except OSError:  # file đích đang bị đọc (Windows) -> giữ bản đã có
        tmp.unlink(missing_ok=True)


def _load_wav(path: Path) -> np.ndarray:
    with wave.open(str(path), "rb") as w:
        return np.frombuffer(w.readframes(w.getnframes()), dtype=np.int16)


class Dubber:
    def __init__(self, cfg: dict, log: Callable[[str], None] | None = None,
                 progress: Callable[[int, int], None] | None = None, stop: threading.Event | None = None):
        self.cfg = cfg
        self.log = log or (lambda m: None)
        self.progress = progress or (lambda done, total: None)
        self.stop = stop or threading.Event()
        self._win: WindowsVoice | None = None
        self._win_lock: asyncio.Lock | None = None
        self._vn: vieneu_tts.VieNeuVoice | None = None
        self._vn_lock: asyncio.Lock | None = None
        self._g_sem: asyncio.Semaphore | None = None
        self.stats = {"edge": 0, "windows": 0, "vieneu": 0, "google": 0, "sped_up": 0, "overflow": 0, "cached": 0}

    # ---------- tạo giọng ----------
    async def _synth_once(self, text: str, voice: str, rate: int) -> np.ndarray | None:
        volume = int(self.cfg.get("volume", 0))
        pitch = int(self.cfg.get("pitch", 0))
        path = _cache_path(voice, rate, volume, text, pitch)
        if path.exists():
            self.stats["cached"] += 1
            return _load_wav(path)
        pcm = None
        if voice != WINDOWS_VOICE:
            for attempt in range(int(self.cfg.get("dub_edge_retries", 4))):
                try:
                    samples = await asyncio.wait_for(edge_synth(text, voice, rate, volume, cancel=self.stop, pitch=pitch), 15)
                    if samples is not None:
                        pcm = np.frombuffer(trim_silence(samples), dtype=np.int16).copy()
                        self.stats["edge"] += 1
                        break
                except Exception:  # noqa: BLE001  NoAudioReceived ngẫu nhiên / mạng
                    pass
                if self.stop.is_set():
                    return None
                await asyncio.sleep(0.5 * (attempt + 1))
        if pcm is None:  # Edge hỏng hẳn (hoặc chọn giọng Windows)
            if voice != WINDOWS_VOICE:
                self.log(f"Edge lỗi nhiều lần, dùng giọng Windows: {text[:50]}")
            async with self._win_lock:
                self._win = self._win or WindowsVoice()
                samples = await self._win.synth(text, rate, volume, pitch)
            pcm = np.frombuffer(trim_silence(samples), dtype=np.int16).copy()
            self.stats["windows"] += 1
            voice = WINDOWS_VOICE
            path = _cache_path(voice, rate, volume, text, pitch)
        _save_wav(path, pcm)
        return pcm

    async def _google_raw(self, text: str, volume: int) -> np.ndarray | None:
        """Giọng Google, thử lại vài lần (mạng chập chờn / bị chặn tạm thời). None nếu vẫn lỗi."""
        async with self._g_sem:  # ít yêu cầu cùng lúc để Google không chặn
            for attempt in range(int(self.cfg.get("dub_edge_retries", 4))):
                if self.stop.is_set():
                    return None
                try:
                    return await asyncio.wait_for(asyncio.to_thread(google_tts.synth, text, volume), 20)
                except Exception:  # noqa: BLE001
                    await asyncio.sleep(1.0 * (attempt + 1))
        return None

    async def _raw(self, text: str, voice: str) -> np.ndarray | None:
        """VieNeu / Google ở tốc độ tự nhiên (lưu cache như vậy; tốc độ, cao độ chỉnh sau bằng ffmpeg)."""
        volume = int(self.cfg.get("volume", 0))
        path = _cache_path(voice, 0, volume, text)
        if path.exists():
            self.stats["cached"] += 1
            return _load_wav(path)
        if google_tts.is_google(voice):
            samples = await self._google_raw(text, volume)
            if samples is None:
                return None
            self.stats["google"] += 1
        else:
            async with self._vn_lock:  # mô hình đọc lần lượt từng câu
                if self.stop.is_set():
                    return None
                if self._vn is None:
                    self.log("Đang nạp giọng VieNeu...")
                    self._vn = await asyncio.to_thread(vieneu_tts.VieNeuVoice, int(self.cfg.get("vieneu_threads", 0)))
                samples = await asyncio.to_thread(self._vn.synth, text, voice, volume)
            self.stats["vieneu"] += 1
        pcm = trim_silence(samples).copy()
        _save_wav(path, pcm)
        return pcm

    async def _stretch_fit(self, text: str, voice: str, slot: float) -> np.ndarray | None:
        raw = await self._raw(text, voice)
        if raw is None:
            return None
        tempo = 1 + int(self.cfg.get("rate", 10)) / 100
        max_tempo = 1 + int(self.cfg.get("dub_max_rate", 80)) / 100
        dur = len(raw) / SAMPLE_RATE
        if dur / tempo > slot and tempo < max_tempo:
            tempo = min(dur / slot * 1.03, max_tempo)
            self.stats["sped_up"] += 1
        if google_tts.is_google(voice):
            pcm = await asyncio.to_thread(google_tts.adjust, raw, tempo, int(self.cfg.get("pitch", 0)))
        else:
            pcm = await asyncio.to_thread(vieneu_tts.time_stretch, raw, tempo)
        if len(pcm) / SAMPLE_RATE > slot + 0.05:
            self.stats["overflow"] += 1
        return pcm

    async def _synth_fit(self, text: str, slot: float) -> np.ndarray | None:
        """Tạo giọng; nếu dài hơn khoảng trống tới câu kế thì tạo lại với tốc độ nhanh hơn cho vừa."""
        voice = self.cfg.get("voice", "vi-VN-HoaiMyNeural")
        if vieneu_tts.is_vieneu(voice):
            return await self._stretch_fit(text, voice, slot)
        if google_tts.is_google(voice):
            pcm = await self._stretch_fit(text, voice, slot)
            if pcm is not None or self.stop.is_set():
                return pcm
            self.log(f"Giọng Google lỗi nhiều lần, dùng giọng Windows: {text[:50]}")
            voice = WINDOWS_VOICE
        base = int(self.cfg.get("rate", 10))
        max_rate = int(self.cfg.get("dub_max_rate", 80))
        pcm = await self._synth_once(text, voice, base)
        if pcm is None:
            return None
        dur = len(pcm) / SAMPLE_RATE
        if dur > slot and base < max_rate:
            need = ((1 + base / 100) * dur / slot * 1.04 - 1) * 100
            rate = int(min(max(need, base + 5), max_rate))
            faster = await self._synth_once(text, voice, rate)
            if faster is not None:
                pcm = faster
                self.stats["sped_up"] += 1
        if len(pcm) / SAMPLE_RATE > slot + 0.05:
            self.stats["overflow"] += 1
        return pcm

    async def _synth_all(self, cues: list[Cue]) -> list[np.ndarray | None]:
        self._win_lock = asyncio.Lock()
        self._vn_lock = asyncio.Lock()
        self._g_sem = asyncio.Semaphore(int(self.cfg.get("google_concurrency", 3)))
        sem =asyncio.Semaphore(int(self.cfg.get("dub_concurrency", 6)))
        results: list[np.ndarray | None] = [None] * len(cues)
        done = 0
        gap = float(self.cfg.get("dub_gap", 0.08))

        async def one(i: int):
            nonlocal done
            c = cues[i]
            nxt = cues[i + 1].start if i + 1 < len(cues) else c.end + 1.5
            slot = max(nxt - c.start - gap, c.end - c.start, 0.4)
            async with sem:
                if not self.stop.is_set():
                    results[i] = await self._synth_fit(speakable(c.text), slot)
            done += 1
            self.progress(done, len(cues))

        await asyncio.gather(*(one(i) for i in range(len(cues))))
        return results

    # ---------- ghép ----------
    def build_track(self, cues: list[Cue], duration: float) -> tuple[np.ndarray, list[tuple[float, float]]]:
        """Trả về (track thuyết minh, danh sách đoạn có lời (giây))."""
        clips = asyncio.run(self._synth_all(cues))
        track = np.zeros(int((duration + 5) * SAMPLE_RATE), dtype=np.int32)
        spans = []
        cursor = 0
        for c, pcm in zip(cues, clips):
            if pcm is None:
                continue
            # Đặt đúng lúc phụ đề hiện; nếu câu trước còn đang đọc thì lùi lại một chút (không đọc chồng)
            start = max(int(c.start * SAMPLE_RATE), cursor)
            end = min(start + len(pcm), len(track))
            track[start:end] += pcm[: end - start]
            spans.append((start / SAMPLE_RATE, end / SAMPLE_RATE))
            cursor = end + int(0.05 * SAMPLE_RATE)
        return np.clip(track[: int(duration * SAMPLE_RATE)], -32768, 32767).astype(np.int16), spans


ENV_RATE = 1000  # mẫu/giây của đường âm lượng tiếng gốc


def duck_envelope(spans: list[tuple[float, float]], duration: float, low: float, ramp: float = 0.15) -> np.ndarray:
    """Đường âm lượng cho tiếng gốc: 1.0 khi không thoại, `low` khi đang thuyết minh, chuyển mượt `ramp` giây."""
    merged: list[list[float]] = []
    for a, b in sorted(spans):  # 2 câu sát nhau -> giữ nhỏ liên tục, tránh tiếng gốc "nhấp nhô"
        if merged and a - merged[-1][1] < 0.6:
            merged[-1][1] = max(merged[-1][1], b)
        else:
            merged.append([a, b])
    spans = [(a, b) for a, b in merged]
    n = int(duration * ENV_RATE) + 1
    env = np.ones(n, dtype=np.float32)
    r = max(int(ramp * ENV_RATE), 1)
    for a, b in spans:
        i0, i1 = int(a * ENV_RATE), int(b * ENV_RATE)
        seg = np.full(i1 - i0 + 2 * r, low, dtype=np.float32)
        seg[:r] = np.linspace(1.0, low, r)
        seg[-r:] = np.linspace(low, 1.0, r)
        lo, hi = i0 - r, i1 + r
        s0, s1 = max(lo, 0), min(hi, n)
        if s1 > s0:
            env[s0:s1] = np.minimum(env[s0:s1], seg[s0 - lo: s1 - lo])
    return env


def output_path(video: str) -> Path:
    p = Path(video)
    ext = p.suffix.lower() if p.suffix.lower() in (".mp4", ".mkv", ".mov") else ".mkv"
    return p.with_name(f"{p.stem}.thuyet-minh{ext}")


def dub_video(video: str, cues: list[Cue], cfg: dict, log: Callable[[str], None] | None = None,
              progress: Callable[[int, int], None] | None = None, stop: threading.Event | None = None) -> Path:
    """Tạo file video mới: track 1 = thuyết minh (+ tiếng gốc nhỏ lại), track 2 = tiếng gốc."""
    log = log or (lambda m: None)
    # Tab File phim có giọng riêng ("dub_voice"); chưa chọn thì dùng giọng của tab Trực tiếp
    cfg = {**cfg, "voice": cfg.get("dub_voice") or cfg.get("voice", "windows")}
    info = probe(video)
    dubber = Dubber(cfg, log=log, progress=progress, stop=stop)
    if vieneu_tts.is_vieneu(cfg.get("voice", "")):
        vieneu_tts.ensure_models(log, dubber.stop)
        log("Giọng VieNeu chậm hơn giọng Windows nhiều (nhanh gấp ~1,6 lần thời gian thực trên CPU 14 nhân): "
            "phim 35 phút mất khoảng 9 phút, máy yếu hơn lâu hơn. Câu đã tạo được lưu lại, lần sau không phải tạo lại.")
    log(f"Đang tạo giọng cho {len(cues)} câu...")
    track, spans = dubber.build_track(cues, info.duration)
    if dubber.stop.is_set():
        raise RuntimeError("Đã huỷ.")
    s = dubber.stats
    log(f"Giọng: Edge {s['edge']}, Google {s['google']}, Windows {s['windows']}, VieNeu {s['vieneu']}, "
        f"dùng lại từ cache {s['cached']}; "
        f"tăng tốc {s['sped_up']} câu, {s['overflow']} câu vẫn dài hơn khoảng trống.")

    out = output_path(video)
    dub_wav = out.with_suffix(".dub.wav")
    _save_wav(dub_wav, track)
    log("Đang ghép tiếng vào video (không mã hoá lại hình)...")
    orig_vol = float(cfg.get("original_volume", 0.35))
    env_wav = out.with_suffix(".env.wav")
    cmd = [ffmpeg_exe(), "-y", "-hide_banner", "-loglevel", "error", "-i", video, "-i", str(dub_wav)]
    if info.has_audio:
        # pan chép nguyên kênh (aformat mono->stereo sẽ giảm 3 dB)
        dub_in = "[1:a]aresample=48000,pan=stereo|c0=c0|c1=c0,aformat=sample_fmts=fltp[dub];"
        orig_in = ("[0:a:0]aresample=48000," +
                   ("pan=stereo|c0=c0|c1=c0," if info.audio_mono else "aformat=channel_layouts=stereo,") +
                   "aformat=sample_fmts=fltp[orig];")
        if cfg.get("duck_original", True):
            # Tiếng gốc chỉ nhỏ đi lúc có lời thuyết minh; nhạc/hiệu ứng giữ nguyên khi không thoại
            env = duck_envelope(spans, info.duration, orig_vol)
            with wave.open(str(env_wav), "wb") as w:
                w.setnchannels(1)
                w.setsampwidth(2)
                w.setframerate(ENV_RATE)
                w.writeframes((env * 32767).astype(np.int16).tobytes())
            cmd += ["-i", str(env_wav)]
            fc = (dub_in + orig_in +
                  "[2:a]aresample=48000,pan=stereo|c0=c0|c1=c0,aformat=sample_fmts=fltp[env];"
                  "[orig][env]amultiply[bg];"
                  "[bg][dub]amix=inputs=2:duration=first:normalize=0[mix]")
        else:
            fc = (dub_in + orig_in + f"[orig]volume={orig_vol}[bg];"
                  "[bg][dub]amix=inputs=2:duration=first:normalize=0[mix]")
        cmd += ["-filter_complex", fc, "-map", "0:v:0", "-map", "[mix]", "-map", "0:a:0",
                "-metadata:s:a:0", "title=Thuyết minh", "-metadata:s:a:0", "language=vie",
                "-metadata:s:a:1", "title=Tiếng gốc", "-disposition:a:0", "default", "-disposition:a:1", "0"]
    else:
        cmd += ["-map", "0:v:0", "-map", "1:a", "-metadata:s:a:0", "title=Thuyết minh"]
    cmd += ["-c:v", "copy", "-c:a:0", "aac", "-b:a:0", "192k"]
    if info.has_audio:
        cmd += ["-c:a:1", "copy"]  # giữ nguyên track tiếng gốc
    cmd += ["-shortest", str(out)]
    r = subprocess.run(cmd, capture_output=True, text=True, encoding="utf-8", errors="replace",
                       creationflags=_NO_WINDOW)
    dub_wav.unlink(missing_ok=True)
    env_wav.unlink(missing_ok=True)
    if r.returncode != 0:
        raise RuntimeError(f"ffmpeg lỗi khi ghép tiếng:\n{r.stderr[-800:]}")
    log(f"Xong: {out}")
    return out
