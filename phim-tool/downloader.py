"""Tải video về máy: file .mp4 trực tiếp, hoặc HLS (.m3u8) ghép từ nhiều đoạn .ts.

on_progress(percent, detail) — percent: 0..100 (hoặc None nếu chưa biết).
Hàm tải trả về đường dẫn file khi xong, None nếu bị dừng bằng stop_event.
Bị dừng giữa chừng thì lần sau gọi lại sẽ tải tiếp từ chỗ dở.
"""
import os
import re
import shutil
import subprocess
import threading
import time
import urllib.request
from concurrent.futures import ThreadPoolExecutor
from urllib.parse import urljoin

from sites.common import UA, fetch

VIDEO_EXTS = (".mp4", ".ts")


def existing_file(base):
    """File đã tải xong (base + .mp4 hoặc .ts) nếu có."""
    for ext in VIDEO_EXTS:
        if os.path.exists(base + ext):
            return base + ext
    return None


class SourceBroken(RuntimeError):
    """Server từ chối hoặc không có file (401/403/404/410): link này hỏng hẳn,
    thử lại không ích gì → chuyển sang nguồn dự phòng."""


PERMANENT_HTTP = (401, 403, 404, 410)
HTTP_REASON = {401: "không cho truy cập", 403: "chặn truy cập",
               404: "không có", 410: "đã xoá"}


def _is_permanent(err):
    return getattr(err, "code", None) in PERMANENT_HTTP


def _pause(stop_event, seconds):
    """Nghỉ `seconds` giây; trả về True nếu bị bấm Dừng trong lúc nghỉ."""
    if stop_event is not None:
        return stop_event.wait(seconds)
    time.sleep(seconds)
    return False


def download(vid, base, on_progress=None, stop_event=None, skip_ads=True, rounds=3):
    """Tải video `vid` (kết quả get_video_url) về `base` + đuôi phù hợp.

    Lỗi thì tự thử lại: lần lượt link chính rồi link dự phòng (server khác),
    tối đa `rounds` vòng, nghỉ lâu dần giữa các vòng. Mỗi lần thử tải tiếp từ
    phần đã có, không tải lại từ đầu."""
    urls = list(dict.fromkeys(u for u in (vid.get("sd"), vid.get("hd"),
                                          vid.get("du_phong"), *(vid.get("them") or []))
                              if u))
    if vid.get("loai") == "embed":
        raise RuntimeError("Tập này phát qua trang nhúng bên ngoài, không tải trực tiếp được.")
    if not urls:
        raise RuntimeError("Không có link video.")
    done = existing_file(base)
    if done:
        return done
    last_err = None
    broken = set()                  # link mà server báo không có file (404) → bỏ qua luôn
    for rnd in range(rounds):
        for n, url in enumerate(urls):
            if url in broken:
                continue
            if stop_event is not None and stop_event.is_set():
                return None
            if n and on_progress:
                on_progress(None, "thử nguồn dự phòng")
            try:
                if vid.get("loai") == "m3u8" or ".m3u8" in url:
                    return download_hls(url, base, on_progress, stop_event, skip_ads)
                return download_mp4(url, base + ".mp4", on_progress, stop_event)
            except SourceBroken as e:
                broken.add(url)
                last_err = e
            except Exception as e:
                last_err = e
        if len(broken) == len(urls):          # mọi nguồn đều hỏng hẳn, thử lại vô ích
            raise last_err
        if rnd < rounds - 1:
            if on_progress:
                on_progress(None, f"lỗi, thử lại {rnd + 2}/{rounds}")
            if _pause(stop_event, 10 * (rnd + 1)):
                return None
    raise RuntimeError(f"{last_err} (đã thử {rounds} lần)") from last_err


# ---------------------------------------------------------------- MP4

