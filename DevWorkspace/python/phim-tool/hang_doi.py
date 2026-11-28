"""Tab hàng đợi: dán nhiều link phim, tool tải lần lượt từng phim (mỗi phim
một thư mục con), trong mỗi phim tải vài tập song song.

Hàng đợi được lưu vào hang_doi.json cạnh tool, đóng tool mở lại vẫn còn;
bấm "Bắt đầu" lại thì các tập đã tải xong được bỏ qua, tập dở được tải tiếp.
"""
import json
import os
import re
import threading
import time
import tkinter as tk
from concurrent.futures import ThreadPoolExecutor
from tkinter import filedialog, messagebox, ttk

import downloader
from lay_link_tap import episode_base, get_episodes, get_video_url, movie_slug, safe_filename
from sites import find_site

TOOL_DIR = os.path.dirname(os.path.abspath(__file__))
SAVE_FILE = os.path.join(TOOL_DIR, "hang_doi.json")
DEFAULT_ROOT = os.path.join(os.path.expanduser("~"), "Videos", "Phim")
URL_RE = re.compile(r"https?://[^\s\"'<>]+")

# Trạng thái một phim
CHO, DANG, XONG, LOI, DUNG = "cho", "dang", "xong", "loi", "dung"


class QueuePanel(ttk.Frame):
    """Khung hàng đợi, đặt trong một tab của cửa sổ chính."""

    def __init__(self, master):
        super().__init__(master)

        self.items = []          # mỗi phim: dict url, folder, site, state, ...
        self.lock = threading.Lock()
        self.stop_event = threading.Event()
        self.running = False

        # --- Ô dán link ---
        top = ttk.Frame(self, padding=(10, 10, 10, 4))
        top.pack(fill="x")
        ttk.Label(top, text="Dán link các phim (mỗi dòng một link):").pack(anchor="w")
        box = ttk.Frame(top)
        box.pack(fill="x", pady=(2, 4))
        self.text = tk.Text(box, height=5, wrap="none", undo=True)
        self.text.pack(side="left", fill="x", expand=True)
        ttk.Button(box, text="Thêm vào\nhàng đợi",
                   command=self.add_from_text).pack(side="left", padx=(6, 0), fill="y")

        row = ttk.Frame(top)
        row.pack(fill="x")
        ttk.Label(row, text="Lưu vào thư mục:").pack(side="left")
        self.root_var = tk.StringVar(value=DEFAULT_ROOT)
        ttk.Entry(row, textvariable=self.root_var).pack(
            side="left", fill="x", expand=True, padx=6)
        ttk.Button(row, text="Chọn...", command=self.choose_root).pack(side="left")
        ttk.Label(row, text="   Số tập tải cùng lúc:").pack(side="left")
        self.workers_var = tk.IntVar(value=2)
        ttk.Spinbox(row, from_=1, to=4, width=3, state="readonly",
                    textvariable=self.workers_var).pack(side="left", padx=(4, 0))

        # --- Bảng hàng đợi ---
        mid = ttk.Frame(self, padding=(10, 4))
        mid.pack(fill="both", expand=True)
        self.tree = ttk.Treeview(mid, columns=("stt", "phim", "trang", "tien_do", "url"),
                                 show="headings", selectmode="extended")
        for col, text, width, stretch in (
                ("stt", "#", 40, False), ("phim", "Phim (thư mục)", 220, False),
                ("trang", "Trang", 80, False), ("tien_do", "Tiến độ", 380, True),
                ("url", "Link", 280, True)):
            self.tree.heading(col, text=text)
            self.tree.column(col, width=width, stretch=stretch,
                             anchor="center" if col in ("stt", "trang") else "w")
        sb = ttk.Scrollbar(mid, orient="vertical", command=self.tree.yview)
        self.tree.configure(yscrollcommand=sb.set)
        self.tree.pack(side="left", fill="both", expand=True)
        sb.pack(side="right", fill="y")
        self.tree.bind("<Double-1>", lambda e: self.open_folder())
        self.tree.bind("<Delete>", lambda e: self.remove_selected())
        self.tree.bind("<<TreeviewSelect>>", lambda e: self.show_errors())

        # Dòng chi tiết lỗi của phim đang chọn
        self.err_var = tk.StringVar()
        self.err_label = ttk.Label(self, textvariable=self.err_var, foreground="#b00020",
                                   padding=(12, 0), wraplength=1000, justify="left")
        self.err_label.pack(fill="x")
        self.err_label.bind("<Configure>", lambda e: self.err_label.config(
            wraplength=max(200, e.width - 24)))

        # --- Nút ---
        bottom = ttk.Frame(self, padding=(10, 4, 10, 10))
        bottom.pack(fill="x")
        self.start_btn = ttk.Button(bottom, text="▶ Bắt đầu tải", command=self.start)
        self.start_btn.pack(side="left")
        self.stop_btn = ttk.Button(bottom, text="Dừng", state="disabled",
                                   command=self.stop)
        self.stop_btn.pack(side="left", padx=6)
        ttk.Button(bottom, text="Lên", width=5,
                   command=lambda: self.move(-1)).pack(side="left")
        ttk.Button(bottom, text="Xuống", width=6,
                   command=lambda: self.move(1)).pack(side="left", padx=(2, 6))
        ttk.Button(bottom, text="Xoá dòng đã chọn",
                   command=self.remove_selected).pack(side="left")
        ttk.Button(bottom, text="Xoá phim đã xong",
                   command=self.remove_done).pack(side="left", padx=6)
        ttk.Button(bottom, text="Mở thư mục",
                   command=self.open_folder).pack(side="left")
        self.status = tk.StringVar(value="Dán link rồi bấm “Thêm vào hàng đợi”.")
        ttk.Label(bottom, textvariable=self.status,
                  foreground="#555").pack(side="right")

        self.load()

    # ---------------------------------------------------------- dữ liệu
    def load(self):
        try:
            with open(SAVE_FILE, encoding="utf-8") as f:
                data = json.load(f)
        except (OSError, ValueError):
            return
        if data.get("root"):
            self.root_var.set(data["root"])
        for it in data.get("items", []):
            if not it.get("url") or not it.get("folder"):
                continue
            state = it.get("state", CHO)
            if state == DANG:
                state = DUNG
            self._insert(it["url"], it["folder"], it.get("site", ""), state,
                         it.get("text", ""), it.get("errors") or {})
        if self.items:
            n = sum(1 for it in self.items if it["state"] != XONG)
            self.status.set(f"Đã mở lại hàng đợi: {n} phim chưa xong.")

    def save(self):
        with self.lock:
            data = {"root": self.root_var.get().strip(),
                    "items": [{k: it[k] for k in ("url", "folder", "site", "state", "text", "errors")}
                              for it in self.items]}
        try:
            with open(SAVE_FILE + ".tmp", "w", encoding="utf-8") as f:
                json.dump(data, f, ensure_ascii=False, indent=1)
            os.replace(SAVE_FILE + ".tmp", SAVE_FILE)
        except OSError:
            pass

    def _insert(self, url, folder, site, state=CHO, text="", errors=None):
        it = {"url": url, "folder": folder, "site": site, "state": state,
              "text": text or self._state_text(state), "last_ui": 0.0,
              "errors": errors or {}}     # {tập: lý do lỗi}
        it["iid"] = self.tree.insert("", "end", values=(
            len(self.items) + 1, os.path.basename(folder), site, it["text"], url))
        self.items.append(it)
        return it

    @staticmethod
    def _state_text(state):
        return {CHO: "chờ", DANG: "đang chạy...", XONG: "xong ✓",
                LOI: "lỗi", DUNG: "đã dừng"}.get(state, "")

    def _renumber(self):
        for n, it in enumerate(self.items, 1):
            self.tree.set(it["iid"], "stt", n)

    # ---------------------------------------------------------- thêm / xoá
    def choose_root(self):
        folder = filedialog.askdirectory(parent=self, title="Chọn thư mục lưu phim",
                                         initialdir=self.root_var.get() or None)
        if folder:
            self.root_var.set(os.path.normpath(folder))
            self.save()

    def _unique_folder(self, root, url, site):
        folder = os.path.join(root, safe_filename(movie_slug(url)))
        taken = {it["folder"].lower() for it in self.items if it["url"] != url}
        if folder.lower() in taken:          # trùng tên phim ở trang khác
            folder += f" ({site})"
        return folder

    def add_from_text(self):
        root = self.root_var.get().strip()
        if not root:
            self.choose_root()
            root = self.root_var.get().strip()
            if not root:
                return
        urls = URL_RE.findall(self.text.get("1.0", "end"))
        if not urls:
            self.status.set("Không thấy link nào trong ô.")
            return
        bad = self.add_urls(urls, root)
        self.text.delete("1.0", "end")
        if bad:                               # để lại link lỗi trong ô cho người dùng xem
            self.text.insert("1.0", "\n".join(bad))

    def add_urls(self, urls, root=None):
        """Thêm các link vào cuối hàng đợi; trả về các link chưa hỗ trợ."""
        root = root or self.root_var.get().strip() or DEFAULT_ROOT
        have = {it["url"] for it in self.items if it["state"] != XONG}
        added, dup, bad = 0, 0, []
        for url in dict.fromkeys(u.rstrip(".,;)") for u in urls):
            if url in have:
                dup += 1
                continue
            try:
                site = find_site(url).NAME
            except Exception:
                bad.append(url)
                continue
            self._insert(url, self._unique_folder(root, url, site), site)
            have.add(url)
            added += 1
        msg = f"Đã thêm {added} phim."
        if dup:
            msg += f" {dup} link đã có trong hàng đợi."
        if bad:
            msg += f" {len(bad)} link thuộc trang chưa hỗ trợ."
        self.status.set(msg)
        self.save()
        return bad

    def show_errors(self):
        """Hiện lý do lỗi từng tập của phim đang chọn."""
        sel = self._selected_items()
        errs = sel[0]["errors"] if len(sel) == 1 else {}
        self.err_var.set("\n".join(f"Tập {tap}: {msg}" if tap else msg
                                   for tap, msg in errs.items()))

    def _selected_items(self):
        sel = set(self.tree.selection())
        return [it for it in self.items if it["iid"] in sel]

    def remove_selected(self):
        sel = self._selected_items()
        if any(it["state"] == DANG for it in sel):
            self.status.set("Không xoá được phim đang tải — bấm Dừng trước.")
            sel = [it for it in sel if it["state"] != DANG]
        with self.lock:
            for it in sel:
                self.items.remove(it)
                self.tree.delete(it["iid"])
        self._renumber()
        self.save()

    def remove_done(self):
        with self.lock:
            done = [it for it in self.items if it["state"] == XONG]
            for it in done:
                self.items.remove(it)
                self.tree.delete(it["iid"])
        self._renumber()
        self.status.set(f"Đã xoá {len(done)} phim đã xong khỏi danh sách (file vẫn giữ).")
        self.save()

    def move(self, step):
        sel = self._selected_items()
        if len(sel) != 1:
            return
        it = sel[0]
        with self.lock:
            i = self.items.index(it)
            j = i + step
            if not 0 <= j < len(self.items):
                return
            self.items[i], self.items[j] = self.items[j], self.items[i]
        self.tree.move(it["iid"], "", j)
        self._renumber()
        self.save()

    def open_folder(self):
        sel = self._selected_items()
        path = sel[0]["folder"] if sel else self.root_var.get().strip()
        while path and not os.path.isdir(path):   # chưa tải thì mở thư mục cha
            parent = os.path.dirname(path)
            if parent == path:
                return
            path = parent
        if path:
            os.startfile(path)

    # ---------------------------------------------------------- chạy
    def start(self):
        if self.running:
            return
        with self.lock:
            todo = [it for it in self.items if it["state"] != XONG]
            for it in todo:
                it["state"] = CHO
        if not todo:
            self.status.set("Không còn phim nào cần tải.")
            return
        for it in todo:
            self._show(it, self._state_text(CHO))
        self.running = True
        self.stop_event.clear()
        self.start_btn.config(state="disabled")
        self.stop_btn.config(state="normal")
        threading.Thread(target=self._worker, daemon=True).start()

    def stop(self):
        if self.running:
            self.stop_event.set()
            self.status.set("Đang dừng... (bấm Bắt đầu lần sau sẽ tải tiếp từ chỗ dở)")

    def _worker(self):
        while not self.stop_event.is_set():
            with self.lock:          # lấy phim chờ đầu tiên (kể cả phim vừa thêm lúc đang chạy)
                it = next((x for x in self.items if x["state"] == CHO), None)
                if it:
                    it["state"] = DANG
            if it is None:
                break
            self._process(it)
            self.after(0, self.save)
        self.after(0, self._worker_done)

    def _worker_done(self):
        self.running = False
        self.start_btn.config(state="normal")
        self.stop_btn.config(state="disabled")
        n = {s: sum(1 for it in self.items if it["state"] == s) for s in (XONG, LOI, CHO, DUNG)}
        if self.stop_event.is_set():
            self.status.set(f"Đã dừng. {n[XONG]} phim xong.")
        else:
            msg = f"Hết hàng đợi: {n[XONG]} phim xong."
            if n[LOI]:
                msg += f" {n[LOI]} phim có lỗi (bấm Bắt đầu để thử lại)."
            self.status.set(msg)
        self.save()

    def _show(self, it, text, force=True):
        """Cập nhật ô tiến độ (gọi được từ luồng khác; tự giới hạn 2 lần/giây)."""
        now = time.monotonic()
        if not force and now - it["last_ui"] < 0.5:
            return
        it["last_ui"] = now
        it["text"] = text
        self.after(0, self._set_cell, it["iid"], text)

    def _set_cell(self, iid, text):
        if self.tree.exists(iid):
            self.tree.set(iid, "tien_do", text)

    def _process(self, it):
        self.after(0, self.status.set, f"Đang tải: {os.path.basename(it['folder'])}")
        it["errors"] = {}
        self._show(it, "đang lấy danh sách tập...")
        try:
            eps = get_episodes(it["url"])
            if not eps:
                raise RuntimeError("Không tìm thấy tập nào.")
            os.makedirs(it["folder"], exist_ok=True)
        except Exception as e:
            it["state"] = LOI
            it["errors"] = {"": f"Không lấy được danh sách tập: {e}"}
            self._show(it, f"lỗi: {e}")
            return

        slug = movie_slug(it["url"])
        total = len(eps)
        st = {"ok": 0, "failed": {}, "active": {}, "note": ""}   # failed: {tập: lý do}
        st_lock = threading.Lock()

        def summary():
            with st_lock:
                parts = [f"{st['ok']}/{total} tập"]
                if st["failed"]:
                    parts.append("lỗi: " + ", ".join(f"T{t}" for t in st["failed"]))
                parts += [f"T{tap}: {txt}" for tap, txt in st["active"].items()]
                if st["note"]:
                    parts.append(st["note"])
            return " · ".join(parts)

        def one(ep):
            if self.stop_event.is_set():
                return "dung"
            tap = ep["tap"]
            base = episode_base(it["folder"], slug, ep)
            if downloader.existing_file(base):
                with st_lock:
                    st["ok"] += 1
                self._show(it, summary())
                return "ok"
            with st_lock:
                st["active"][tap] = "lấy link"
            self._show(it, summary(), force=False)

            def prog(pct, detail):
                with st_lock:
                    st["active"][tap] = f"{pct}%" if pct is not None else detail[:22]
                self._show(it, summary(), force=False)

            err = None
            try:
                vid = get_video_url(ep)
                finished = downloader.download(vid, base, prog, self.stop_event)
                result = "ok" if finished else "dung"
            except Exception as e:
                result, err = "loi", str(e) or type(e).__name__
            with st_lock:
                st["active"].pop(tap, None)
                if result == "ok":
                    st["ok"] += 1
                    st["failed"].pop(tap, None)
                elif result == "loi":
                    st["failed"][tap] = err
            self._show(it, summary())
            return result

        workers = max(1, min(4, int(self.workers_var.get() or 2)))
        with ThreadPoolExecutor(max_workers=workers) as pool:
            results = list(pool.map(one, eps))

            # Tải xong cả phim mà còn tập lỗi → nghỉ một chút rồi tự thử lại một lần
            if st["failed"] and "dung" not in results:
                with st_lock:
                    st["note"] = "chờ thử lại tập lỗi..."
                self._show(it, summary())
                if not self.stop_event.wait(15):
                    with st_lock:
                        st["note"] = ""
                        retry = [ep for ep in eps if ep["tap"] in st["failed"]]
                    results += list(pool.map(one, retry))
                with st_lock:
                    st["note"] = ""

        it["errors"] = dict(st["failed"])
        if self.stop_event.is_set() or "dung" in results:
            it["state"] = DUNG
            self._show(it, f"đã dừng · {st['ok']}/{total} tập")
        elif st["failed"]:
            it["state"] = LOI
            self._show(it, f"xong {st['ok']}/{total} tập · lỗi: "
                           + ", ".join(f"T{t}" for t in st["failed"])
                           + " (chọn dòng để xem lý do)")
        else:
            it["state"] = XONG
            self._show(it, f"xong ✓ {total}/{total} tập")
        self.after(0, self.show_errors)
