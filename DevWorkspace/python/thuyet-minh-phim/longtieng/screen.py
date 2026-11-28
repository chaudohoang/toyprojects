"""Tìm màn hình + cửa sổ đang chiếu phim và tính vùng phụ đề tự động (phần dưới cửa sổ đó)."""
from __future__ import annotations

import ctypes
import os
from ctypes import wintypes

_MONITOR_DEFAULTTONEAREST = 2


class _MONITORINFO(ctypes.Structure):
    _fields_ = [("cbSize", wintypes.DWORD), ("rcMonitor", wintypes.RECT),
                ("rcWork", wintypes.RECT), ("dwFlags", wintypes.DWORD)]


def _user32():
    u = ctypes.windll.user32
    u.GetForegroundWindow.restype = wintypes.HWND
    u.GetWindowThreadProcessId.argtypes = [wintypes.HWND, ctypes.POINTER(wintypes.DWORD)]
    u.MonitorFromWindow.argtypes = [wintypes.HWND, wintypes.DWORD]
    u.MonitorFromWindow.restype = ctypes.c_void_p
    u.MonitorFromPoint.argtypes = [wintypes.POINT, wintypes.DWORD]
    u.MonitorFromPoint.restype = ctypes.c_void_p
    u.GetMonitorInfoW.argtypes = [ctypes.c_void_p, ctypes.POINTER(_MONITORINFO)]
    return u


def active_monitor() -> tuple[list[int], str]:
    """[left, top, width, height] (pixel thật) của màn hình đang xem phim, kèm cách đã xác định.

    Ưu tiên màn hình chứa cửa sổ đang active (trình duyệt khi bấm phím tắt lúc xem phim).
    Nếu cửa sổ active là chính phần mềm này (bấm nút trên giao diện) thì lấy màn hình có con trỏ chuột.
    """
    u = _user32()
    hwnd = u.GetForegroundWindow()
    pid = wintypes.DWORD()
    if hwnd:
        u.GetWindowThreadProcessId(hwnd, ctypes.byref(pid))
    if hwnd and pid.value != os.getpid():
        hmon, how = u.MonitorFromWindow(hwnd, _MONITOR_DEFAULTTONEAREST), "cửa sổ đang mở"
    else:
        pt = wintypes.POINT()
        u.GetCursorPos(ctypes.byref(pt))
        hmon, how = u.MonitorFromPoint(pt, _MONITOR_DEFAULTTONEAREST), "con trỏ chuột"
    info = _MONITORINFO()
    info.cbSize = ctypes.sizeof(info)
    u.GetMonitorInfoW(hmon, ctypes.byref(info))
    r = info.rcMonitor
    return [r.left, r.top, r.right - r.left, r.bottom - r.top], how


def list_monitors() -> list[list[int]]:
    """Các màn hình [left, top, width, height], đánh số 1, 2, 3... từ trái sang phải."""
    import mss

    with (getattr(mss, "MSS", None) or mss.mss)() as sct:
        mons = [[m["left"], m["top"], m["width"], m["height"]] for m in sct.monitors[1:]]
    return sorted(mons, key=lambda m: (m[0], m[1]))


def monitor_region(monitor: int = 0, fraction: float = 1 / 3, hint=None):
    """Vùng phụ đề = phần dưới `fraction` của cửa sổ đang chiếu phim (cả màn hình nếu không tìm thấy cửa sổ).

    monitor = 0: tự động (tìm cửa sổ phim trên mọi màn hình); 1, 2, 3...: cố định màn hình đó.
    hint: cửa sổ người dùng dùng ngay trước khi bấm vào app này (xem last_active_window).
    Trả về (vùng, màn hình, số thứ tự màn hình, mô tả, cửa sổ phim hoặc None).
    """
    mons = list_monitors()
    if 1 <= monitor <= len(mons):
        mon = mons[monitor - 1]
        hwnd, how = find_player(mon, hint)
        desc = f"màn hình {monitor}"
    else:
        hwnd, how = find_player(None, hint)
        if hwnd:
            mon = monitor_of(hwnd)
        else:
            mon, how = active_monitor()
        monitor = next((i + 1 for i, m in enumerate(mons) if m == mon), 0)
        desc = f"tự động, màn hình {monitor}"
    rect = window_rect(hwnd) if hwnd else None
    if rect:
        title = window_title(hwnd) or "không tên"
        return bottom_of(rect, mon, fraction), mon, monitor, f"{desc}, {how}: «{title[:40]}»", hwnd
    return bottom_of(mon, mon, fraction), mon, monitor, f"{desc}, theo {how}", None


