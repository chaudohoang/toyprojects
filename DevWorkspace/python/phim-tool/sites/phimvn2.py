"""phimvn2.pro — player vn2data, file .mp4 trên cloudcdnvn (cần HTTP/2 để tải)."""
import re
from html.parser import HTMLParser
from urllib.parse import urljoin

from .common import episode, fetch, host, video

NAME = "phimvn2"
# Link phim có mã số ở cuối (…/phim/kiem-sac-hoa-hong-4368) → bỏ khi đặt tên thư mục/file
SLUG_HAS_ID = True

# Link tập có dạng: /phim/<slug>/tap-<số>[-...]-<id>
EP_RE = re.compile(r"/phim/[^/]+/tap-([0-9]+)[^/]*$", re.IGNORECASE)


def matches(url):
    return "phimvn2" in host(url)


class _EpisodeParser(HTMLParser):
    def __init__(self):
        super().__init__()
        self.episodes = []          # [(href, text)]
        self._href = None
        self._text = []

    def handle_starttag(self, tag, attrs):
        if tag == "a":
            href = dict(attrs).get("href") or ""
            if EP_RE.search(href.split("?")[0]):
                self._href, self._text = href, []

    def handle_data(self, data):
        if self._href is not None:
            self._text.append(data)

    def handle_endtag(self, tag):
        if tag == "a" and self._href is not None:
            self.episodes.append((self._href, "".join(self._text).strip()))
            self._href = None


def get_episodes(page_url):
    parser = _EpisodeParser()
    parser.feed(fetch(page_url))

    by_url = {}
    for href, text in parser.episodes:
        full = urljoin(page_url, href)
        num = int(EP_RE.search(href.split("?")[0]).group(1))
        # Chỉ nhận nhãn bắt đầu bằng số (vd "1", "8-End"), bỏ nút "XEM PHIM"
        label = text if re.match(r"^\d", text or "") else None
        ep = by_url.setdefault(full, episode(None, num, full))
        if label and not ep["tap"]:
            ep["tap"] = label

    result = sorted(by_url.values(), key=lambda e: e["so"])
    for ep in result:
        ep["tap"] = ep["tap"] or str(ep["so"])
    return result


_keyfix_cache = {}


def _channel_keyfix(domain, referer):
    if domain not in _keyfix_cache:
        js = fetch(f"https://vn2data.com/check-user.php?url={domain}", referer)
        m = re.search(r'channel_keyfix\s*=\s*"([^"]*)"', js)
        _keyfix_cache[domain] = m.group(1) if m else ""
    return _keyfix_cache[domain]


def get_video_url(ep):
    """Mô phỏng cách trang tạo iframe player (content-new.js) cho một trình
    duyệt Windows chưa có cookie, rồi đọc link trong trang player."""
    ep_url = ep["url"]
    html = fetch(ep_url)
    v = dict(re.findall(r'var (\w+)\s*=\s*["\']([^"\']*)["\']', html))
    domain = v.get("nowDomain") or host(ep_url)

    key_prefix = re.search(r"var channel_key = '(\d+)' \+ channel_keyfix", html)
    mid = re.search(r"var totalview = channel_key \+ '([^']*)'", html)
    if not (key_prefix and mid and "domain_api" in v):
        raise RuntimeError("Trang tập không có thông tin player như mong đợi.")

    channel_key = (key_prefix.group(1) + _channel_keyfix(domain, ep_url)
                   + "catcookie" + "notyescookienumw")
    totalview = (channel_key + mid.group(1) + "1920clientw1903"
                 + "usersvcatasiavn")

    start = ep_url.rfind("tap-")
    tail = ep_url[start:]
    sv = ("-vietsub-" if "-vietsub-" in tail
          else "-tm2-" if "-tm2-" in tail else "-thuyet-minh-")
    tap = ep_url[start + 4: ep_url.find(sv, start)]

    fixfull = v.get("name_fixfull", "")
    num_sv = "111"
    if "-vietsub-" in ep_url and "fix-to111" not in fixfull:
        num_sv = "333"
    elif "-tm2-" in ep_url:
        num_sv = "222"
    if "fix-to333" in fixfull:
        num_sv = "333"

    full = v.get("channel_full", "")
    cp = full.find("links")
    channel_fix = full[cp + 5:]          # giống JS: indexOf = -1 -> substring(4)
    iframe = ""
    if "ok.ru" in channel_fix:
        iframe, channel_fix = channel_fix, ""
    fix4 = "nofb1" if "nofb1" in channel_fix else ""

    idplay = f"{tap}sv{num_sv}id{int(v['name_idphim'])}numview{totalview}chankey1"
    player = (v["domain_api"] + "/js_fix/9x/play.php?link=" + idplay
              + "vn2fix1" + channel_fix.strip()
              + "vn2fix2" + v.get("channel2_fix", "").strip()
              + "vn2fix3" + iframe
              + "vn2fixload" + fixfull.strip()
              + "vn2fixurl4" + fix4 + "vn2fixurl5" + "6O548l721190"
              + v.get("name_fix", "").strip() + v.get("channel3_fix", "").strip()
              + "vn2myname" + v.get("name_fixload", "")
              + "folderfix" + v.get("name_numget", "")
              + "catnumget" + v.get("name_fixfull2", "") + ep_url)
    player = player.replace("&", "vn2_fix")

    body = fetch(player, f"https://{domain}/")
    # Trang gán các biến này nhiều lần (có lần gán rỗng) → lấy giá trị đầu tiên khác rỗng
    links = {}
    for name, val in re.findall(
            r'var (link_video_sd|link_video_hd|link_video_load)\s*=\s*"([^"]*)"', body):
        if val and name not in links:
            links[name] = val
    sd = links.get("link_video_sd", "")
    # Tập có link trực tiếp sẵn trong trang (không qua player vn2data)
    if not sd and channel_fix.startswith("https:"):
        sd = channel_fix.strip()
    if not sd:
        raise RuntimeError("Không tìm thấy link video trong trang player.")
    result = video(sd)
    result["hd"] = links.get("link_video_hd", "")
    result["du_phong"] = links.get("link_video_load", "")
    return result
