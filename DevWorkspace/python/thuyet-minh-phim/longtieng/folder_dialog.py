"""Hộp chọn NHIỀU thư mục kiểu Windows Explorer (IFileOpenDialog + FOS_PICKFOLDERS | FOS_ALLOWMULTISELECT).

Giữ Ctrl / Shift để chọn nhiều thư mục rồi bấm "Select Folder". Gọi COM thẳng bằng ctypes (không cần pywin32).
Lỗi bất ngờ thì trả về None để nơi gọi dùng hộp chọn một thư mục của tkinter.
"""
from __future__ import annotations

import ctypes
from ctypes import POINTER, WINFUNCTYPE, byref, c_long, c_uint, c_ulong, c_void_p, c_wchar_p

HRESULT = c_long
FOS_PICKFOLDERS, FOS_FORCEFILESYSTEM, FOS_ALLOWMULTISELECT, FOS_PATHMUSTEXIST = 0x20, 0x40, 0x200, 0x800
SIGDN_FILESYSPATH = 0x80058000
ERROR_CANCELLED = 0x800704C7 - (1 << 32)  # HRESULT_FROM_WIN32(ERROR_CANCELLED) dạng số âm

# Thứ tự hàm trong vtable (IUnknown 0-2, IModalWindow::Show 3, IFileDialog 4-26, IFileOpenDialog 27-28)
_SHOW, _SET_OPTIONS, _GET_OPTIONS, _SET_FOLDER, _SET_TITLE, _SET_OK_LABEL, _GET_RESULTS = 3, 9, 10, 12, 17, 18, 27
_RELEASE = 2
_ARRAY_GET_COUNT, _ARRAY_GET_ITEM = 7, 8       # IShellItemArray
_ITEM_GET_DISPLAY_NAME = 5                    # IShellItem


class _GUID(ctypes.Structure):
    _fields_ = [("Data1", c_ulong), ("Data2", ctypes.c_ushort), ("Data3", ctypes.c_ushort),
                ("Data4", ctypes.c_ubyte * 8)]

    def __init__(self, s: str):
        super().__init__()
        ctypes.oledll.ole32.CLSIDFromString(c_wchar_p("{" + s + "}"), byref(self))


CLSID_FileOpenDialog = "DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7"
IID_IFileOpenDialog = "D57C7288-D4AD-4768-BE02-9D969532D960"
IID_IShellItem = "43826D1E-E718-42EE-BC55-A1E261C37BFE"


def _method(obj: c_void_p, index: int, *argtypes):
    vtbl = ctypes.cast(obj, POINTER(POINTER(c_void_p))).contents
    fn = WINFUNCTYPE(HRESULT, c_void_p, *argtypes)(vtbl[index])
    return lambda *args: fn(obj, *args)


def _check(hr: int, what: str) -> None:
    if hr < 0:
        raise OSError(f"{what}: HRESULT {hr & 0xFFFFFFFF:#010x}")


def pick_folders(owner: int = 0, title: str = "Chọn thư mục", initial: str | None = None) -> list[str] | None:
    """Trả về danh sách thư mục đã chọn ([] nếu bấm Huỷ), hoặc None nếu không mở được hộp này."""
    ole32 = ctypes.oledll.ole32
    try:
        ctypes.windll.ole32.CoInitializeEx(None, 2)  # STA; đã khởi tạo rồi thì thôi
        dlg = c_void_p()
        ole32.CoCreateInstance(byref(_GUID(CLSID_FileOpenDialog)), None, 1,  # CLSCTX_INPROC_SERVER
                               byref(_GUID(IID_IFileOpenDialog)), byref(dlg))
    except OSError:
        return None
    try:
        opts = c_uint()
        _check(_method(dlg, _GET_OPTIONS, POINTER(c_uint))(byref(opts)), "GetOptions")
        _check(_method(dlg, _SET_OPTIONS, c_uint)(
            opts.value | FOS_PICKFOLDERS | FOS_FORCEFILESYSTEM | FOS_ALLOWMULTISELECT | FOS_PATHMUSTEXIST),
            "SetOptions")
        _method(dlg, _SET_TITLE, c_wchar_p)(title)
        _method(dlg, _SET_OK_LABEL, c_wchar_p)("Chọn thư mục")
        if initial:
            item = c_void_p()
            if ctypes.windll.shell32.SHCreateItemFromParsingName(c_wchar_p(initial), None,
                                                                 byref(_GUID(IID_IShellItem)), byref(item)) >= 0:
                _method(dlg, _SET_FOLDER, c_void_p)(item)
                _method(item, _RELEASE)()
        hr = _method(dlg, _SHOW, c_void_p)(c_void_p(owner or None))
        if hr == ERROR_CANCELLED:
            return []
        _check(hr, "Show")
        arr = c_void_p()
        _check(_method(dlg, _GET_RESULTS, POINTER(c_void_p))(byref(arr)), "GetResults")
        try:
            count = c_uint()
            _check(_method(arr, _ARRAY_GET_COUNT, POINTER(c_uint))(byref(count)), "GetCount")
            out = []
            for i in range(count.value):
                item = c_void_p()
                _check(_method(arr, _ARRAY_GET_ITEM, c_uint, POINTER(c_void_p))(i, byref(item)), "GetItemAt")
                name = c_wchar_p()
                hr = _method(item, _ITEM_GET_DISPLAY_NAME, c_uint, POINTER(c_wchar_p))(SIGDN_FILESYSPATH, byref(name))
                if hr >= 0 and name.value:
                    out.append(name.value)
                    ctypes.windll.ole32.CoTaskMemFree(name)
                _method(item, _RELEASE)()
            return out
        finally:
            _method(arr, _RELEASE)()
    except OSError:
        return None
    finally:
        _method(dlg, _RELEASE)()
