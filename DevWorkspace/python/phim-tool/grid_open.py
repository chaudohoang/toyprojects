"""Mở nhiều link, mỗi link một cửa sổ trình duyệt (Chrome/Edge, chế độ app),
rồi xếp đều các cửa sổ thành lưới trên màn hình. Chỉ chạy trên Windows.

Trang phim chặn nhúng iframe (X-Frame-Options: DENY), nên không thể gom
vào một trang duy nhất; thay vào đó dùng nhiều cửa sổ của cùng một trình duyệt.
"""
import ctypes
import ctypes.wintypes as wt
import math
import os
import subprocess
import time

user32 = ctypes.windll.user32
kernel32 = ctypes.windll.kernel32
kernel32.OpenProcess.restype = wt.HANDLE
user32.SetThreadDpiAwarenessContext.restype = ctypes.c_void_p
user32.SetThreadDpiAwarenessContext.argtypes = [ctypes.c_void_p]

SW_RESTORE = 9
WM_CLOSE = 0x0010
SPI_GETWORKAREA = 0x0030
PROCESS_QUERY_LIMITED_INFORMATION = 0x1000
DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = ctypes.c_void_p(-4)

EnumWindowsProc = ctypes.WINFUNCTYPE(wt.BOOL, wt.HWND, wt.LPARAM)


def find_browser():
    """Ưu tiên Chrome, không có thì dùng Edge."""
    pf = os.environ.get("ProgramFiles", r"C:\Program Files")
    pf86 = os.environ.get("ProgramFiles(x86)", r"C:\Program Files (x86)")
    local = os.environ.get("LOCALAPPDATA", "")
    candidates = [
        os.path.join(pf, r"Google\Chrome\Application\chrome.exe"),
        os.path.join(pf86, r"Google\Chrome\Application\chrome.exe"),
        os.path.join(local, r"Google\Chrome\Application\chrome.exe"),
        os.path.join(pf86, r"Microsoft\Edge\Application\msedge.exe"),
        os.path.join(pf, r"Microsoft\Edge\Application\msedge.exe"),
    ]
    for path in candidates:
        if os.path.isfile(path):
            return path
    return None


def _process_exe(hwnd):
    pid = wt.DWORD()
    user32.GetWindowThreadProcessId(hwnd, ctypes.byref(pid))
    h = kernel32.OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, False, pid.value)
    if not h:
        return ""
    try:
        buf = ctypes.create_unicode_buffer(1024)
        size = wt.DWORD(len(buf))
        if kernel32.QueryFullProcessImageNameW(h, 0, buf, ctypes.byref(size)):
            return os.path.basename(buf.value).lower()
        return ""
    finally:
        kernel32.CloseHandle(h)


def _browser_windows(exe_name):
    """Các cửa sổ chính, đang hiện, thuộc trình duyệt exe_name."""
    found = set()
    cls = ctypes.create_unicode_buffer(256)

    def cb(hwnd, _lp):
        if not user32.IsWindowVisible(hwnd):
            return True
        if user32.GetWindowTextLengthW(hwnd) == 0:
            return True
        user32.GetClassNameW(hwnd, cls, 256)
        if cls.value != "Chrome_WidgetWin_1":
            return True
        if _process_exe(hwnd) == exe_name:
            found.add(hwnd)
        return True

    user32.EnumWindows(EnumWindowsProc(cb), 0)
    return found


def _work_area():
    r = wt.RECT()
    user32.SystemParametersInfoW(SPI_GETWORKAREA, 0, ctypes.byref(r), 0)
    return r.left, r.top, r.right - r.left, r.bottom - r.top


def grid_cells(n, left, top, width, height):
    """Chia vùng màn hình thành n ô gần vuông nhất có thể."""
    cols = math.ceil(math.sqrt(n))
    rows = math.ceil(n / cols)
    cw, ch = width // cols, height // rows
    return [(left + (i % cols) * cw, top + (i // cols) * ch, cw, ch)
            for i in range(n)]


def _place(hwnd, cell):
    x, y, w, h = cell
    user32.ShowWindow(hwnd, SW_RESTORE)
    user32.MoveWindow(hwnd, x, y, w, h, True)


def open_in_grid(urls, browser=None, timeout=15, on_progress=None):
    """Mở mỗi URL một cửa sổ và xếp thành lưới.

    Trả về danh sách hwnd (None nếu cửa sổ của URL đó không xuất hiện).
    """
    if not urls:
        return []
    browser = browser or find_browser()
    if not browser:
        raise RuntimeError("Không tìm thấy Chrome hoặc Edge trên máy.")
    exe_name = os.path.basename(browser).lower()

    # Toạ độ theo pixel thật (không bị Windows scale) — chỉ áp dụng cho luồng này.
    old_ctx = user32.SetThreadDpiAwarenessContext(
        DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2)
    try:
        cells = grid_cells(len(urls), *_work_area())
        known = _browser_windows(exe_name)
        hwnds = []
        for i, (url, cell) in enumerate(zip(urls, cells), 1):
            if on_progress:
                on_progress(i, len(urls))
            subprocess.Popen([browser, "--new-window", f"--app={url}"])
            hwnd, deadline = None, time.time() + timeout
            while time.time() < deadline:
                new = _browser_windows(exe_name) - known
                if new:
                    hwnd = new.pop()
                    known.add(hwnd)
                    break
                time.sleep(0.2)
            if hwnd:
                _place(hwnd, cell)
            hwnds.append(hwnd)

        # Trình duyệt đôi khi tự đổi kích thước cửa sổ ngay sau khi mở → xếp lại.
        time.sleep(1.0)
        for hwnd, cell in zip(hwnds, cells):
            if hwnd and user32.IsWindow(hwnd):
                _place(hwnd, cell)
        return hwnds
    finally:
        if old_ctx:
            user32.SetThreadDpiAwarenessContext(ctypes.c_void_p(old_ctx))


def close_windows(hwnds):
    for hwnd in hwnds:
        if hwnd and user32.IsWindow(hwnd):
            user32.PostMessageW(hwnd, WM_CLOSE, 0, 0)
