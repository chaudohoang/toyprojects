"""Đọc văn bản thành giọng nói.

- Edge: giọng Neural tiếng Việt của Microsoft Edge (miễn phí, cần Internet, rất tự nhiên).
- Windows: giọng "Microsoft An" có sẵn trong Windows (offline, rất nhanh), đồng thời là giọng dự phòng khi Edge lỗi.
- VieNeu: 25 giọng VieNeu-TTS v3 Turbo (offline, tự nhiên hơn nhưng chậm: ~1,8 lần thời gian thực), xem vieneu_tts.py.
- Google: giọng "chị Google" của Google Dịch (online, nhanh), xem google_tts.py.

Để theo kịp phụ đề đổi nhanh:
- Cắt khoảng lặng đầu/cuối của âm thanh (giọng TTS thường thêm ~0,8 s im lặng ở cuối).
- Tự tăng tốc độ đọc theo "nhịp phụ đề" (số chữ/giây phụ đề chạy trên màn hình).
- Tổng hợp câu mới trong lúc câu cũ vẫn đang phát; câu cũ sắp xong thì cho đọc nốt.
"""
from __future__ import annotations

import array
import asyncio
import re
import threading
import time
from collections import deque
from typing import Callable

from . import google_tts, vieneu_tts

WINDOWS_VOICE = "windows"
VIENEU = "vieneu"  # khoá chung cho mọi giọng "vieneu:<tên>" khi ước lượng tốc độ
GOOGLE = google_tts.VOICE

VOICES = {
    "vi-VN-HoaiMyNeural": "Hoài My (nữ, online)",
    "vi-VN-NamMinhNeural": "Nam Minh (nam, online)",
    GOOGLE: google_tts.LABEL,
    WINDOWS_VOICE: "Microsoft An (nam, offline)",
    **vieneu_tts.voices(),
}

SAMPLE_RATE = 24000

# Số âm tiết/giây khi rate = +0% (đã cắt khoảng lặng), đo thực tế; sau đó tự hiệu chỉnh khi chạy.
_DEFAULT_SPS0 = {WINDOWS_VOICE: 4.8, "edge": 3.8, VIENEU: 4.0, GOOGLE: 3.4}

_WORD = re.compile(r"[^\W_]+", re.UNICODE)


def syllables(text: str) -> int:
    """Tiếng Việt: mỗi chữ cách nhau bằng dấu cách là một âm tiết."""
    return len(_WORD.findall(text))


def _pct(v: int) -> str:
    return f"{'+' if v >= 0 else ''}{int(v)}%"


def _trim(samples, threshold: int = 400, lead: float = 0.03, tail: float = 0.08):
    """Cắt khoảng lặng đầu/cuối (giữ lại một chút cho tự nhiên)."""
    import numpy as np

    a = np.frombuffer(samples, dtype=np.int16)
    loud = np.flatnonzero(np.abs(a.astype(np.int32)) > threshold)
    if len(loud) == 0:
        return samples
    start = max(int(loud[0]) - int(lead * SAMPLE_RATE), 0)
    end = min(int(loud[-1]) + int(tail * SAMPLE_RATE), len(a))
    return samples[start:end]


