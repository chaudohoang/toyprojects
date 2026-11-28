"""motchill (motchilll.ch, ...) — web Next.js; danh sách tập nằm trong dữ liệu JSON
của trang, link video lấy từ player api.clubc.live/embed/<id> (encrypted_url,
mã hoá XOR với khoá cố định)."""
import json
import re
from urllib.parse import urljoin

from . import kkphim
from .common import episode, fetch, host, origin, video

NAME = "motchill"

API_BASE = "https://api.clubc.live"     # hàm wN() trong chunk JS của trang
XOR_KEY = b"mySecretKey2024"            # khoá giải encrypted_url (media_player.js)


def matches(url):
    return "motchill" in host(url)


def _movie_slug(url):
    m = re.search(r"/phim/([^/?#]+)", url)
    if not m:
        raise RuntimeError("URL không đúng dạng /phim/<tên-phim>.")
    return m.group(1)


def _episodes_json(html):
    """Lấy mảng "episodes" dài nhất trong payload RSC của Next.js (chuỗi JSON
    bị escape). Trang còn có các phim liên quan với "episodes":[] rỗng."""
    best = []
    pos = 0
    while True:
        i = html.find('\\"episodes\\":[', pos)
        if i < 0:
            break
        start = html.index("[", i)
        depth = 0
        for j in range(start, len(html)):
            c = html[j]
            if c == "[":
                depth += 1
            elif c == "]":
                depth -= 1
                if depth == 0:
                    break
        pos = j
        try:
            arr = json.loads(json.loads('"' + html[start:j + 1] + '"'))
        except ValueError:
            continue
        if len(arr) > len(best):
            best = arr
    return best


def get_episodes(page_url):
    slug = _movie_slug(page_url)
    base = origin(page_url)
    html = fetch(page_url)
    eps = _episodes_json(html)
    if not eps:
        # Trang giới thiệu phim không chứa danh sách tập → mở trang xem tập đầu
        m = re.search(r'href="((?:https?://[^"/]+)?/phim/%s/[^"]+)"' % re.escape(slug), html)
        if m:
            eps = _episodes_json(fetch(urljoin(base + "/", m.group(1))))
    if not eps:
        raise RuntimeError("Không tìm thấy danh sách tập trong trang.")
    # Phim có nhiều bản (Thuyết minh, Phụ đề...) thì motchill liệt kê mỗi bản thành
    # một tập riêng → gộp lại theo số tập, bản ưu tiên (Lồng tiếng → Thuyết minh → Vietsub) đứng đầu.
    groups = {}
    for e in eps:
        name = e.get("name", "")
        num = re.search(r"(\d+)", name)
        groups.setdefault(int(num.group(1)) if num else name, []).append(e)
    result = []
    for key, vers in groups.items():
        vers.sort(key=lambda v: kkphim.rank(v.get("server", "")))
        first = vers[0]
        name = first.get("name", "")
        ban = [{"server": v.get("server", ""), "id": v.get("id"),
                "kieu": v.get("type", "m3u8"), "link": v.get("link", ""),
                "url": f"{base}/phim/{slug}/{v['slug']}"} for v in vers]
        result.append(episode(
            name.replace("Tập", "").strip() or name,
            (0, key) if isinstance(key, int) else (1, 0),
            ban[0]["url"], id=first.get("id"), server=first.get("server"), ban=ban))
    result.sort(key=lambda e: e["so"])
    return result


def _decrypt(hex_str):
    data = bytes.fromhex(hex_str)
    return bytes(c ^ XOR_KEY[i % len(XOR_KEY)] for i, c in enumerate(data)).decode()


def _embed_link(ep_id, referer):
    html = fetch(f"{API_BASE}/embed/{ep_id}", referer)
    m = re.search(r'"encrypted_url"\s*:\s*"([0-9a-fA-F]+)"', html)
    if not m:
        raise RuntimeError("Không tìm thấy link video trong player.")
    url = _decrypt(m.group(1))
    if not url.startswith("http"):
        raise RuntimeError("Giải mã link thất bại (có thể trang đã đổi khoá).")
    return url


def get_video_url(ep):
    """Ưu tiên Lồng tiếng → Thuyết minh → Vietsub; bản kế tiếp làm dự phòng. Các bản
    lấy từ motchill và từ kkphim (nơi motchill lấy video)."""
    if not ep.get("ban"):                # chỉ có URL → tra lại trong danh sách tập
        match = [e for e in get_episodes(ep["url"])
                 if any(v["url"] == ep["url"] for v in e["ban"])]
        if not match:
            raise RuntimeError("Không tìm thấy tập này trong danh sách.")
        ep = match[0]
    referer = origin(ep["url"]) + "/"
    own, last_err = [], None
    for v in ep["ban"]:
        if v["kieu"] == "embed":
            continue
        try:
            own.append((v["server"], _embed_link(v["id"], referer)))
        except Exception as e:
            last_err = e
    if not own:
        embeds = [v["link"] for v in ep["ban"]
                  if v["kieu"] == "embed" and v["link"].startswith("http")]
        if embeds:
            return video(embeds[0], "embed")
        raise last_err or RuntimeError("Không tìm thấy link video.")
    return kkphim.best_video(_movie_slug(ep["url"]), ep["so"][1], own)
