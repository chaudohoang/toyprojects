"""Danh sách các trang phim được hỗ trợ. Thêm trang mới: tạo module có
NAME, matches(url), get_episodes(page_url), get_video_url(ep) rồi thêm vào SITES."""
from . import motchill, phimvn2, tvhay

SITES = [phimvn2, tvhay, motchill]


def find_site(url):
    for site in SITES:
        if site.matches(url):
            return site
    names = ", ".join(s.NAME for s in SITES)
    raise RuntimeError(f"Chưa hỗ trợ trang này. Các trang hỗ trợ: {names}.")