# ---------- bám theo cửa sổ phim (để không đọc chữ của app khác) ----------
_GW_HWNDNEXT, _GW_HWNDPREV = 2, 3
_GWL_EXSTYLE, _GWL_STYLE = -20, -16
_WS_CAPTION = 0x00C00000
_WS_EX_TRANSPARENT = 0x20   # cửa sổ "xuyên chuột" (overlay của Discord, Game Bar...)
_WS_EX_TOOLWINDOW = 0x80
_DWMWA_EXTENDED_FRAME_BOUNDS, _DWMWA_CLOAKED = 9, 14
_SHELL_CLASSES = {"Shell_TrayWnd", "Shell_SecondaryTrayWnd", "Progman", "WorkerW"}


def _win_api():
    u = _user32()
    u.GetWindow.argtypes = [wintypes.HWND, wintypes.UINT]
    u.GetWindow.restype = wintypes.HWND
    u.GetTopWindow.argtypes = [wintypes.HWND]
    u.GetTopWindow.restype = wintypes.HWND
    u.IsWindowVisible.argtypes = [wintypes.HWND]
    u.IsIconic.argtypes = [wintypes.HWND]
    u.IsWindow.argtypes = [wintypes.HWND]
    u.GetWindowLongW.argtypes = [wintypes.HWND, ctypes.c_int]
    u.GetWindowLongW.restype = ctypes.c_long
    u.GetWindowRect.argtypes = [wintypes.HWND, ctypes.POINTER(wintypes.RECT)]
    u.GetClassNameW.argtypes = [wintypes.HWND, wintypes.LPWSTR, ctypes.c_int]
    u.GetWindowTextW.argtypes = [wintypes.HWND, wintypes.LPWSTR, ctypes.c_int]
    d = ctypes.windll.dwmapi
    d.DwmGetWindowAttribute.argtypes = [wintypes.HWND, wintypes.DWORD, ctypes.c_void_p, wintypes.DWORD]
    return u, d


def _cls(u, hwnd) -> str:
    buf = ctypes.create_unicode_buffer(256)
    u.GetClassNameW(hwnd, buf, 256)
    return buf.value


def window_title(hwnd) -> str:
    u, _ = _win_api()
    buf = ctypes.create_unicode_buffer(256)
    u.GetWindowTextW(hwnd, buf, 256)
    return buf.value


def window_rect(hwnd) -> list[int] | None:
    """Khung nhìn thấy được [left, top, width, height] của cửa sổ; None nếu đã đóng, ẩn, thu nhỏ."""
    u, d = _win_api()
    if not hwnd or not u.IsWindow(hwnd) or not u.IsWindowVisible(hwnd) or u.IsIconic(hwnd):
        return None
    cloaked = wintypes.DWORD()
    if d.DwmGetWindowAttribute(hwnd, _DWMWA_CLOAKED, ctypes.byref(cloaked), 4) == 0 and cloaked.value:
        return None  # cửa sổ ở desktop ảo khác / app UWP đang ẩn
    r = wintypes.RECT()
    # Khung DWM: bỏ phần viền vô hình để kéo giãn (GetWindowRect rộng hơn thật ~7 px mỗi bên)
    if d.DwmGetWindowAttribute(hwnd, _DWMWA_EXTENDED_FRAME_BOUNDS, ctypes.byref(r), ctypes.sizeof(r)) != 0:
        u.GetWindowRect(hwnd, ctypes.byref(r))
    if r.right - r.left <= 0 or r.bottom - r.top <= 0:
        return None
    return [r.left, r.top, r.right - r.left, r.bottom - r.top]


def _is_player_candidate(u, hwnd) -> bool:
    """Cửa sổ có thể là cửa sổ phim: thấy được, không phải app này, taskbar/desktop hay cửa sổ phụ."""
    pid = wintypes.DWORD()
    u.GetWindowThreadProcessId(hwnd, ctypes.byref(pid))
    if pid.value == os.getpid():
        return False
    ex = u.GetWindowLongW(hwnd, _GWL_EXSTYLE)
    if ex & (_WS_EX_TRANSPARENT | _WS_EX_TOOLWINDOW) or _cls(u, hwnd) in _SHELL_CLASSES:
        return False
    r = window_rect(hwnd)
    return bool(r and r[2] >= 320 and r[3] >= 200)


def _contains(rect, x, y) -> bool:
    return rect[0] <= x < rect[0] + rect[2] and rect[1] <= y < rect[1] + rect[3]


def _z_order(u):
    hwnd = u.GetTopWindow(None)
    while hwnd:
        yield hwnd
        hwnd = u.GetWindow(hwnd, _GW_HWNDNEXT)


def window_at(x: int, y: int) -> int | None:
    """Cửa sổ trên cùng (trừ app này) chứa điểm (x, y)."""
    u, _ = _win_api()
    for hwnd in _z_order(u):
        if _is_player_candidate(u, hwnd) and _contains(window_rect(hwnd), x, y):
            return hwnd
    return None


