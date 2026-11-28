"""Giao diện lấy link / tải tất cả các tập phim (phimvn2, tvhay/tvphim, motchill).

Mở bằng cách nhấp đúp file này, hoặc:  pythonw lay_link_tap_ui.pyw
"""
import threading
import tkinter as tk
import webbrowser
from tkinter import filedialog, messagebox, ttk

import os
import time
from concurrent.futures import ThreadPoolExecutor

import downloader
import grid_open
import hang_doi
from lay_link_tap import (episode_base, get_episodes, get_video_url, movie_folder,
                          movie_slug)

def video_text(vid):
    """Chữ hiện ở cột Link video: kèm tên bản (Thuyết minh / Vietsub) nếu biết."""
    url = vid["sd"] or vid["hd"]
    return f"[{vid['ban']}] {url}" if vid.get("ban") else url


DEFAULT_URL = "https://phimvn2.pro/phim/thanh-tra-vo-hinh-shadow-detective-2971"


class App(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title("Lấy link tập phim")
        self.geometry("1100x600")
        self.minsize(640, 380)
        self.episodes = []

        # Hai tab trong cùng một cửa sổ: một phim / hàng đợi nhiều phim
        self.tabs = ttk.Notebook(self)
        self.tabs.pack(fill="both", expand=True, padx=6, pady=6)
        single = ttk.Frame(self.tabs)
        self.tabs.add(single, text="  Một phim  ")
        self.queue = hang_doi.QueuePanel(self.tabs)
        self.tabs.add(self.queue, text="  Hàng đợi nhiều phim  ")
        self.protocol("WM_DELETE_WINDOW", self.on_close)

        # --- Hàng nhập URL ---
        top = ttk.Frame(single, padding=(10, 10, 10, 4))
        top.pack(fill="x")
        ttk.Label(top, text="URL trang phim:").pack(side="left")
        self.url_var = tk.StringVar(value=DEFAULT_URL)
        self.url_entry = ttk.Entry(top, textvariable=self.url_var)
        self.url_entry.pack(side="left", fill="x", expand=True, padx=6)
        self.url_entry.bind("<Return>", lambda e: self.fetch())
        self.fetch_btn = ttk.Button(top, text="Lấy link", command=self.fetch)
        self.fetch_btn.pack(side="left")

        # --- Bảng kết quả ---
        mid = ttk.Frame(single, padding=(10, 4))
        mid.pack(fill="both", expand=True)
        self.tree = ttk.Treeview(mid, columns=("tap", "tai", "url", "video"),
                                 show="headings", selectmode="extended")
        self.tree.heading("tai", text="Tải về")
        self.tree.column("tai", width=130, anchor="center", stretch=False)
        self.tree.heading("tap", text="Tập")
        self.tree.heading("url", text="Link trang xem")
        self.tree.heading("video", text="Link video (.mp4 / .m3u8)")
        self.tree.column("tap", width=70, anchor="center", stretch=False)
        self.tree.column("url", width=420)
        self.tree.column("video", width=520)
        sb = ttk.Scrollbar(mid, orient="vertical", command=self.tree.yview)
        self.tree.configure(yscrollcommand=sb.set)
        self.tree.pack(side="left", fill="both", expand=True)
        sb.pack(side="right", fill="y")
        self.tree.bind("<Double-1>", self.open_selected)
        self.tree.bind("<Control-c>", lambda e: self.copy_selected())
        self.tree.bind("<Control-a>", lambda e: self.tree.selection_set(
            self.tree.get_children()))

        # --- Hàng nút ---
        bottom = ttk.Frame(single, padding=(10, 4, 10, 10))
        bottom.pack(fill="x")
        ttk.Button(bottom, text="Copy tất cả",
                   command=self.copy_all).pack(side="left")
        ttk.Button(bottom, text="Copy dòng đã chọn",
                   command=self.copy_selected).pack(side="left", padx=6)
        ttk.Button(bottom, text="Lưu ra file .txt",
                   command=self.save).pack(side="left")
        self.grid_btn = ttk.Button(bottom, text="Mở tất cả (lưới)",
                                   command=self.open_grid)
        self.grid_btn.pack(side="left", padx=6)
        self.video_btn = ttk.Button(bottom, text="Lấy link video",
                                    command=self.fetch_videos)
        self.video_btn.pack(side="left")
        ttk.Button(bottom, text="Copy link video",
                   command=self.copy_videos).pack(side="left", padx=6)

        # --- Hàng tải video ---
        dl = ttk.Frame(single, padding=(10, 0, 10, 10))
        dl.pack(fill="x")
        self.dl_btn = ttk.Button(dl, text="⬇ Tải video về máy",
                                 command=self.download)
        self.dl_btn.pack(side="left")
        self.stop_btn = ttk.Button(dl, text="Dừng tải", state="disabled",
                                   command=self.stop_download)
        self.stop_btn.pack(side="left", padx=6)
        ttk.Button(dl, text="＋ Thêm phim này vào hàng đợi",
                   command=self.add_to_queue).pack(side="left")
        self.status = tk.StringVar(
            value="Dán URL rồi bấm “Lấy link”. Nhấp đúp một dòng để mở trong trình duyệt.")
        ttk.Label(dl, textvariable=self.status,
                  foreground="#555").pack(side="right")
        self.stop_event = threading.Event()
        self.movie_slug = "phim"
        self.page_url = ""
        self.dl_folder = None

        self.url_entry.focus_set()
        self.url_entry.select_range(0, "end")

    # ---------- Hành động ----------
    def fetch(self):
        url = self.url_var.get().strip()
        if not url:
            messagebox.showwarning("Thiếu URL", "Hãy dán URL trang phim.")
            return
        self.fetch_btn.config(state="disabled")
        self.status.set("Đang tải trang...")
        self.config(cursor="watch")
        threading.Thread(target=self._fetch_worker, args=(url,),
                         daemon=True).start()

    def _fetch_worker(self, url):
        try:
            eps, err = get_episodes(url), None
        except Exception as e:  # lỗi mạng, URL sai...
            eps, err = [], e
        self.after(0, self._fetch_done, eps, err)

    def _fetch_done(self, eps, err):
        self.fetch_btn.config(state="normal")
        self.config(cursor="")
        if err:
            self.status.set("Lỗi.")
            messagebox.showerror("Lỗi khi tải trang", str(err))
            return
        self.episodes = eps
        self.page_url = self.url_var.get().strip()
        self.movie_slug = movie_slug(self.page_url)
        self.tree.delete(*self.tree.get_children())
        for i, ep in enumerate(eps):
            self.tree.insert("", "end", iid=str(i),
                             values=(ep["tap"], "", ep["url"], ""))
        if eps:
            self.status.set(f"Tìm thấy {len(eps)} tập.")
        else:
            self.status.set("Không tìm thấy tập nào trên trang này.")

    def _selected_urls(self):
        return [self.episodes[int(i)]["url"] for i in self.tree.selection()]

    def _copy(self, urls):
        if not urls:
            self.status.set("Chưa có link để copy.")
            return
        self.clipboard_clear()
        self.clipboard_append("\n".join(urls))
        self.status.set(f"Đã copy {len(urls)} link.")

    def copy_all(self):
        self._copy([ep["url"] for ep in self.episodes])

    def copy_selected(self):
        self._copy(self._selected_urls())

    def open_selected(self, _event=None):
        for url in self._selected_urls()[:1]:
            webbrowser.open(url)

    # ---------- Link video ----------
    def fetch_videos(self):
        """Lấy link video cho các dòng đang chọn (hoặc tất cả nếu không chọn)."""
        sel = self.tree.selection()
        idx = [int(i) for i in sel] if sel else list(range(len(self.episodes)))
        if not idx:
            self.status.set("Chưa có tập nào. Bấm “Lấy link” trước.")
            return
        self.video_btn.config(state="disabled")
        for i in idx:
            self.tree.set(str(i), "video", "đang lấy...")
        threading.Thread(target=self._video_worker, args=(idx,),
                         daemon=True).start()

    def _video_worker(self, idx):
        done = [0]

        def one(i):
            try:
                vid = get_video_url(self.episodes[i])
                text = video_text(vid)
            except Exception as e:
                vid, text = None, f"Lỗi: {e}"
            done[0] += 1
            self.after(0, self._video_one_done, i, vid, text, done[0], len(idx))

        with ThreadPoolExecutor(max_workers=4) as pool:
            list(pool.map(one, idx))
        self.after(0, lambda: self.video_btn.config(state="normal"))

    def _video_one_done(self, i, vid, text, n_done, n_total):
        self.episodes[i]["video"] = vid
        self.tree.set(str(i), "video", text)
        self.status.set(f"Đã lấy link video {n_done}/{n_total} tập.")

    def copy_videos(self):
        sel = self.tree.selection()
        idx = [int(i) for i in sel] if sel else range(len(self.episodes))
        urls = [v["sd"] or v["hd"] for v in
                (self.episodes[i].get("video") for i in idx) if v]
        if not urls:
            self.status.set("Chưa có link video. Bấm “Lấy link video” trước.")
            return
        self._copy(urls)

    # ---------- Tải video ----------
    def download(self):
        """Tải các tập đang chọn (hoặc tất cả) về một thư mục."""
        sel = self.tree.selection()
        idx = [int(i) for i in sel] if sel else list(range(len(self.episodes)))
        if not idx:
            self.status.set("Chưa có tập nào. Bấm “Lấy link” trước.")
            return
        folder = filedialog.askdirectory(
            title="Chọn thư mục lưu video", initialdir=self.dl_folder or None)
        if not folder:
            return
        self.dl_folder = folder                       # nhớ thư mục gốc cho lần sau
        folder = os.path.normpath(movie_folder(folder, self.page_url))
        os.makedirs(folder, exist_ok=True)
        self.status.set(f"Lưu vào: {folder}")
        self.stop_event.clear()
        self.dl_btn.config(state="disabled")
        self.stop_btn.config(state="normal")
        for i in idx:
            self.tree.set(str(i), "tai", "chờ...")
        threading.Thread(target=self._download_worker, args=(idx, folder),
                         daemon=True).start()

    def stop_download(self):
        if str(self.dl_btn["state"]) != "disabled":   # không có gì đang tải
            return
        self.stop_event.set()
        self.status.set("Đang dừng... (lần sau bấm tải sẽ tải tiếp từ chỗ dở)")

    def _set_cell(self, i, text):
        self.after(0, self.tree.set, str(i), "tai", text)

    def _download_one(self, i, folder):
        if self.stop_event.is_set():
            self._set_cell(i, "đã dừng")
            return "dung"
        ep = self.episodes[i]
        base = episode_base(folder, self.movie_slug, ep)
        if downloader.existing_file(base):
            self._set_cell(i, "đã có ✓")
            return "ok"
        try:
            self._set_cell(i, "lấy link...")
            # Lấy link mới ngay trước khi tải, phòng link cũ đã hết hạn
            vid = get_video_url(ep)
            ep["video"] = vid
            self.after(0, self.tree.set, str(i), "video", video_text(vid))
            last = [0.0]

            def prog(pct, detail):
                now = time.monotonic()
                if now - last[0] >= 0.5 or pct == 100:
                    last[0] = now
                    self._set_cell(i, detail if pct is None else f"{pct}% · {detail}")

            finished = downloader.download(vid, base, prog, self.stop_event)
            if finished:
                ext = os.path.splitext(finished)[1]
                self._set_cell(i, f"xong ✓ ({ext})")
            else:
                self._set_cell(i, "đã dừng")
            return "ok" if finished else "dung"
        except Exception as e:
            self._set_cell(i, "lỗi")
            self.after(0, self.tree.set, str(i), "video", f"Lỗi tải: {e}")
            return "loi"

    def _download_worker(self, idx, folder):
        with ThreadPoolExecutor(max_workers=2) as pool:
            results = list(pool.map(lambda i: self._download_one(i, folder), idx))
        self.after(0, self._download_done, results, folder)

    def _download_done(self, results, folder):
        self.dl_btn.config(state="normal")
        self.stop_btn.config(state="disabled")
        ok, loi = results.count("ok"), results.count("loi")
        msg = f"Tải xong {ok}/{len(results)} tập vào {folder}."
        if loi:
            msg += f" {loi} tập lỗi."
        if "dung" in results:
            msg = f"Đã dừng. Xong {ok}/{len(results)} tập."
        self.status.set(msg)

    def open_grid(self):
        """Mở các tập, mỗi tập một cửa sổ, xếp đều thành lưới.
        Nếu đang chọn từ 2 dòng trở lên thì chỉ mở các dòng đó."""
        selected = self._selected_urls()
        urls = selected if len(selected) >= 2 else [e["url"] for e in self.episodes]
        if not urls:
            self.status.set("Chưa có link để mở.")
            return
        self.grid_btn.config(state="disabled")
        threading.Thread(target=self._grid_worker, args=(urls,),
                         daemon=True).start()

    def _grid_worker(self, urls):
        def progress(i, n):
            self.after(0, self.status.set, f"Đang mở tập {i}/{n}...")
        try:
            hwnds, err = grid_open.open_in_grid(urls, on_progress=progress), None
        except Exception as e:
            hwnds, err = [], e
        self.after(0, self._grid_done, urls, hwnds, err)

    def _grid_done(self, urls, hwnds, err):
        self.grid_btn.config(state="normal")
        if err:
            self.status.set("Lỗi.")
            messagebox.showerror("Không mở được", str(err))
            return
        ok = sum(1 for h in hwnds if h)
        msg = f"Đã mở và xếp {ok}/{len(urls)} cửa sổ."
        if ok < len(urls):
            msg += " Có cửa sổ không xuất hiện kịp."
        self.status.set(msg)

    def add_to_queue(self):
        """Đưa link phim đang nhập sang tab hàng đợi."""
        url = self.url_var.get().strip()
        if not url:
            self.status.set("Chưa có URL phim.")
            return
        bad = self.queue.add_urls([url])
        if bad:
            messagebox.showwarning("Chưa hỗ trợ", "Trang này chưa được hỗ trợ.")
            return
        self.tabs.select(self.queue)

    def on_close(self):
        busy = self.queue.running or str(self.dl_btn["state"]) == "disabled"
        if busy and not messagebox.askyesno(
                "Đang tải", "Đang tải video. Thoát bây giờ sẽ dừng tải "
                "(lần sau mở lại sẽ tải tiếp từ chỗ dở).\n\nVẫn thoát?"):
            return
        self.queue.save()
        self.destroy()

    def save(self):
        if not self.episodes:
            self.status.set("Chưa có link để lưu.")
            return
        path = filedialog.asksaveasfilename(
            defaultextension=".txt", initialfile="links.txt",
            filetypes=[("Text", "*.txt"), ("Tất cả", "*.*")])
        if not path:
            return
        with open(path, "w", encoding="utf-8") as f:
            f.write("\n".join(ep["url"] for ep in self.episodes) + "\n")
        self.status.set(f"Đã lưu {len(self.episodes)} link.")


if __name__ == "__main__":
    App().mainloop()