class Pacer:
    """Ước lượng nhịp phụ đề và chọn tốc độ đọc cho vừa."""

    def __init__(self, cfg: dict):
        self.cfg = cfg
        self._lock = threading.Lock()
        self._hist: deque[tuple[int, float]] = deque(maxlen=10)  # (số chữ, thời gian tới câu kế)
        self._last: tuple[int, float] | None = None
        self._sps0: dict[str, float] = dict(_DEFAULT_SPS0)

    def on_subtitle(self, text: str, t: float | None = None) -> None:
        t = time.monotonic() if t is None else t
        with self._lock:
            if self._last is not None:
                syl, t0 = self._last
                dt = t - t0
                if 0.3 < dt < self.cfg.get("pace_max_gap", 8.0):  # khoảng nghỉ dài giữa 2 đoạn thoại thì bỏ qua
                    self._hist.append((syl, dt))
            self._last = (syllables(text), t)

    def screen_rate(self) -> float | None:
        """Số chữ/giây phụ đề chạy trên màn hình (None nếu chưa đủ dữ liệu)."""
        with self._lock:
            if len(self._hist) < 2:
                return None
            return sum(s for s, _ in self._hist) / sum(d for _, d in self._hist)

    def rate_for(self, voice_key: str) -> tuple[int, float | None]:
        base = int(self.cfg.get("rate", 10))
        if not self.cfg.get("auto_speed", True):
            return base, None
        sps = self.screen_rate()
        if sps is None:
            return base, None
        # Muốn đọc xong trong ~pace_fill (85%) thời gian phụ đề hiện
        need = (sps / self.cfg.get("pace_fill", 0.85) / self._sps0[voice_key] - 1) * 100
        return int(max(-50, min(max(base, need), self.cfg.get("max_rate", 100)))), sps

    def calibrate(self, voice_key: str, text: str, rate: int, seconds: float) -> None:
        syl = syllables(text)
        if syl < 4 or seconds < 0.4:
            return
        observed = syl / (seconds * (1 + rate / 100))
        self._sps0[voice_key] = 0.7 * self._sps0[voice_key] + 0.3 * observed


class _Player:
    """Phát PCM 16-bit mono qua miniaudio, chạy nền; biết còn bao lâu thì xong."""

    def __init__(self):
        import miniaudio

        self._device = miniaudio.PlaybackDevice(
            output_format=miniaudio.SampleFormat.SIGNED16, nchannels=1, sample_rate=SAMPLE_RATE
        )
        self._done = threading.Event()
        self._done.set()
        self._stop = threading.Event()
        self._pos = 0
        self._n = 0

    def _gen(self, samples):
        self._pos, self._n = 0, len(samples)
        required = yield b""
        while self._pos < self._n and not self._stop.is_set():
            chunk = samples[self._pos:self._pos + required]
            self._pos += required
            required = yield chunk
        self._done.set()
        while True:  # giữ device im lặng tới khi bị thay generator
            required = yield b"\x00\x00" * required

    @property
    def playing(self) -> bool:
        return not self._done.is_set()

    def remaining(self) -> float:
        return 0.0 if self._done.is_set() else max(self._n - self._pos, 0) / SAMPLE_RATE

    def wait_done(self, timeout: float | None = None, abort: threading.Event | None = None) -> None:
        end = None if timeout is None else time.monotonic() + timeout
        while not self._done.wait(0.02):
            if abort is not None and abort.is_set():
                return
            if end is not None and time.monotonic() > end:
                return

    def play(self, samples) -> None:
        self.stop()
        self._stop.clear()
        self._done.clear()
        gen = self._gen(samples)
        next(gen)
        if self._device.running:
            self._device.stop()
        self._device.start(gen)

    def stop(self) -> None:
        self._stop.set()
        self._done.wait(0.3)

    def close(self) -> None:
        self.stop()
        try:
            self._device.close()
        except Exception:  # noqa: BLE001
            pass


