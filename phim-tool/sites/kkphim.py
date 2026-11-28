"""Tra nguồn dự phòng trên kkphim (phimapi.com).

tvhay, tvphim, motchill đều phát video từ kkphim nhưng mỗi trang chỉ hiện một bản
(Thuyết minh hoặc Vietsub). API kkphim liệt kê đủ các bản của phim, nên khi bản
chính bị hỏng trên server, tool lấy bản còn lại của cùng tập để tải thay."""
import json
import re
import threading
import unicodedata

from .common import fetch

API = "https://phimapi.com/phim/{slug}"
_cache, _lock = {}, threading.Lock()


def episodes(slug):
    """[(tên bản, số tập, link m3u8)] của phim; [] nếu không tra được."""
    with _lock:
        if slug in _cache:
            return _cache[slug]
    out = []
    try:
        data = json.loads(fetch(API.format(slug=slug), timeout=15))
        for sv in data.get("episodes") or []:
            for e in sv.get("server_data") or []:
                num = re.search(r"(\d+)", e.get("name") or e.get("slug") or "")
                if num and e.get("link_m3u8"):
                    out.append((sv.get("server_name", ""), int(num.group(1)), e["link_m3u8"]))
    except Exception:
        out = []
    with _lock:
        _cache[slug] = out
    return out


def rank(label):
    """Thứ tự ưu tiên các bản: Lồng tiếng → Thuyết minh → Vietsub/Phụ đề → khác."""
    s = unicodedata.normalize("NFC", label or "").lower()
    if "lồng tiếng" in s or "long tieng" in s or "long-tieng" in s:
        return 0
    if "thuyết minh" in s or "thuyet minh" in s or "thuyet-minh" in s:
        return 1
    if "vietsub" in s or "phụ đề" in s or "phu de" in s or "sub" in s:
        return 2
    return 3


def ordered_links(slug, num, own):
    """Các bản của tập `num`, bản ưu tiên đứng đầu.

    own: [(tên bản, link)] trang phim tự có. Thêm các bản trên kkphim, bỏ trùng,
    rồi xếp Lồng tiếng → Thuyết minh → Vietsub. Trả về [(tên bản, link)]."""
    kk = [(name, link) for name, n, link in episodes(slug) if n == num] \
        if slug and num is not None else []
    kk_name = {link: name for name, link in kk}
    out, seen = [], set()
    for name, link in list(own) + kk:
        if link and link not in seen:
            seen.add(link)
            out.append((kk_name.get(link) or name, link))
    out.sort(key=lambda c: rank(c[0]))          # sort ổn định: cùng hạng giữ thứ tự cũ
    return out


def best_video(slug, num, own):
    """Kết quả get_video_url: bản ưu tiên làm link chính, bản kế tiếp làm dự phòng."""
    from .common import video
    links = ordered_links(slug, num, own)
    if not links:
        raise RuntimeError("Không tìm thấy link video.")
    return video(links[0][1], du_phong=links[1][1] if len(links) > 1 else "",
                 ban=links[0][0], du_phong_ban=links[1][0] if len(links) > 1 else "",
                 them=[link for _name, link in links[2:]])   # bản thứ 3 trở đi
