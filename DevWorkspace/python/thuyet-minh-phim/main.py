"""Thuyết minh phim: bắt phụ đề trên màn hình -> OCR -> đọc bằng giọng tiếng Việt.

    ThuyetMinhPhim.exe              mở giao diện
    ThuyetMinhPhim.exe --selftest   kiểm tra OCR / giọng đọc / ffmpeg, ghi kết quả ra selftest.txt
    ThuyetMinhPhim.exe --selftest <phim.mp4>   như trên + quét và thuyết minh thử cả phim đó
"""
import ctypes
import sys
import traceback


def _dpi_aware():
    # Để toạ độ chuột/tkinter trùng với pixel thật khi Windows đang scale 125%/150%
    try:
        ctypes.windll.shcore.SetProcessDpiAwareness(2)
    except Exception:  # noqa: BLE001
        try:
            ctypes.windll.user32.SetProcessDPIAware()
        except Exception:  # noqa: BLE001
            pass


def _selftest() -> int:
    import asyncio
    import subprocess

    import numpy as np

    from longtieng import __version__
    from longtieng.paths import BUNDLED_TESSERACT, DATA_DIR, PORTABLE_DIR

    lines, ok = [f"Thuyết minh phim {__version__}"], True

    def check(name, fn):
        nonlocal ok
        try:
            lines.append(f"[OK]  {name}: {fn()}")
        except Exception as e:  # noqa: BLE001
            ok = False
            lines.append(f"[LỖI] {name}: {e}")

    def ocr():
        from longtieng.ocr import TesseractOcr

        t = TesseractOcr("vie", None, "best")
        t.recognize(np.full((40, 120), 255, np.uint8))
        return f"{t.exe} (tessdata {t.model})"

    def voice():
        from longtieng.tts import WindowsVoice

        pcm = asyncio.run(WindowsVoice().synth("Xin chào", 0, 0, -20))
        return f"Microsoft An, {len(pcm)} mẫu"

    def ffmpeg():
        from longtieng.video import ffmpeg_exe

        exe = ffmpeg_exe()
        out = subprocess.run([exe, "-version"], capture_output=True, text=True,
                             creationflags=getattr(subprocess, "CREATE_NO_WINDOW", 0)).stdout
        return out.splitlines()[0] if out else exe

    def screen():
        from longtieng.screen import list_monitors

        return f"{len(list_monitors())} màn hình"

    def libs():
        import edge_tts  # noqa: F401
        import keyboard  # noqa: F401
        import miniaudio  # noqa: F401

        return "edge-tts, keyboard, miniaudio"

    def vietocr():
        from longtieng.vietocr_ocr import VietOcr, is_available

        why = is_available()
        if why:
            raise RuntimeError(why)
        mask = np.zeros((40, 160), np.uint8)
        mask[10:30, 10:150] = 255
        VietOcr().recognize_mask(mask)
        return "vgg_transformer"

    def vieneu():
        from longtieng import vieneu_tts

        if not vieneu_tts.installed():
            raise RuntimeError("thiếu thư viện vieneu/onnxruntime")
        need = vieneu_tts.missing_bytes()
        if need:
            return f"{len(vieneu_tts.voices())} giọng; chưa tải mô hình ({need / 1e6:.0f} MB, app tải khi chọn giọng VieNeu)"
        pcm = vieneu_tts.VieNeuVoice().synth("Xin chào", "vieneu:Thiện Minh")
        return f"{len(vieneu_tts.voices())} giọng, {len(pcm)} mẫu ({vieneu_tts.model_dir()})"

    def google():
        from longtieng import google_tts

        import gtts  # noqa: F401  (thiếu thư viện -> lỗi thật)
        try:
            pcm = google_tts.adjust(google_tts.synth("Xin chào", timeout=5), 1.2, -10)
        except Exception as e:  # noqa: BLE001  mất mạng không phải lỗi của bản cài
            return f"có thư viện gTTS; chưa gọi được Google ({type(e).__name__}) — cần Internet"
        return f"chị Google, {len(pcm)} mẫu"

    def full_run(video):
        # Chạy đủ quy trình chế độ File trên một phim: quét -> .srt -> thuyết minh -> video mới
        from longtieng import config
        from longtieng.dubber import dub_video
        from longtieng.gui_file import srt_path
        from longtieng.srt import write_srt
        from longtieng.subscan import scan_video

        cfg = config.load()
        cues = scan_video(video, cfg)
        write_srt(srt_path(video), cues)
        out = dub_video(video, cues, cfg)
        return f"{len(cues)} câu -> {out}"

    lines.append(f"Tesseract đi kèm: {BUNDLED_TESSERACT}")
    lines.append(f"Dữ liệu: {DATA_DIR}" + (" (bản portable)" if PORTABLE_DIR else ""))
    tests = [("OCR Tesseract", ocr), ("OCR VietOCR", vietocr), ("Giọng Windows", voice), ("Giọng VieNeu", vieneu),
             ("Giọng Google", google), ("ffmpeg", ffmpeg),
             ("Màn hình", screen), ("Thư viện", libs)]
    args = [a for a in sys.argv[1:] if a != "--selftest"]
    if args:  # ThuyetMinhPhim.exe --selftest <phim>: thử cả quét + thuyết minh
        tests.append(("Quét + thuyết minh", lambda: full_run(args[0])))
    for name, fn in tests:
        check(name, fn)
    (DATA_DIR / "selftest.txt").write_text("\n".join(lines) + "\n", encoding="utf-8")
    return 0 if ok else 1


def _crash(exc: BaseException) -> None:
    """Bản đóng gói không có cửa sổ console: ghi lỗi ra file và báo bằng hộp thoại."""
    try:
        from longtieng.paths import DATA_DIR

        log = DATA_DIR / "error.log"
        log.write_text("".join(traceback.format_exception(exc)), encoding="utf-8")
        where = f"\n\nChi tiết: {log}"
    except Exception:  # noqa: BLE001
        where = ""
    ctypes.windll.user32.MessageBoxW(None, f"Thuyết minh phim gặp lỗi khi chạy:\n{exc}{where}",
                                     "Thuyết minh phim", 0x10)


if __name__ == "__main__":
    if "--selftest" in sys.argv:
        sys.exit(_selftest())
    if sys.platform == "win32":
        _dpi_aware()
    try:
        from longtieng.gui import App

        App().run()
    except Exception as e:  # noqa: BLE001
        if getattr(sys, "frozen", False):
            _crash(e)
        else:
            raise