class Speaker:
    """Đọc nền. say() trả về ngay.

    Mỗi câu phụ đề được tổng hợp giọng NGAY khi xuất hiện, song song với nhau (Edge mất ~0,7–2 s/câu,
    nếu huỷ câu cũ mỗi khi có câu mới thì phụ đề nhanh sẽ bị bỏ gần hết). Âm thanh phát theo thứ tự;
    chỉ bỏ một câu khi câu mới hơn đã có âm thanh trước nó (tức là câu đó đã quá trễ).
    """

    def __init__(self, cfg: dict, on_error: Callable[[str], None] | None = None,
                 on_info: Callable[[str], None] | None = None):
        self.cfg = cfg
        self.on_error = on_error or (lambda m: None)
        self.on_info = on_info or (lambda m: None)
        self.pacer = Pacer(cfg)
        self._closing = threading.Event()
        self._player: _Player | None = None
        self._win = None
        self._vn = None
        self._vn_failed = False
        self._edge_failures = 0
        self._edge_off_until = 0.0
        self._seq = 0
        self._latest_ready = 0
        self._loop = asyncio.new_event_loop()
        started = threading.Event()
        self._thread = threading.Thread(target=self._thread_main, args=(started,), daemon=True, name="speaker")
        self._thread.start()
        started.wait(5)

    # ---- public ----
    def say(self, text: str) -> None:
        if self._closing.is_set():
            return
        self.pacer.on_subtitle(text)
        self._loop.call_soon_threadsafe(self._enqueue, text)

    def close(self) -> None:
        if self._closing.is_set():
            return
        self._closing.set()
        if self._player:
            self._player.stop()

        def shutdown():
            for task in asyncio.all_tasks(self._loop):
                task.cancel()
            self._loop.stop()

        try:
            self._loop.call_soon_threadsafe(shutdown)
        except RuntimeError:
            pass

    # ---- internal (chạy trong luồng của event loop) ----
    def _thread_main(self, started: threading.Event):
        asyncio.set_event_loop(self._loop)
        self._jobs: asyncio.Queue = asyncio.Queue()
        self._win_lock = asyncio.Lock()
        self._vn_lock = asyncio.Lock()
        self._loop.create_task(self._playback())
        if vieneu_tts.is_vieneu(self.cfg.get("voice", "")):
            self._loop.create_task(self._synth_vieneu("", self.cfg["voice"], -1))  # nạp sẵn mô hình (~4 s)
        started.set()
        try:
            self._loop.run_forever()
            pending = asyncio.all_tasks(self._loop)
            if pending:
                self._loop.run_until_complete(asyncio.gather(*pending, return_exceptions=True))
        finally:
            if self._player:
                self._player.close()
            self._loop.close()

    def _enqueue(self, text: str):
        self._seq += 1
        seq = self._seq
        self._jobs.put_nowait((seq, text, self._loop.create_task(self._job(seq, text))))

    async def _job(self, seq: int, text: str):
        try:
            res = await self._synthesize(text, seq)
        except asyncio.CancelledError:
            raise
        except Exception as e:  # noqa: BLE001
            self.on_error(f"TTS lỗi: {e}")
            return None
        if res is None:
            return None
        samples, key, rate, sps = res
        samples = _trim(samples)
        self.pacer.calibrate(key, text, rate, len(samples) / SAMPLE_RATE)
        self._latest_ready = max(self._latest_ready, seq)
        return samples, key, rate, sps

    async def _playback(self):
        while True:
            seq, text, task = await self._jobs.get()
            res = await task
            if res is None or self._closing.is_set():
                continue
            interrupt = self.cfg.get("interrupt", True)
            if self._latest_ready > seq:
                continue  # câu mới hơn đã sẵn sàng -> bỏ câu này cho khỏi trễ hình
            if not interrupt and self._seq - seq > int(self.cfg.get("max_backlog", 1)):
                continue
            samples, key, rate, sps = res
            p = self._player = self._player or _Player()
            if p.playing:
                if not interrupt:
                    while p.playing and not self._closing.is_set():
                        await asyncio.sleep(0.02)
                else:
                    grace = self.cfg.get("finish_grace", 0.6)
                    if p.remaining() <= grace:  # câu cũ sắp xong -> đọc nốt
                        end = time.monotonic() + grace + 0.2
                        while p.playing and time.monotonic() < end:
                            await asyncio.sleep(0.02)
                    # còn dài -> cắt (play() dừng câu cũ)
            if self._closing.is_set():
                return
            p.play(samples)
            pace = f"nhịp phụ đề {sps:.1f} chữ/s → " if sps else ""
            name = {WINDOWS_VOICE: "Windows", VIENEU: "VieNeu", GOOGLE: "Google"}.get(key, "Edge")
            self.on_info(f"{pace}đọc {_pct(rate)} ({name})")

    async def _synthesize(self, text: str, seq: int = 0):
        """Trả về (samples, voice_key, rate đã dùng, nhịp phụ đề) hoặc None."""
        voice = self.cfg.get("voice", "vi-VN-HoaiMyNeural")
        if vieneu_tts.is_vieneu(voice) and not self._vn_failed:
            return await self._synth_vieneu(text, voice, seq)
        if voice == WINDOWS_VOICE or vieneu_tts.is_vieneu(voice) or time.monotonic() < self._edge_off_until:
            return await self._synth_windows(text)

        # Giọng online (Edge, Google): Edge đôi khi trả lỗi "No audio was received" ngẫu nhiên (mất ~2,8 s),
        # Google có thể tạm chặn khi gửi nhiều; mạng có thể chậm. Chờ tối đa edge_deadline giây,
        # sau đó đọc bằng giọng Windows offline để không lệch hình.
        if voice == GOOGLE:
            key, name = GOOGLE, "Giọng Google"
            rate, sps = self.pacer.rate_for(GOOGLE)
            job = self._synth_google(text, rate)
        else:
            key, name = "edge", "Edge TTS"
            rate, sps = self.pacer.rate_for("edge")
            job = self._synth_edge(text, voice, rate)
        try:
            samples = await asyncio.wait_for(job, self.cfg.get("edge_deadline", 2.0))
            reason = "không trả âm thanh"
        except asyncio.TimeoutError:
            samples, reason = None, "quá chậm"
        except Exception as e:  # noqa: BLE001  (edge_tts.exceptions.NoAudioReceived, lỗi mạng...)
            samples, reason = None, type(e).__name__
        if self._closing.is_set():
            return None
        if samples is not None:
            self._edge_failures = 0
            return samples, key, rate, sps
        self._edge_failures += 1
        if self._edge_failures >= 3:  # giọng online đang hỏng: tạm dùng giọng Windows 60 s cho khỏi trễ
            self._edge_failures = 0
            self._edge_off_until = time.monotonic() + self.cfg.get("edge_pause_seconds", 60)
            reason += f" 3 lần liền, tạm dùng giọng Windows {self.cfg.get('edge_pause_seconds', 60)} giây"
        if not self.cfg.get("edge_fallback", True):
            self.on_error(f"{name} {reason}, bỏ qua câu: {text[:40]}")
            return None
        self.on_error(f"{name} {reason} → đọc bằng giọng Windows")
        return await self._synth_windows(text)

    async def _synth_windows(self, text: str):
        async with self._win_lock:  # SpeechSynthesizer dùng chung -> tổng hợp lần lượt (rất nhanh)
            if self._win is None:
                self._win = _WindowsVoice()
            rate, sps = self.pacer.rate_for(WINDOWS_VOICE)
            return (await self._win.synth(text, rate, self.cfg.get("volume", 0), self.cfg.get("pitch", 0)),
                    WINDOWS_VOICE, rate, sps)

    async def _synth_vieneu(self, text: str, voice: str, seq: int):
        # Mô hình đọc lần lượt từng câu (~1–2 s/câu). Đến lượt mà đã có câu mới hơn thì bỏ câu này,
        # để không dồn hàng chờ làm tiếng trễ dần so với hình.
        async with self._vn_lock:
            if self._closing.is_set() or (text and self.cfg.get("interrupt", True) and self._seq > seq):
                return None
            try:
                if self._vn is None:
                    self.on_info("Đang nạp giọng VieNeu (vài giây)...")
                    self._vn = await asyncio.to_thread(vieneu_tts.VieNeuVoice, int(self.cfg.get("vieneu_threads", 0)))
                    self.on_info("Đã nạp giọng VieNeu")
                if not text:  # chỉ nạp sẵn
                    return None
                rate, sps = self.pacer.rate_for(VIENEU)
                pcm = await asyncio.to_thread(self._vn.synth, text, voice, int(self.cfg.get("volume", 0)))
            except Exception as e:  # noqa: BLE001  thiếu mô hình/thư viện -> dùng giọng Windows
                self._vn_failed = True
                self.on_error(f"Giọng VieNeu lỗi ({e}) → đọc bằng giọng Windows")
                return await self._synth_windows(text)
        pcm = _trim(pcm)  # mảng int16 (cắt theo mẫu, không theo byte)
        pcm = await asyncio.to_thread(vieneu_tts.time_stretch, pcm, 1 + rate / 100)
        return array.array("h", pcm.tobytes()), VIENEU, rate, sps

    async def _synth_google(self, text: str, rate: int):
        pcm = await asyncio.to_thread(google_tts.synth, text, int(self.cfg.get("volume", 0)), 5.0)
        if self._closing.is_set():
            return None
        pcm = _trim(pcm)  # mảng int16
        pcm = await asyncio.to_thread(google_tts.adjust, pcm, 1 + rate / 100, int(self.cfg.get("pitch", 0)))
        return array.array("h", pcm.tobytes())

    async def _synth_edge(self, text: str, voice: str, rate: int):
        return await edge_synth(text, voice, rate, self.cfg.get("volume", 0), cancel=self._closing,
                                pitch=self.cfg.get("pitch", 0))


