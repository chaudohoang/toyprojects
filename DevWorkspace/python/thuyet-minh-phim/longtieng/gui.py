"""Cửa sổ điều khiển nhỏ, luôn nổi trên cùng."""
from __future__ import annotations

import threading
import time
import tkinter as tk
from tkinter import messagebox, ttk

import cv2

from . import config, vieneu_tts
from .engine import DEBUG_DIR, Reader, grab_window_region, new_sct, recognize_frame
from .gui_file import FileTab
from .ocr import create_ocr
from .overlay import flash_region
from .screen import last_active_window, list_monitors, monitor_region, window_at
from .selector import select_region
from .tts import VOICES, Speaker


class App:
    def __init__(self):
        self.cfg = config.load()
        self.root = tk.Tk()
        self.root.title("Thuyết minh phim")
        self.root.attributes("-topmost", True)
        self.root.minsize(420, 380)
        self._selecting = False
        self._downloading = False
        self.reader = Reader(self.cfg, on_text=self._from_thread(self._add_line),
                             on_status=self._from_thread(self._set_status))
        self._build()
        self._register_hotkey()
        self._last_fg = None  # cửa sổ dùng ngay trước khi bấm vào app này (thường là cửa sổ phim)
        self._watch_foreground()
        self.root.protocol("WM_DELETE_WINDOW", self._on_close)

    # ---------- UI ----------
    def _build(self):
        pad = {"padx": 8, "pady": 4}
        nb = ttk.Notebook(self.root)
        nb.pack(fill="both", expand=True, padx=6, pady=(6, 0))
        f = ttk.Frame(nb)
        nb.add(f, text="Trực tiếp (màn hình)")
        self.file_tab = FileTab(nb, self)
        nb.add(self.file_tab.frame, text="File phim (thuyết minh sẵn)")
        tabs = ["live", "file"]
        nb.select(tabs.index(self.cfg.get("last_tab", "live")) if self.cfg.get("last_tab") in tabs else 0)
        nb.bind("<<NotebookTabChanged>>", lambda e: self._set("last_tab", tabs[nb.index(nb.select())]))

        row = ttk.Frame(f)
        row.pack(fill="x", **pad)
        ttk.Button(row, text=f"Chọn vùng phụ đề ({self.cfg['region_hotkey'].upper()})", command=self._pick_region).pack(side="left")
        ttk.Button(row, text=f"Thử đọc 1 lần ({self.cfg['test_hotkey'].upper()})", command=self._test_once).pack(side="left", padx=6)
        ttk.Button(row, text=f"Đổi màn hình ({self.cfg['monitor_hotkey'].upper()})", command=self._next_monitor).pack(side="left")

        row2 = ttk.Frame(f)
        row2.pack(fill="x", **pad)
        self.auto_var = tk.BooleanVar(value=self.cfg["auto_region"])
        ttk.Checkbutton(row2, text="Tự động: 1/3 dưới cửa sổ phim", variable=self.auto_var,
                        command=self._toggle_auto).pack(side="left")
        self.region_var = tk.StringVar(value=self._region_text())
        ttk.Label(row2, textvariable=self.region_var, foreground="#666").pack(side="left", padx=6)

        grid = ttk.Frame(f)
        grid.pack(fill="x", **pad)
        grid.columnconfigure(1, weight=1)

        ttk.Label(grid, text="Giọng đọc").grid(row=0, column=0, sticky="w")
        self.voice_var = tk.StringVar(value=VOICES.get(self.cfg["voice"], self.cfg["voice"]))
        cb = ttk.Combobox(grid, textvariable=self.voice_var, values=list(VOICES.values()), state="readonly")
        cb.grid(row=0, column=1, sticky="ew", pady=2)
        cb.bind("<<ComboboxSelected>>", lambda e: self._set_voice())

        ttk.Label(grid, text="Tốc độ (tối thiểu)").grid(row=1, column=0, sticky="w")
        self.rate_var = tk.IntVar(value=self.cfg["rate"])
        self._slider(grid, 1, self.rate_var, -50, 100, "rate", "%")

        ttk.Label(grid, text="Âm lượng").grid(row=2, column=0, sticky="w")
        self.vol_var = tk.IntVar(value=self.cfg["volume"])
        self._slider(grid, 2, self.vol_var, -50, 50, "volume", "%")

        ttk.Label(grid, text="Cao độ").grid(row=3, column=0, sticky="w")
        self.pitch_var = tk.IntVar(value=self.cfg["pitch"])
        self._slider(grid, 3, self.pitch_var, -50, 50, "pitch", "%")

        ttk.Label(grid, text="OCR").grid(row=4, column=0, sticky="w")
        self.ocr_var = tk.StringVar(value=self.cfg["ocr_engine"])
        ocb = ttk.Combobox(grid, textvariable=self.ocr_var, values=["auto", "windows", "tesseract"], state="readonly")
        ocb.grid(row=4, column=1, sticky="ew", pady=2)
        ocb.bind("<<ComboboxSelected>>", lambda e: self._set("ocr_engine", self.ocr_var.get()))

        opts = ttk.Frame(f)
        opts.pack(fill="x", **pad)
        self.int_var = tk.BooleanVar(value=self.cfg["interrupt"])
        ttk.Checkbutton(opts, text="Câu mới ngắt câu đang đọc", variable=self.int_var,
                        command=lambda: self._set("interrupt", self.int_var.get())).pack(side="left")
        self.dbg_var = tk.BooleanVar(value=self.cfg["save_debug_images"])
        ttk.Checkbutton(opts, text="Lưu ảnh debug", variable=self.dbg_var,
                        command=lambda: self._set("save_debug_images", self.dbg_var.get())).pack(side="left", padx=10)

        self.start_btn = ttk.Button(f, text=f"▶ Bắt đầu  ({self.cfg['hotkey'].upper()})", command=self.toggle)
        self.start_btn.pack(fill="x", **pad)

        self.log = tk.Text(f, height=8, wrap="word", font=("Segoe UI", 10), state="disabled")
        self.log.pack(fill="both", expand=True, **pad)

        self.status_var = tk.StringVar(value="Sẵn sàng")
        ttk.Label(self.root, textvariable=self.status_var, foreground="#0a5", wraplength=560).pack(
            fill="x", padx=14, pady=(2, 8))

    def _slider(self, parent, row, var, lo, hi, key, unit):
        frame = ttk.Frame(parent)
        frame.grid(row=row, column=1, sticky="ew", pady=2)
        lbl = ttk.Label(frame, width=6, text=f"{var.get():+d}{unit}")
        lbl.pack(side="right")

        def changed(_=None):
            v = int(float(var.get()))
            var.set(v)
            lbl.config(text=f"{v:+d}{unit}")
            self._set(key, v)

        ttk.Scale(frame, from_=lo, to=hi, variable=var, command=changed).pack(side="left", fill="x", expand=True)

    # ---------- helpers ----------
    def _watch_foreground(self):
        try:
            self._last_fg = last_active_window(self._last_fg)
        except Exception:  # noqa: BLE001
            pass
        self.root.after(500, self._watch_foreground)

    def _from_thread(self, fn):
        return lambda *a: self.root.after(0, fn, *a)

    def _region_text(self, desc: str = ""):
        r = self.cfg.get("region")
        if self.cfg.get("auto_region"):
            mon = self.cfg.get("monitor", 0)
            which = f"màn hình {mon}" if mon else "màn hình đang xem"
            return f"{desc or which}" + (f" → {r[2]}×{r[3]} tại ({r[0]},{r[1]})" if (r and desc) else "")
        return f"vùng kéo tay: {r[2]}×{r[3]} tại ({r[0]},{r[1]})" if r else "(chưa chọn)"

    def _resolve_region(self):
        """Tìm cửa sổ đang chiếu phim ngay lúc bấm. Chế độ tự động: vùng đọc = 1/3 dưới cửa sổ đó.
        Vùng kéo tay: giữ nguyên vùng, chỉ lấy cửa sổ phim ở giữa vùng để bỏ phần bị app khác đè lên."""
        if not self.cfg.get("auto_region"):
            r = self.cfg.get("region")
            self.reader.target = window_at(r[0] + r[2] // 2, r[1] + r[3] // 2) if r else None
            self.reader.follow = False
            return None
        region, mon, idx, desc, hwnd = monitor_region(self.cfg.get("monitor", 0),
                                                      self.cfg.get("auto_region_fraction", 1 / 3), self._last_fg)
        self.reader.target, self.reader.follow = hwnd, hwnd is not None
        self._set("region", region)
        self.region_var.set(self._region_text(desc))
        return region, mon, idx, desc

    def _next_monitor(self):
        """F4: Tự động → Màn hình 1 → 2 → ... → Tự động, và hiện khung đỏ trên màn hình vừa chọn."""
        n = len(list_monitors())
        self._set("monitor", (self.cfg.get("monitor", 0) + 1) % (n + 1))
        if not self.cfg.get("auto_region"):
            self._set("auto_region", True)
            self.auto_var.set(True)
        region, mon, idx, desc = self._resolve_region()
        if self.cfg["monitor"]:
            title, sub = f"Màn hình {idx}", f"cố định • {self.cfg['monitor_hotkey'].upper()} để đổi tiếp"
        else:
            title, sub = "Tự động", f"đang là màn hình {idx} ({desc.split(', ', 1)[-1]})"
        flash_region(self.root, mon, region, title, sub)
        self._set_status(f"Vùng phụ đề: {title.lower()} — {sub}")
        if self.reader.running:  # áp dụng ngay
            self.toggle()
            self.toggle()

    def _toggle_auto(self):
        self._set("auto_region", self.auto_var.get())
        self.region_var.set(self._region_text())
        if self.reader.running:  # áp dụng ngay
            self.toggle()
            self.toggle()

    def _set(self, key, value):
        self.cfg[key] = value
        config.save(self.cfg)

    def _set_voice(self):
        self.pick_voice(self.voice_var, "voice")

    def pick_voice(self, var: tk.StringVar, key: str):
        """Lưu giọng chọn ở combobox `var` vào cfg[key] ("voice": tab Trực tiếp, "dub_voice": tab File phim).
        Giọng VieNeu mà chưa có mô hình thì hỏi tải trước."""
        name = var.get()
        voice = next((k for k, v in VOICES.items() if v == name), name)
        if not vieneu_tts.is_vieneu(voice):
            self._set(key, voice)
            if voice == "google":
                self._set_status("Giọng chị Google: cần Internet; mất mạng thì tạm đọc bằng giọng Windows. "
                                 "Kéo Cao độ xuống để có giọng trầm hơn.")
            return
        cur = self.cfg.get(key) or self.cfg["voice"]
        old = VOICES.get(cur, cur)
        if key == "voice" and not vieneu_tts.is_vieneu(cur) and not messagebox.askyesno(
                "Thuyết minh phim",
                "Giọng VieNeu tạo tiếng chậm: ở chế độ Trực tiếp mỗi câu sẽ đọc trễ thêm khoảng 0,5–3 giây "
                "(câu càng dài càng trễ), dễ lệch với phụ đề.\n\n"
                "Để xem trực tiếp nên dùng giọng Microsoft (gần như tức thì); VieNeu hợp với tab File phim.\n\n"
                "Vẫn dùng VieNeu cho tab Trực tiếp?"):
            var.set(old)
            return
        need = vieneu_tts.missing_bytes()
        if not need:
            self._set(key, voice)
            if key == "voice":
                self._set_status("Giọng VieNeu ở chế độ Trực tiếp: mỗi câu trễ thêm ~0,5–3 giây; "
                                 "không chỉnh được cao độ.")
            else:
                self._set_status("Giọng VieNeu: offline, tự nhiên hơn nhưng tạo giọng chậm (~1,6 lần thời gian "
                                 "thực); không chỉnh được cao độ.")
            return
        if self._downloading or not messagebox.askyesno(
                "Thuyết minh phim",
                f"Giọng VieNeu cần tải mô hình khoảng {need / 1e6:.0f} MB từ HuggingFace (chỉ tải một lần).\n\n"
                "Tải ngay bây giờ?"):
            var.set(old)
            return
        self._downloading = True

        def progress(done, total):
            self.root.after(0, self._set_status, f"Đang tải mô hình VieNeu {done * 100 // total}% "
                                                 f"({done / 1e6:.0f}/{total / 1e6:.0f} MB)...")

        def work():
            try:
                vieneu_tts.download(progress)
                err = None
            except Exception as e:  # noqa: BLE001
                err = e
            self.root.after(0, finish, err)

        def finish(err):
            self._downloading = False
            if err is not None:
                var.set(old)
                self._set_status("Tải mô hình VieNeu thất bại")
                messagebox.showerror("Thuyết minh phim", f"Không tải được mô hình VieNeu:\n{err}")
                return
            if var.get() == name:
                self._set(key, voice)
            self._set_status("Đã tải xong mô hình VieNeu. Giọng này không chỉnh được cao độ.")

        threading.Thread(target=work, daemon=True).start()

    def _add_line(self, text):
        self.log.config(state="normal")
        self.log.insert("end", f"{time.strftime('%H:%M:%S')}  {text}\n")
        self.log.see("end")
        self.log.config(state="disabled")

    def _set_status(self, text):
        self.status_var.set(text)
        if text.startswith("Đã dừng") or not self.reader.running:
            self.start_btn.config(text=f"▶ Bắt đầu  ({self.cfg['hotkey'].upper()})")

    # ---------- actions ----------
    def _pick_region(self):
        if self._selecting:  # bấm phím tắt lần nữa khi đang chọn
            return
        self._selecting = True
        was_running = self.reader.running
        if was_running:
            self.reader.stop()
        self.root.withdraw()
        self.root.update()
        time.sleep(0.25)
        try:
            region = select_region(self.root)
        finally:
            self.root.deiconify()
            self._selecting = False
        if region:
            self._set("auto_region", False)  # đã kéo tay thì tắt tự động
            self.auto_var.set(False)
            self._set("region", region)
            self.region_var.set(self._region_text())
        if was_running:
            self.toggle()

    def _test_once(self):
        self._resolve_region()
        if not self.cfg.get("region"):
            messagebox.showinfo("Thuyết minh phim", "Hãy chọn vùng phụ đề trước.")
            return
        try:
            ocr = create_ocr(self.cfg)
            with new_sct() as sct:
                frame = grab_window_region(sct, self.cfg["region"], self.reader.target)
            raw, text, prepared = recognize_frame(frame, ocr, self.cfg)
        except Exception as e:  # noqa: BLE001
            messagebox.showerror("Thuyết minh phim", str(e))
            return
        DEBUG_DIR.mkdir(exist_ok=True)
        cv2.imwrite(str(DEBUG_DIR / "test_frame.png"), frame)
        cv2.imwrite(str(DEBUG_DIR / "test_ocr.png"), prepared)
        self._add_line(f"[thử] {text or '(không thấy chữ)'}")
        self._set_status(f"OCR {ocr.name}: thô = {raw!r}  — ảnh lưu ở thư mục debug/")
        if text:
            sp = Speaker(self.cfg, on_error=self._from_thread(self._set_status))
            sp.say(text)
            self.root.after(15000, sp.close)

    def toggle(self):
        if self.reader.running:
            self.reader.stop()
            self.start_btn.config(text=f"▶ Bắt đầu  ({self.cfg['hotkey'].upper()})")
        else:
            try:
                self._resolve_region()
                self.reader.start()
            except RuntimeError as e:
                messagebox.showinfo("Thuyết minh phim", str(e))
                return
            self.start_btn.config(text=f"■ Dừng  ({self.cfg['hotkey'].upper()})")
            if vieneu_tts.is_vieneu(self.cfg.get("voice", "")):
                self._add_line("⚠ Đang dùng giọng VieNeu: mỗi câu đọc trễ thêm ~0,5–3 giây. "
                               "Muốn đọc kịp phụ đề hãy chọn giọng Microsoft.")

    def _hotkey_actions(self):
        return {"hotkey": self.toggle, "region_hotkey": self._pick_region, "test_hotkey": self._test_once,
                "monitor_hotkey": self._next_monitor}

    def _register_hotkey(self):
        try:
            import keyboard

            for key, action in self._hotkey_actions().items():
                keyboard.add_hotkey(self.cfg[key], self._from_thread(action))
        except Exception as e:  # noqa: BLE001
            self._set_status(f"Không đăng ký được phím tắt: {e}")

    def _on_close(self):
        self.reader.stop()
        config.save(self.cfg)
        self.root.destroy()

    def run(self):
        self.root.mainloop()
