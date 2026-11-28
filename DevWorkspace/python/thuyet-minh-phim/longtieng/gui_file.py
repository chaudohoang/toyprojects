"""Tab "File phim": quét phụ đề -> .srt -> thuyết minh -> xuất video mới."""
from __future__ import annotations

import os
import threading
import time
import tkinter as tk
from pathlib import Path
from tkinter import filedialog, messagebox, ttk

from .batch import BatchQueue, _natural_key, collect_videos, remove_sources, srt_path
from .folder_dialog import pick_folders
from .dubber import dub_video, output_path
from .srt import read_srt, write_srt
from .subscan import scan_video
from .tts import VOICES

VIDEO_TYPES = [("Video", "*.mp4 *.mkv *.avi *.mov *.webm *.ts *.flv *.m4v"), ("Tất cả", "*.*")]
__all__ = ["FileTab", "srt_path"]


class FileTab:
    def __init__(self, parent: ttk.Notebook, app):
        self.app = app
        self.cfg = app.cfg
        self.root = app.root
        self.frame = ttk.Frame(parent)
        self._stop = threading.Event()
        self._busy = False
        self._last_out: Path | None = None  # video vừa xuất (vẫn mở được khi phim gốc đã bị xoá)
        self._queue: BatchQueue | None = None  # hàng chờ "Cả thư mục" đang chạy
        self._build()

    # ---------- UI ----------
    def _build(self):
        pad = {"padx": 8, "pady": 4}
        f = self.frame

        row = ttk.Frame(f)
        row.pack(fill="x", **pad)
        self.video_var = tk.StringVar(value=self.cfg.get("last_video") or "")
        ttk.Entry(row, textvariable=self.video_var).pack(side="left", fill="x", expand=True)
        ttk.Button(row, text="Chọn phim...", command=self._choose).pack(side="left", padx=(6, 0))
        self.batch_btn = ttk.Button(row, text="Cả thư mục...", command=self._batch)
        self.batch_btn.pack(side="left", padx=(6, 0))

        # Giọng riêng khi xuất file (vd tab Trực tiếp dùng Microsoft An cho nhanh, xuất file dùng VieNeu cho hay)
        vrow = ttk.Frame(f)
        vrow.pack(fill="x", **pad)
        ttk.Label(vrow, text="Giọng thuyết minh").pack(side="left")
        if not self.cfg.get("dub_voice"):  # lần đầu: lấy theo tab Trực tiếp, từ đó hai giọng độc lập
            self.app._set("dub_voice", self.cfg["voice"])
        cur = self.cfg["dub_voice"]
        self.voice_var = tk.StringVar(value=VOICES.get(cur, cur))
        cb = ttk.Combobox(vrow, textvariable=self.voice_var, values=list(VOICES.values()), state="readonly")
        cb.pack(side="left", fill="x", expand=True, padx=(6, 0))
        cb.bind("<<ComboboxSelected>>", lambda e: self.app.pick_voice(self.voice_var, "dub_voice"))

        steps = ttk.Frame(f)
        steps.pack(fill="x", **pad)
        self.scan_btn = ttk.Button(steps, text="1. Quét phụ đề", command=self._scan)
        self.scan_btn.pack(side="left")
        self.edit_btn = ttk.Button(steps, text="Sửa phụ đề (.srt)", command=self._open_srt)
        self.edit_btn.pack(side="left", padx=6)
        self.dub_btn = ttk.Button(steps, text="2. Thuyết minh & xuất", command=self._dub)
        self.dub_btn.pack(side="left")
        self.play_btn = ttk.Button(steps, text="▶ Xem", command=self._play)
        self.play_btn.pack(side="left", padx=6)
        self.cancel_btn = ttk.Button(steps, text="Huỷ", command=self._stop.set, state="disabled")
        self.cancel_btn.pack(side="right")

        vol = ttk.Frame(f)
        vol.pack(fill="x", **pad)
        ttk.Label(vol, text="Tiếng gốc khi có lời").pack(side="left")
        self.orig_var = tk.IntVar(value=int(round(self.cfg.get("original_volume", 0.3) * 100)))
        lbl = ttk.Label(vol, width=5, text=f"{self.orig_var.get()}%")
        lbl.pack(side="right")

        def changed(_=None):
            v = int(float(self.orig_var.get()))
            self.orig_var.set(v)
            lbl.config(text=f"{v}%")
            self.app._set("original_volume", v / 100)

        ttk.Scale(vol, from_=0, to=100, variable=self.orig_var, command=changed).pack(side="left", fill="x",
                                                                                     expand=True, padx=6)

        self.delete_var = tk.BooleanVar(value=bool(self.cfg.get("delete_source", False)))
        ttk.Checkbutton(f, variable=self.delete_var,
                        text="Xuất xong thì xoá file phim gốc và .srt (chuyển vào Thùng rác) — cả tập lẻ và cả thư mục",
                        command=lambda: self.app._set("delete_source", self.delete_var.get())).pack(anchor="w", **pad)

        self.progress = ttk.Progressbar(f, maximum=1000)
        self.progress.pack(fill="x", **pad)
        self.log = tk.Text(f, height=9, wrap="word", font=("Segoe UI", 9), state="disabled")
        self.log.pack(fill="both", expand=True, **pad)
        ttk.Label(f, foreground="#666", wraplength=520, text=(
            "Tốc độ tối thiểu và âm lượng lấy theo tab Trực tiếp. File mới lưu cạnh phim gốc: "
            "<tên>.thuyet-minh.srt và <tên>.thuyet-minh.mp4/.mkv (track 1 thuyết minh, track 2 tiếng gốc).")).pack(
            fill="x", **pad)
        self._refresh()

    # ---------- helpers ----------
    def _ui(self, fn, *a):
        self.root.after(0, fn, *a)

    def _write(self, msg: str):
        self.log.config(state="normal")
        self.log.insert("end", f"{time.strftime('%H:%M:%S')}  {msg}\n")
        self.log.see("end")
        self.log.config(state="disabled")

    def _video(self) -> str | None:
        v = self.video_var.get().strip().strip('"')
        return v if v and os.path.isfile(v) else None

    def _refresh(self):
        v = self._video()
        has_srt = bool(v and srt_path(v).exists())
        has_out = bool(v and output_path(v).exists()) or bool(self._last_out and self._last_out.exists())
        busy = self._busy
        self.scan_btn.config(state="disabled" if busy or not v else "normal")
        queued = self._queue is not None  # đang chạy hàng chờ: nút thành "Thêm thư mục..."
        self.batch_btn.config(state="normal" if queued or not busy else "disabled",
                              text="Thêm thư mục..." if queued else "Cả thư mục...")
        self.edit_btn.config(state="normal" if has_srt and not busy else "disabled")
        self.dub_btn.config(state="normal" if has_srt and not busy else "disabled")
        self.play_btn.config(state="normal" if has_out and not busy else "disabled")
        self.cancel_btn.config(state="normal" if busy else "disabled")

    def _choose(self):
        path = filedialog.askopenfilename(title="Chọn file phim", filetypes=VIDEO_TYPES)
        if path:
            self.video_var.set(os.path.normpath(path))
            self.app._set("last_video", self.video_var.get())
            v = self.video_var.get()
            if srt_path(v).exists():
                self._write(f"Đã có phụ đề từ lần trước ({len(read_srt(srt_path(v)))} câu): có thể thuyết minh luôn.")
        self._refresh()

    def _run_bg(self, work, done, cleanup=None):
        self._busy = True
        self._stop.clear()
        self._refresh()

        def runner():
            try:
                result, err = work(), None
            except Exception as e:  # noqa: BLE001
                result, err = None, e
            self._ui(finish, result, err)

        def finish(result, err):
            self._busy = False
            if cleanup:
                cleanup()
            self._refresh()
            if err is not None:
                self._write(f"Lỗi: {err}")
                if not self._stop.is_set():
                    messagebox.showerror("Thuyết minh phim", str(err))
            else:
                done(result)

        threading.Thread(target=runner, daemon=True).start()

    # ---------- actions ----------
    def _scan(self):
        video = self._video()
        if not video:
            return
        self.app._set("last_video", video)
        self._write(f"Quét phụ đề: {Path(video).name}")
        t0 = time.time()

        def progress(frac, n):
            self._ui(self.progress.config, {"value": frac * 1000})
            self._ui(self.app._set_status, f"Đang quét {frac * 100:.0f}% — {n} đoạn")

        def work():
            cues = scan_video(video, dict(self.cfg), progress, self._stop, log=lambda m: self._ui(self._write, m))
            if self._stop.is_set():
                raise RuntimeError("Đã huỷ quét.")
            write_srt(srt_path(video), cues)
            return cues

        def done(cues):
            self._write(f"Xong: {len(cues)} câu trong {time.time() - t0:.0f} giây → {srt_path(video).name}")
            self._write("Bấm \"Sửa phụ đề\" để xem/sửa chữ sai (lưu lại file), rồi bấm \"2. Thuyết minh & xuất\".")
            self.app._set_status(f"Quét xong {len(cues)} câu")

        self._run_bg(work, done)

    def _open_srt(self):
        v = self._video()
        if v and srt_path(v).exists():
            os.startfile(srt_path(v))  # noqa: S606  mở bằng trình soạn thảo mặc định

    def _dub(self):
        video = self._video()
        if not video or not srt_path(video).exists():
            return
        cues = read_srt(srt_path(video))
        if not cues:
            messagebox.showinfo("Thuyết minh phim", "File phụ đề trống.")
            return
        t0 = time.time()

        def progress(done, total):
            self._ui(self.progress.config, {"value": done / max(total, 1) * 1000})
            self._ui(self.app._set_status, f"Đang tạo giọng {done}/{total} câu")

        delete = self.delete_var.get()

        def work():
            log = lambda m: self._ui(self._write, m)  # noqa: E731
            out = dub_video(video, cues, dict(self.cfg), log=log, progress=progress, stop=self._stop)
            if delete:
                remove_sources(video, log)
            return out

        def done(out):
            self._last_out = Path(out)
            self._write(f"Hoàn tất sau {time.time() - t0:.0f} giây. Bấm ▶ Xem để mở phim.")
            self.app._set_status(f"Đã xuất: {Path(out).name}")
            self._refresh()

        self._run_bg(work, done)

    def _batch(self):
        adding = self._queue is not None  # đang chạy -> thêm vào hàng chờ
        title = ("Chọn thư mục phim để thêm vào hàng chờ" if adding else "Chọn thư mục phim") + \
            " (giữ Ctrl để chọn nhiều thư mục; chọn thư mục cha thì lấy cả thư mục con)"
        last = self.cfg.get("last_folder") or None
        picked = pick_folders(int(self.root.wm_frame(), 16), title, last)
        if picked is None:  # không mở được hộp chọn nhiều -> hộp chọn một thư mục
            one = filedialog.askdirectory(title=title, initialdir=last)
            picked = [one] if one else []
        if not picked:
            return
        picked = sorted({os.path.normpath(p) for p in picked}, key=_natural_key)
        folder = picked[0] if len(picked) == 1 else f"{len(picked)} thư mục ({', '.join(Path(p).name for p in picked)})"
        self.app._set("last_folder", str(Path(picked[0]).parent) if len(picked) > 1 else picked[0])
        videos, seen = [], set()
        for p in picked:  # chọn cả thư mục cha lẫn thư mục con thì không lặp tập
            for v in collect_videos(p):
                if v not in seen:
                    seen.add(v)
                    videos.append(v)
        if adding:  # chọn lại thư mục cũ (vd đã tải thêm phim): chỉ lấy tập chưa có trong hàng chờ
            videos = [v for v in videos if not self._queue.has(v)]
            if not videos:
                messagebox.showinfo("Thuyết minh phim", "Không có tập mới: các tập ở đây đã nằm trong hàng chờ.")
                return
        if not videos:
            messagebox.showinfo("Thuyết minh phim", "Các thư mục đã chọn (và thư mục con) không có file phim nào.")
            return
        done = sum(output_path(v).exists() for v in videos)
        has_srt = sum(srt_path(v).exists() and not output_path(v).exists() for v in videos)
        todo = len(videos) - done
        if not todo:
            messagebox.showinfo("Thuyết minh phim", f"Cả {len(videos)} tập đã có bản thuyết minh.")
            return
        folders = sorted({v.parent for v in videos}, key=lambda p: _natural_key(str(p)))
        common = os.path.commonpath([str(p) for p in folders]) if len(folders) > 1 else ""
        where = (f"{len(videos)} tập trong {len(folders)} thư mục:\n"
                 + "\n".join(f"  • {os.path.relpath(p, common)}: {sum(v.parent == p for v in videos)} tập"
                             for p in folders[:8])
                 + ("\n  …" if len(folders) > 8 else "")) if len(folders) > 1 else \
            (f"{len(videos)} tập trong thư mục:\n" + "\n".join(f"  • {v.name}" for v in videos[:6])
             + ("\n  …" if len(videos) > 6 else ""))
        plan = (f"Quét phụ đề + thuyết minh {todo} tập"
                + (f" ({has_srt} tập đã có phụ đề, dùng luôn)" if has_srt else "")
                + (f"; bỏ qua {done} tập đã xuất" if done else "") + ".")
        if adding:
            if messagebox.askyesno("Thuyết minh phim", f"{where}\n\n{plan}\n\nThêm vào cuối hàng chờ?"):
                n = self._queue.add(videos)
                self._write(f"Thêm vào hàng chờ: {folder} ({n} tập mới, cả hàng chờ {self._queue.total} tập)")
            return
        voice = self.cfg.get("dub_voice") or self.cfg["voice"]
        workers = int(self.cfg.get("batch_scan_workers", 2))
        msg = (f"{where}\n\n{plan}\nGiọng: {VOICES.get(voice, voice)}.\n"
               + ("Xuất xong tập nào thì chuyển phim gốc + .srt của tập đó vào Thùng rác.\n"
                  if self.delete_var.get() else "") + "\n"
               f"Quét {workers} tập cùng lúc, song song với thuyết minh tập đã quét. Đang chạy vẫn bấm "
               "\"Thêm thư mục...\" được. Huỷ giữa chừng thì lần sau chạy lại sẽ làm tiếp. Bắt đầu?")
        if not messagebox.askyesno("Thuyết minh phim", msg):
            return
        self._write(f"Chạy hàng chờ: {folder} ({len(videos)} tập)")
        t0 = time.time()

        def progress(frac, text):
            self._ui(self.progress.config, {"value": frac * 1000})
            if text:
                self._ui(self.app._set_status, text)

        delete = self.delete_var.get()
        self._queue = BatchQueue(dict(self.cfg), log=lambda m: self._ui(self._write, m), progress=progress,
                                 stop=self._stop, delete_source=delete)
        self._stop.clear()
        self._queue.add(videos)

        def work():
            return self._queue.run()

        def finished(res):
            mins = (time.time() - t0) / 60
            text = (f"Xong hàng chờ sau {mins:.0f} phút: {len(res['done'])} tập mới, "
                    f"bỏ qua {len(res['skipped'])}, lỗi {len(res['failed'])}"
                    + (f", đã xoá phim gốc của {len(res['deleted'])} tập" if delete else "") + ".")
            if self._stop.is_set():
                text = f"Đã huỷ sau {mins:.0f} phút ({len(res['done'])} tập đã xong; chạy lại sẽ làm tiếp)."
            self._write(text)
            for name, err in res["failed"]:
                self._write(f"  Lỗi: {name}: {err}")
            self.app._set_status(text)
            if res["outputs"]:
                self._last_out = Path(res["outputs"][-1])  # ▶ Xem mở tập vừa xuất (kể cả khi phim gốc đã xoá)
            self._refresh()

        def cleanup():  # cả khi run() lỗi bất ngờ
            self._queue = None

        self._run_bg(work, finished, cleanup=cleanup)

    def _play(self):
        v = self._video()
        out = output_path(v) if v and output_path(v).exists() else self._last_out
        if out and out.exists():
            os.startfile(out)  # noqa: S606  mở bằng trình phát mặc định