def monitor_of(hwnd) -> list[int]:
    u = _user32()
    info = _MONITORINFO()
    info.cbSize = ctypes.sizeof(info)
    u.GetMonitorInfoW(u.MonitorFromWindow(hwnd, _MONITOR_DEFAULTTONEAREST), ctypes.byref(info))
    r = info.rcMonitor
    return [r.left, r.top, r.right - r.left, r.bottom - r.top]


def _is_fullscreen(u, hwnd) -> bool:
    """Toàn màn hình thật (video web bấm phóng to, trình phát phim): phủ kín màn hình và không có thanh tiêu đề.
    Cửa sổ chỉ phóng to (maximize) vẫn có thanh tiêu đề, kể cả ở màn hình không có taskbar."""
    r, m = window_rect(hwnd), monitor_of(hwnd)
    covers = r and r[0] <= m[0] and r[1] <= m[1] and r[0] + r[2] >= m[0] + m[2] and r[1] + r[3] >= m[1] + m[3]
    return bool(covers) and (u.GetWindowLongW(hwnd, _GWL_STYLE) & _WS_CAPTION) != _WS_CAPTION


def find_player(mon: list[int] | None = None, hint=None) -> tuple[int | None, str]:
    """Tìm cửa sổ đang chiếu phim (trên màn hình `mon`, hoặc mọi màn hình nếu None). Thứ tự ưu tiên:
    1. cửa sổ toàn màn hình thật, trên cùng (video web bấm phóng to, trình phát phim) — kể cả khi đang
       bấm vào cửa sổ khác (chat...) hay vào app này;
    2. cửa sổ đang active, nếu không phải app này (bấm phím tắt lúc đang xem phim ở cửa sổ thường);
    3. `hint`: cửa sổ dùng ngay trước khi bấm vào app này;
    4. (chỉ khi đã cố định màn hình) cửa sổ trên cùng ở giữa màn hình đó."""
    u, _ = _win_api()

    def on_mon(hwnd):
        if mon is None:
            return True
        r = window_rect(hwnd)
        return bool(r) and _contains(mon, r[0] + r[2] // 2, r[1] + r[3] // 2)

    for hwnd in _z_order(u):
        if _is_player_candidate(u, hwnd) and on_mon(hwnd) and _is_fullscreen(u, hwnd):
            return hwnd, "phim toàn màn hình"
    fg = u.GetForegroundWindow()
    if fg and _is_player_candidate(u, fg) and on_mon(fg):
        return fg, "cửa sổ đang mở"
    if hint and _is_player_candidate(u, hint) and on_mon(hint):
        return hint, "cửa sổ vừa dùng"
    if mon is not None:
        hwnd = window_at(mon[0] + mon[2] // 2, mon[1] + mon[3] // 2)
        if hwnd:
            return hwnd, "cửa sổ giữa màn hình"
    return None, "con trỏ chuột"


def last_active_window(prev=None):
    """Gọi định kỳ: trả về cửa sổ active hiện tại nếu không phải app này, không thì giữ `prev`."""
    u, _ = _win_api()
    fg = u.GetForegroundWindow()
    return fg if fg and _is_player_candidate(u, fg) else prev


def bottom_of(rect: list[int], mon: list[int], fraction: float) -> list[int]:
    """Phần dưới `fraction` của cửa sổ, cắt theo màn hình (cửa sổ có thể thò ra ngoài màn hình)."""
    left, top = max(rect[0], mon[0]), max(rect[1], mon[1])
    right, bottom = min(rect[0] + rect[2], mon[0] + mon[2]), min(rect[1] + rect[3], mon[1] + mon[3])
    w, h = max(right - left, 1), max(bottom - top, 1)
    hh = max(int(h * fraction), min(40, h))
    return [left, bottom - hh, w, hh]


def occluders(hwnd, region: list[int]) -> list[tuple[int, int, int, int]]:
    """Các phần của `region` bị cửa sổ khác đè lên cửa sổ phim (tính từ góc vùng): app chat, taskbar,
    cửa sổ luôn nổi của chính app này... Chữ trong đó không phải phụ đề nên sẽ bị bôi đen trước khi OCR."""
    u, _ = _win_api()
    out = []
    rx, ry, rw, rh = region
    w = u.GetWindow(hwnd, _GW_HWNDPREV)
    while w:
        if not (u.GetWindowLongW(w, _GWL_EXSTYLE) & _WS_EX_TRANSPARENT):
            r = window_rect(w)
            if r:
                x0, y0 = max(r[0], rx), max(r[1], ry)
                x1, y1 = min(r[0] + r[2], rx + rw), min(r[1] + r[3], ry + rh)
                # Che gần hết vùng thì nhiều khả năng là lớp phủ trong suốt (overlay) chứ không phải cửa sổ thật
                if x1 > x0 and y1 > y0 and (x1 - x0) * (y1 - y0) < 0.9 * rw * rh:
                    out.append((x0 - rx, y0 - ry, x1 - x0, y1 - y0))
        w = u.GetWindow(w, _GW_HWNDPREV)
    return out
