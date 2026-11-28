"""tvhay.buzz, tvphim.buzz (cùng một mã nguồn) — link .m3u8 (kkphim) nằm sẵn trong trang xem.

Trang chỉ hiện một bản (thường là Thuyết minh). Video thật nằm ở kkphim, nên tool
tra thêm kkphim (xem kkphim.py) để lấy bản còn lại (Vietsub...) làm nguồn dự
phòng khi bản chính bị hỏng trên server."""
import re
import urllib.parse
from urllib.parse import urljoin

from . import kkphim
from .common import episode, fetch, host, video

NAME = "tvhay/tvphim"


def matches(url):
    h = host(url)
    return "tvhay" in h or "tvphim" in h


def _watch_page(page_url):
    """Trang /phim/<slug> chỉ liệt kê vài tập; danh sách đủ nằm ở trang /xem/."""
    if "/xem/" in page_url:
        return page_url, fetch(page_url)
    html = fetch(page_url)
    m = re.search(r'href="(/xem/[^"]+)"', html)
    if not m:
        raise RuntimeError("Không tìm thấy nút xem phim trên trang này.")
    url = urljoin(page_url, m.group(1))
    return url, fetch(url)


def get_episodes(page_url):
    watch_url, html = _watch_page(page_url)
    blocks = re.split(r'class="source-container"', html)[1:]
    result = []
    for order, blk in enumerate(blocks):
        t = re.search(r'source-title">.*?</i>\s*([^<]+)', blk, re.S)
        server = t.group(1).strip() if t else f"Server {order + 1}"
        for href, label in re.findall(
                r'href="([^"]+)"\s*class="ep-btn[^"]*">\s*([^<]+?)\s*</a>', blk):
            num = re.search(r"(\d+)", label)
            tap = label.replace("Tập", "").strip() or label
            if len(blocks) > 1:
                tap = f"{tap} ({server})"
            result.append(episode(tap, (order, int(num.group(1)) if num else 0),
                                  urljoin(watch_url, href), server=server))
    result.sort(key=lambda e: e["so"])
    return result


def _main_link(html):
    for pat in (r'var\s+linkHPro\s*=\s*"[^"]*?[?&]url=([^"&]+)',
                r'id="movie-iframe"\s+src="[^"]*?[?&]url=([^"&]+)',
                r'var\s+linkSPro\s*=\s*"[^"]*?[?&]url=([^"&]+)'):
        m = re.search(pat, html)
        if m:
            return urllib.parse.unquote(m.group(1))
    m = re.search(r'https?://[^"\'\s]+\.m3u8[^"\'\s]*', html)
    return m.group(0) if m else None


def get_video_url(ep):
    """Ưu tiên Lồng tiếng → Thuyết minh → Vietsub, bản kế tiếp làm dự phòng — kể cả khi
    trang chỉ hiện bản Vietsub mà kkphim có Thuyết minh."""
    main = _main_link(fetch(ep["url"]))
    if not main:
        raise RuntimeError("Không tìm thấy link video trong trang xem.")
    m = re.search(r"/xem/([^/?#]+)/tap-0*(\d+)", ep["url"])
    slug, num = (m.group(1), int(m.group(2))) if m else (None, None)
    return kkphim.best_video(slug, num, [(ep.get("server", ""), main)])
