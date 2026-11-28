"""Chạy hàng chờ nhiều thư mục phim: quét phụ đề + thuyết minh + xuất video cho từng tập.

- Thư mục chọn vào được tìm cả thư mục con (một thư mục chứa nhiều bộ phim cũng được); các tập xếp theo số tập.
- Đang chạy vẫn thêm thư mục khác vào hàng chờ được.
- Quét 2 tập cùng lúc (batch_scan_workers): một lần quét chưa dùng hết CPU, 2 luồng nhanh hơn ~1,5 lần
  (3 luồng không nhanh thêm). Song song đó một luồng tạo giọng + ghép tập đã quét xong (chủ yếu chờ mạng / ffmpeg).
- Tập đã có .srt thì dùng luôn (có thể đã sửa tay), tập đã có video thuyết minh thì bỏ qua:
  huỷ giữa chừng rồi chạy lại sẽ làm tiếp từ tập còn dở.
- Một tập lỗi thì ghi lại rồi làm tiếp tập sau.
- Tuỳ chọn delete_source: xuất xong tập nào thì chuyển phim gốc + .srt của tập đó vào Thùng rác.
"""
from __future__ import annotations

import os
import queue
import re
import threading
from pathlib import Path
from typing import Callable

from .dubber import dub_video, output_path
from .srt import read_srt, write_srt
from .subscan import scan_video

VIDEO_EXTS = {".mp4", ".mkv", ".avi", ".mov", ".webm", ".ts", ".flv", ".m4v"}
_OUTPUT_TAGS = (".thuyet-minh", ".long-tieng")


def srt_path(video: str | Path) -> Path:
    # Không đặt trùng tên phim (.srt) để trình phát khỏi tự bật thêm phụ đề mềm đè lên phụ đề in cứng
    p = Path(video)
    new, old = p.with_name(f"{p.stem}.thuyet-minh.srt"), p.with_name(f"{p.stem}.long-tieng.srt")
    return old if old.exists() and not new.exists() else new  # file quét bằng bản cũ (tên "lồng tiếng")


def _natural_key(name: str):
    """"Tap 2" đứng trước "Tap 10"."""
    return [int(t) if t.isdigit() else t.lower() for t in re.split(r"(\d+)", name)]


def list_videos(folder: str | Path) -> list[Path]:
    """Các file phim ngay trong thư mục (không tính file do app xuất ra), xếp theo số tập."""
    files = [p for p in Path(folder).iterdir()
             if p.is_file() and p.suffix.lower() in VIDEO_EXTS and not p.stem.lower().endswith(_OUTPUT_TAGS)]
    return sorted(files, key=lambda p: _natural_key(p.name))


def collect_videos(folder: str | Path) -> list[Path]:
    """Phim trong thư mục và mọi thư mục con: hết thư mục này mới sang thư mục kế, theo thứ tự tên/số."""
    out: list[Path] = []
    for root, dirs, _files in os.walk(folder):
        dirs.sort(key=_natural_key)  # os.walk đi theo thứ tự này
        try:
            out += list_videos(root)
        except OSError:
            pass
    return out


def _recycle(paths: list[Path]) -> None:
    """Chuyển file vào Thùng rác (khôi phục được). Ổ không có Thùng rác thì Windows hỏi trước khi xoá hẳn."""
    import ctypes
    from ctypes import wintypes

    class SHFILEOPSTRUCTW(ctypes.Structure):
        _fields_ = [("hwnd", wintypes.HWND), ("wFunc", wintypes.UINT), ("pFrom", wintypes.LPCWSTR),
                    ("pTo", wintypes.LPCWSTR), ("fFlags", ctypes.c_uint16), ("fAnyOperationsAborted", wintypes.BOOL),
                    ("hNameMappings", ctypes.c_void_p), ("lpszProgressTitle", wintypes.LPCWSTR)]

    FO_DELETE, FOF_SILENT, FOF_NOCONFIRMATION, FOF_ALLOWUNDO = 3, 0x4, 0x10, 0x40
    FOF_NOERRORUI, FOF_WANTNUKEWARNING = 0x400, 0x4000
    op = SHFILEOPSTRUCTW(wFunc=FO_DELETE, pFrom="\0".join(str(p.resolve()) for p in paths) + "\0\0",
                         fFlags=FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_SILENT | FOF_NOERRORUI | FOF_WANTNUKEWARNING)
    rc = ctypes.windll.shell32.SHFileOperationW(ctypes.byref(op))
    if rc != 0 or op.fAnyOperationsAborted:
        raise OSError(f"không xoá được (mã {rc:#x})" if rc else "đã huỷ xoá")


