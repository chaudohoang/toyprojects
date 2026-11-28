"""Khung đỏ hiện tạm thời trên màn hình để báo vùng phụ đề đang dùng (không lấy focus, click xuyên qua)."""
from __future__ import annotations

import ctypes
import tkinter as tk

_KEY = "#ff00fe"  # màu được làm trong suốt
_GWL_EXSTYLE = -20
_WS_EX_TRANSPARENT = 0x20      # click xuyên qua
_WS_EX_TOOLWINDOW = 0x80       # không hiện trên taskbar
_WS_EX_NOACTIVATE = 0x08000000  # không lấy focus của trình duyệt
_SW_SHOWNOACTIVATE = 4

_current: tk.Toplevel | None = None


def _hwnd(win: tk.Toplevel) -> int:
    return int(win.wm_frame(), 16)


def flash_region(master: tk.Misc, monitor: list[int], region: list[int], title: str, subtitle: str = "",
                 ms: int = 1500) -> None:
    global _current
    if _current is not None:
        try:
            _current.destroy()
        except tk.TclError:
            pass

    left, top, width, height = monitor
    win = tk.Toplevel(master)
    win.withdraw()
    win.overrideredirect(True)
    win.attributes("-topmost", True)
    win.configure(bg=_KEY)
    win.attributes("-transparentcolor", _KEY)
    win.geometry(f"{width}x{height}+{left}+{top}")

    c = tk.Canvas(win, width=width, height=height, bg=_KEY, highlightthickness=0)
    c.pack()
    rx, ry, rw, rh = region[0] - left, region[1] - top, region[2], region[3]
    c.create_rectangle(rx + 4, ry + 4, rx + rw - 4, ry + rh - 4, outline="#ff3b3b", width=8)
    cx, cy = width / 2, ry - 90 if ry > 200 else height / 2
    c.create_rectangle(cx - 300, cy - 70, cx + 300, cy + 70, fill="#1e1e1e", outline="#ff3b3b", width=3)
    c.create_text(cx, cy - 18, text=title, fill="white", font=("Segoe UI", 34, "bold"))
    if subtitle:
        c.create_text(cx, cy + 40, text=subtitle, fill="#dddddd", font=("Segoe UI", 13))

    win.update_idletasks()
    try:
        u = ctypes.windll.user32
        hwnd = _hwnd(win)
        style = u.GetWindowLongW(hwnd, _GWL_EXSTYLE)
        u.SetWindowLongW(hwnd, _GWL_EXSTYLE, style | _WS_EX_NOACTIVATE | _WS_EX_TOOLWINDOW | _WS_EX_TRANSPARENT)
        u.ShowWindow(hwnd, _SW_SHOWNOACTIVATE)
        win.state("normal")  # đồng bộ trạng thái của Tk; cửa sổ đã hiện mà không lấy focus
    except Exception:  # noqa: BLE001
        win.deiconify()
    _current = win
    win.after(ms, lambda: win.destroy() if win.winfo_exists() else None)