def download_mp4(url, path, on_progress=None, stop_event=None, chunk=1 << 20):
    # Server video của phimvn2 (cloudcdnvn) chỉ trả dữ liệu ổn định qua HTTP/2,
    # giống trình duyệt; qua HTTP/1.1 nó treo rồi ngắt kết nối với file > ~1KB.
    try:
        import httpx
        import h2  # noqa: F401  (cần cho http2=True)
    except ImportError:
        raise RuntimeError('Cần cài thư viện: python -m pip install "httpx[http2]"')

    part = path + ".part"
    have = os.path.getsize(part) if os.path.exists(part) else 0
    headers = {"User-Agent": UA}
    if have:
        headers["Range"] = f"bytes={have}-"
    with httpx.Client(http2=True, timeout=60, follow_redirects=True) as client:
        with client.stream("GET", url, headers=headers) as r:
            if r.status_code == 416:       # đã tải đủ từ trước
                os.replace(part, path)
                return path
            if r.status_code in PERMANENT_HTTP:
                raise SourceBroken(f"Server {HTTP_REASON[r.status_code]} file video này "
                                   f"(HTTP {r.status_code}).")
            r.raise_for_status()
            if have and r.status_code != 206:  # server không cho tải tiếp
                have = 0
            length = int(r.headers.get("Content-Length") or 0)
            total = have + length if length else 0
            done = have
            with open(part, "ab" if have else "wb") as f:
                for buf in r.iter_bytes(chunk):
                    if stop_event is not None and stop_event.is_set():
                        return None
                    f.write(buf)
                    done += len(buf)
                    if on_progress:
                        if total:
                            on_progress(done * 100 // total,
                                        f"{done >> 20}/{total >> 20} MB")
                        else:
                            on_progress(None, f"{done >> 20} MB")
    if total and done < total:
        raise RuntimeError(f"Tải thiếu ({done}/{total} byte), hãy tải lại để tải tiếp.")
    os.replace(part, path)
    return path


# ---------------------------------------------------------------- HLS

def _media_playlist(url):
    """Trả về (url, nội dung) của playlist chứa các đoạn; nếu là master
    playlist thì chọn chất lượng cao nhất."""
    text = fetch(url)
    if "#EXT-X-STREAM-INF" in text:
        best, best_bw = None, -1
        lines = text.splitlines()
        for i, line in enumerate(lines):
            if line.startswith("#EXT-X-STREAM-INF"):
                bw = re.search(r"BANDWIDTH=(\d+)", line)
                bw = int(bw.group(1)) if bw else 0
                nxt = next((l.strip() for l in lines[i + 1:] if l.strip()
                            and not l.startswith("#")), None)
                if nxt and bw > best_bw:
                    best, best_bw = nxt, bw
        if not best:
            raise RuntimeError("Playlist không có luồng video nào.")
        return _media_playlist(urljoin(url, best))
    return url, text


def _segments(url, text):
    segs, dur = [], 0.0
    for line in text.splitlines():
        line = line.strip()
        if line.startswith("#EXT-X-KEY") and "METHOD=NONE" not in line:
            raise RuntimeError("Video HLS có mã hoá (AES), tool chưa hỗ trợ.")
        if line.startswith("#EXTINF:"):
            dur = float(line[8:].split(",")[0] or 0)
        elif line and not line.startswith("#"):
            segs.append({"uri": line, "url": urljoin(url, line), "dur": dur})
    return segs


def remove_ads(segs):
    """Nguồn kkphim chèn quảng cáo dạng các đoạn ở thư mục khác (vd 'convertv9/...',
    '/v9/<mã>/segment_*.ts'). Giữ lại các đoạn thuộc thư mục chiếm nhiều thời
    lượng nhất; nếu thế sẽ bỏ quá 30% thì coi như không có quảng cáo."""
    def folder(s):
        return s["uri"].rsplit("/", 1)[0] if "/" in s["uri"] else ""
    total = sum(s["dur"] for s in segs) or 1
    by_folder = {}
    for s in segs:
        by_folder[folder(s)] = by_folder.get(folder(s), 0) + s["dur"]
    main = max(by_folder, key=by_folder.get)
    kept = [s for s in segs if folder(s) == main]
    if sum(s["dur"] for s in kept) / total < 0.7:
        return segs
    return kept


def _get_bytes(url, timeout=60):
    req = urllib.request.Request(url, headers={"User-Agent": UA})
    with urllib.request.urlopen(req, timeout=timeout) as r:
        return r.read()


TOOL_DIR = os.path.dirname(os.path.abspath(__file__))


def find_ffmpeg():
    """Ưu tiên ffmpeg trong thư mục ffmpeg/ cạnh tool, sau đó tới PATH."""
    local = os.path.join(TOOL_DIR, "ffmpeg", "ffmpeg.exe")
    if os.path.isfile(local):
        return local
    return shutil.which("ffmpeg")