def remove_sources(video: str | Path, log: Callable[[str], None]) -> bool:
    """Sau khi xuất xong: chuyển file phim gốc + .srt vào Thùng rác.
    Chỉ làm khi file thuyết minh có thật và dài bằng phim gốc (lệch < 3 giây)."""
    from .video import probe

    video = Path(video)
    out = output_path(video)
    try:
        ok = out.is_file() and abs(probe(str(out)).duration - probe(str(video)).duration) < 3.0
    except Exception:  # noqa: BLE001
        ok = False
    if not ok:
        log(f"Giữ lại {video.name}: file thuyết minh chưa có hoặc độ dài không khớp phim gốc")
        return False
    p = video
    files = [video] + [s for s in (p.with_name(f"{p.stem}.thuyet-minh.srt"), p.with_name(f"{p.stem}.long-tieng.srt"))
                       if s.is_file()]
    try:
        _recycle(files)
    except OSError as e:
        log(f"Không xoá được {video.name}: {e}")
        return False
    log("Đã chuyển vào Thùng rác: " + ", ".join(f.name for f in files))
    return True


class BatchQueue:
    """Hàng chờ các tập phim. add() được gọi cả khi run() đang chạy (từ luồng khác)."""

    def __init__(self, cfg: dict, log: Callable[[str], None],
                 progress: Callable[[float, str], None] | None = None,
                 stop: threading.Event | None = None, delete_source: bool = False,
                 scan_workers: int | None = None):
        self.cfg = cfg
        self.log = log
        self.progress = progress or (lambda frac, text: None)
        self.stop = stop or threading.Event()
        self.delete_source = delete_source
        self.workers = max(1, int(scan_workers or cfg.get("batch_scan_workers", 2)))
        self.res: dict = {"done": [], "skipped": [], "failed": [], "deleted": [], "outputs": []}
        self._items: list[Path] = []
        self._keys: set[str] = set()
        self._next = 0          # tập kế tiếp chờ quét
        self._scanning = 0      # số luồng quét đang chạy
        self._wid = 0           # số thứ tự luồng quét (để hiện trạng thái riêng từng luồng)
        self._finished = 0      # số tập đã xử lý xong (kể cả bỏ qua / lỗi)
        self._ready: queue.PriorityQueue = queue.PriorityQueue()  # (thứ tự, tập, câu, lỗi)
        self._lock = threading.RLock()
        self._status: dict[str, str] = {}
        self._dub_frac = 0.0

    # ---------- dùng từ ngoài ----------
    def add(self, videos: list[Path]) -> int:
        """Thêm tập vào cuối hàng chờ (bỏ tập đã có trong hàng). Trả về số tập mới thêm."""
        added = 0
        with self._lock:
            for v in videos:
                key = os.path.normcase(str(Path(v).resolve()))
                if key not in self._keys:
                    self._keys.add(key)
                    self._items.append(Path(v))
                    added += 1
            self._ensure_scanners()
        if added:
            self._report()
        return added

    @property
    def total(self) -> int:
        return len(self._items)

    def has(self, video: str | Path) -> bool:
        """Tập này đã nằm trong hàng chờ chưa."""
        with self._lock:
            return os.path.normcase(str(Path(video).resolve())) in self._keys

    def run(self) -> dict:
        """Chạy tới khi hết hàng chờ (hoặc bị huỷ). Luồng gọi run() làm việc tạo giọng + ghép."""
        with self._lock:
            self._ensure_scanners()
        while True:
            try:
                _, i, v, cues, err = self._ready.get(timeout=0.2)
            except queue.Empty:
                with self._lock:
                    if self._scanning == 0 and (self._next >= len(self._items) or self.stop.is_set()):
                        if self._ready.empty():
                            break
                continue
            if not self.stop.is_set():
                self._dub_one(i, v, cues, err)
            with self._lock:
                self._finished += 1
                self._dub_frac = 0.0
                self._status.pop("dub", None)
            self._report()
        return self.res

    # ---------- bên trong ----------
    def _tag(self, i: int) -> str:
        return f"[{i + 1}/{len(self._items)}]"

    def _report(self):
        with self._lock:
            n = max(len(self._items), 1)
            frac = min((self._finished + self._dub_frac) / n, 1.0)
            text = " | ".join(t for _, t in sorted(self._status.items()) if t)
        self.progress(frac, text)

    def _ensure_scanners(self):
        # gọi khi đang giữ self._lock
        while self._scanning < self.workers and self._next < len(self._items) and not self.stop.is_set():
            self._scanning += 1
            self._wid += 1
            threading.Thread(target=self._scanner, args=(self._wid,), daemon=True, name="batch-scan").start()

    def _take(self) -> int | None:
        with self._lock:
            if self.stop.is_set() or self._next >= len(self._items):
                self._scanning -= 1
                return None
            i = self._next
            self._next += 1
            return i

    def _scanner(self, wid: int):
        key = f"scan{wid}"
        while (i := self._take()) is not None:
            v = self._items[i]
            try:
                self._ready.put((i, i, v, *self._scan_one(i, v, key)))
            except Exception as e:  # noqa: BLE001
                self._ready.put((i, i, v, None, e))
            finally:
                with self._lock:
                    self._status.pop(key, None)

    def _scan_one(self, i: int, v: Path, key: str):
        """Trả về (câu, lỗi); lỗi = "skip" nếu đã xuất từ trước."""
        if output_path(v).exists():
            return None, "skip"
        sp = srt_path(v)
        if sp.exists():
            cues = read_srt(sp)
            self.log(f"{self._tag(i)} {v.name}: dùng phụ đề có sẵn ({len(cues)} câu)")
            return cues, None
        self.log(f"{self._tag(i)} Quét phụ đề: {v.name}")

        def prog(frac, cnt):
            with self._lock:
                self._status[key] = f"Tập {i + 1}/{len(self._items)}: quét {frac * 100:.0f}%"
            self._report()

        cues = scan_video(str(v), dict(self.cfg), prog, self.stop, log=lambda m: self.log(f"{self._tag(i)}   {m}"))
        if self.stop.is_set():
            return None, "stop"
        write_srt(sp, cues)
        self.log(f"{self._tag(i)} Quét xong: {len(cues)} câu → {sp.name}")
        return cues, None

    def _dub_one(self, i: int, v: Path, cues, err):
        tag = self._tag(i)
        if err == "stop":
            return
        if err == "skip":
            self.log(f"{tag} {v.name}: đã có {output_path(v).name}, bỏ qua")
            self.res["skipped"].append(v.name)
            return
        if err is not None:
            self.log(f"{tag} LỖI khi quét {v.name}: {err}")
            self.res["failed"].append((v.name, str(err)))
            return
        if not cues:
            self.log(f"{tag} {v.name}: không thấy phụ đề nào, bỏ qua")
            self.res["failed"].append((v.name, "không thấy phụ đề"))
            return
        self.log(f"{tag} Thuyết minh & xuất: {v.name} ({len(cues)} câu)")

        def dprog(done, total):
            with self._lock:
                self._dub_frac = done / max(total, 1) * 0.9  # 10% cuối là ghép video
                self._status["dub"] = f"Tập {i + 1}/{len(self._items)}: tạo giọng {done}/{total}"
            self._report()

        try:
            out = dub_video(str(v), cues, dict(self.cfg), log=lambda m: self.log(f"{tag}   {m}"),
                            progress=dprog, stop=self.stop)
        except Exception as e:  # noqa: BLE001
            if not self.stop.is_set():
                self.log(f"{tag} LỖI khi thuyết minh {v.name}: {e}")
                self.res["failed"].append((v.name, str(e)))
            return
        self.log(f"{tag} Xong → {Path(out).name}")
        self.res["done"].append(v.name)
        self.res["outputs"].append(str(out))
        if self.delete_source and remove_sources(v, lambda m: self.log(f"{tag}   {m}")):
            self.res["deleted"].append(v.name)


def run_batch(videos: list[Path], cfg: dict, log: Callable[[str], None],
              progress: Callable[[float, str], None] | None = None,
              stop: threading.Event | None = None, delete_source: bool = False) -> dict:
    """Chạy một danh sách tập (không thêm được khi đang chạy). Xem BatchQueue."""
    q = BatchQueue(cfg, log, progress, stop, delete_source)
    q.add(videos)
    return q.run()