def edge_pitch(pitch: int) -> str:
    """Cao độ % -> Hz cho Edge (giọng nam ~120 Hz nên 1% ≈ 1,2 Hz)."""
    return f"{round(pitch * 1.2):+d}Hz"


async def edge_synth(text: str, voice: str, rate: int, volume: int = 0, cancel: threading.Event | None = None,
                     pitch: int = 0):
    """Tổng hợp bằng Edge TTS -> PCM 16-bit mono 24 kHz (array 'h'), hoặc None nếu bị huỷ/không có âm thanh."""
    import edge_tts
    import miniaudio

    com = edge_tts.Communicate(text, voice, rate=_pct(rate), volume=_pct(volume), pitch=edge_pitch(pitch))
    mp3 = bytearray()
    async for chunk in com.stream():
        if cancel is not None and cancel.is_set():
            return None
        if chunk["type"] == "audio":
            mp3.extend(chunk["data"])
    if not mp3:
        return None
    decoded = miniaudio.decode(bytes(mp3), output_format=miniaudio.SampleFormat.SIGNED16,
                               nchannels=1, sample_rate=SAMPLE_RATE)
    return decoded.samples


class _WindowsVoice:
    """Giọng offline của Windows (Microsoft An, vi-VN). Cài: Settings → Language → Tiếng Việt → Text-to-speech."""

    def __init__(self):
        from winrt.windows.media.speechsynthesis import SpeechSynthesizer

        voices = list(SpeechSynthesizer.all_voices)
        voice = next((v for v in voices if v.language.lower().startswith("vi")), None)
        if voice is None:
            raise RuntimeError("Máy chưa có giọng Microsoft An (tiếng Việt). Cài: Settings → Time & language → Speech → Add voices → Tiếng Việt, hoặc chọn giọng HoaiMy/NamMinh (cần Internet).")
        self._synth = SpeechSynthesizer()
        self._synth.voice = voice

    async def synth(self, text: str, rate: int, volume: int, pitch: int = 0):
        import miniaudio
        from winrt.windows.storage.streams import DataReader

        opts = self._synth.options
        opts.speaking_rate = max(0.5, min(6.0, 1 + rate / 100))
        opts.audio_pitch = max(0.0, min(2.0, 1 + pitch / 100))  # 1.0 = gốc; giọng 6 = 0.8
        opts.audio_volume = max(0.0, min(1.0, 1 + volume / 100))
        stream = await self._synth.synthesize_text_to_stream_async(text)
        size = stream.size
        reader = DataReader(stream.get_input_stream_at(0))
        await reader.load_async(size)
        wav = bytearray(size)
        reader.read_bytes(wav)
        decoded = miniaudio.decode(bytes(wav), output_format=miniaudio.SampleFormat.SIGNED16,
                                   nchannels=1, sample_rate=SAMPLE_RATE)
        return decoded.samples


WindowsVoice = _WindowsVoice
trim_silence = _trim
