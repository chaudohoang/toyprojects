"""Hàm dùng chung cho các trang phim."""
import urllib.parse
import urllib.request

UA = ("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
      "(KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36")


def fetch(url, referer=None, timeout=30):
    headers = {"User-Agent": UA}
    if referer:
        headers["Referer"] = referer
    req = urllib.request.Request(url, headers=headers)
    with urllib.request.urlopen(req, timeout=timeout) as r:
        charset = r.headers.get_content_charset() or "utf-8"
        return r.read().decode(charset, errors="replace")


def host(url):
    return (urllib.parse.urlsplit(url).hostname or "").lower()


def origin(url):
    p = urllib.parse.urlsplit(url)
    return f"{p.scheme}://{p.netloc}"


def episode(tap, so, url, **extra):
    """Một tập phim: nhãn hiển thị, số để sắp xếp, link trang xem (+ dữ liệu riêng)."""
    return {"tap": tap, "so": so, "url": url, **extra}


def video(url, kind=None, **extra):
    """Kết quả link video. kind: 'mp4' | 'm3u8' | 'embed'."""
    if kind is None:
        kind = "m3u8" if ".m3u8" in url.split("?")[0] else "mp4"
    return {"sd": url, "hd": "", "du_phong": "", "loai": kind, **extra}