def download_hls(url, base, on_progress=None, stop_event=None, skip_ads=True,
                 workers=8):
    try:
        pl_url, text = _media_playlist(url)
    except Exception as e:
        if _is_permanent(e):
            raise SourceBroken(f"Server {HTTP_REASON[e.code]} playlist video này "
                               f"(HTTP {e.code}).") from e
        raise
    segs = _segments(pl_url, text)
    if not segs:
        raise RuntimeError("Playlist không có đoạn video nào.")
    if skip_ads:
        segs = remove_ads(segs)

    parts = base + ".parts"
    # Ghi lại nguồn của các đoạn đã tải; đổi sang nguồn khác (vd bản Vietsub thay
    # cho Thuyết minh) thì xoá đoạn cũ, không ghép lẫn hai bản với nhau.
    src_file = os.path.join(parts, "nguon.txt")
    if os.path.isdir(parts) and os.path.exists(src_file):
        with open(src_file, encoding="utf-8") as f:
            if f.read().strip() != pl_url:
                shutil.rmtree(parts, ignore_errors=True)
    os.makedirs(parts, exist_ok=True)
    with open(src_file, "w", encoding="utf-8") as f:
        f.write(pl_url)
    n = len(segs)
    lock = threading.Lock()
    state = {"done": 0, "bytes": 0, "failed": 0, "missing": 0, "code": 404, "err": None}
    # Server thiếu quá nhiều đoạn (404) → nguồn hỏng, dừng sớm thay vì thử hết
    max_missing = max(5, n // 50)
    abort = threading.Event()

    def seg_path(i):
        return os.path.join(parts, f"{i:05d}.ts")

    for i in range(n):
        if os.path.exists(seg_path(i)):
            state["done"] += 1
            state["bytes"] += os.path.getsize(seg_path(i))

    def report():
        if on_progress:
            on_progress(state["done"] * 100 // n,
                        f"{state['done']}/{n} đoạn · {state['bytes'] >> 20} MB")

    def one(i):
        p = seg_path(i)
        if os.path.exists(p):
            return True
        tries = 5
        for attempt in range(tries):
            if stop_event is not None and stop_event.is_set():
                return False
            if abort.is_set():
                return None
            try:
                data = _get_bytes(segs[i]["url"])
                break
            except Exception as e:
                permanent = _is_permanent(e)
                if permanent or attempt == tries - 1:   # đoạn hỏng; đoạn khác vẫn tải tiếp
                    with lock:
                        state["failed"] += 1
                        state["err"] = e
                        if permanent:
                            state["missing"] += 1
                            state["code"] = e.code
                            if state["missing"] > max_missing:
                                abort.set()
                    return None
                if _pause(stop_event, min(2 ** attempt, 8)):
                    return False
        with open(p + ".tmp", "wb") as f:
            f.write(data)
        os.replace(p + ".tmp", p)
        with lock:
            state["done"] += 1
            state["bytes"] += len(data)
            report()
        return True

    report()
    with ThreadPoolExecutor(max_workers=workers) as pool:
        results = list(pool.map(one, range(n)))
    if False in results or (stop_event is not None and stop_event.is_set()):
        return None
    if abort.is_set():
        raise SourceBroken(f"Nguồn video hỏng trên server: thiếu ít nhất "
                           f"{state['missing']}/{n} đoạn (HTTP {state['code']}).")
    if state["missing"] and state["missing"] == state["failed"]:
        raise SourceBroken(f"Server thiếu {state['missing']}/{n} đoạn video "
                           f"(HTTP {state['code']}).")
    if state["failed"]:
        raise RuntimeError(f"{state['failed']}/{n} đoạn video tải hỏng ({state['err']})")

    # Ghép các đoạn .ts lại (MPEG-TS ghép nối trực tiếp được)
    if on_progress:
        on_progress(100, "đang ghép file...")
    ts_path = base + ".ts"
    with open(ts_path + ".part", "wb") as out:
        for i in range(n):
            with open(seg_path(i), "rb") as f:
                shutil.copyfileobj(f, out)
    os.replace(ts_path + ".part", ts_path)
    shutil.rmtree(parts, ignore_errors=True)

    # Có ffmpeg thì đổi sang .mp4 (không nén lại, chỉ đổi định dạng chứa)
    ffmpeg = find_ffmpeg()
    if ffmpeg:
        if on_progress:
            on_progress(100, "đang chuyển sang .mp4...")
        mp4 = base + ".mp4"
        r = subprocess.run([ffmpeg, "-y", "-loglevel", "error", "-i", ts_path,
                            "-c", "copy", "-bsf:a", "aac_adtstoasc", mp4],
                           capture_output=True,
                           creationflags=getattr(subprocess, "CREATE_NO_WINDOW", 0))
        if r.returncode == 0 and os.path.exists(mp4):
            os.remove(ts_path)
            return mp4
        if os.path.exists(mp4):
            os.remove(mp4)
    return ts_path
