"""Cửa sổ toàn màn hình để kéo chuột chọn vùng phụ đề."""
from __future__ import annotations

import tkinter as tk

from PIL import Image, ImageEnhance, ImageTk

from .engine import new_sct


def select_region(master: tk.Misc) -> list[int] | None:
    """Chụp toàn bộ màn hình (mọi màn hình), cho người dùng kéo một hình chữ nhật.

    Trả về [left, top, width, height] theo pixel thật, hoặc None nếu bấm Esc.
    """
    with new_sct() as sct:
        mon = sct.monitors[0]  # màn hình ảo bao trùm tất cả
        shot = sct.grab(mon)
    img = Image.frombytes("RGB", shot.size, shot.rgb)
    dim = ImageEnhance.Brightness(img).enhance(0.45)

    top = tk.Toplevel(master)
    top.overrideredirect(True)
    top.attributes("-topmost", True)
    top.geometry(f"{mon['width']}x{mon['height']}+{mon['left']}+{mon['top']}")
    canvas = tk.Canvas(top, width=mon["width"], height=mon["height"], highlightthickness=0, cursor="crosshair")
    canvas.pack(fill="both", expand=True)

    bg = ImageTk.PhotoImage(dim)
    canvas.create_image(0, 0, image=bg, anchor="nw")
    bright_ref = {"img": None}
    canvas.create_text(
        mon["width"] // 2, 40, fill="white", font=("Segoe UI", 18, "bold"),
        text="Kéo chuột bao quanh vùng phụ đề (chừa rộng ra 2 bên, đủ 2 dòng) — Esc để huỷ",
    )

    state = {"x0": 0, "y0": 0, "rect": None, "patch": None, "result": None}

    def on_press(e):
        state["x0"], state["y0"] = e.x, e.y

    def on_drag(e):
        x0, y0 = state["x0"], state["y0"]
        x1, y1 = e.x, e.y
        l, t, r, b = min(x0, x1), min(y0, y1), max(x0, x1), max(y0, y1)
        if state["patch"]:
            canvas.delete(state["patch"])
        if r - l > 2 and b - t > 2:
            bright_ref["img"] = ImageTk.PhotoImage(img.crop((l, t, r, b)))
            state["patch"] = canvas.create_image(l, t, image=bright_ref["img"], anchor="nw")
        if state["rect"]:
            canvas.delete(state["rect"])
        state["rect"] = canvas.create_rectangle(l, t, r, b, outline="#ff4d4d", width=2)

    def on_release(e):
        x0, y0 = state["x0"], state["y0"]
        l, t, r, b = min(x0, e.x), min(y0, e.y), max(x0, e.x), max(y0, e.y)
        if r - l >= 20 and b - t >= 10:
            state["result"] = [l + mon["left"], t + mon["top"], r - l, b - t]
            top.destroy()

    canvas.bind("<ButtonPress-1>", on_press)
    canvas.bind("<B1-Motion>", on_drag)
    canvas.bind("<ButtonRelease-1>", on_release)
    top.bind("<Escape>", lambda e: top.destroy())
    top.focus_force()
    top.grab_set()
    master.wait_window(top)
    return state["result"]
