"""Lấy link tất cả các tập phim (và link/tải video) từ các trang phim.

Trang hỗ trợ: phimvn2, tvhay/tvphim, motchill (xem thư mục sites/).

Cách dùng:
    python lay_link_tap.py <url_trang_phim> [-o file.txt] [--json] [--video]
    python lay_link_tap.py <url_trang_phim> --tai <thu_muc>

Ví dụ:
    python lay_link_tap.py https://phimvn2.pro/phim/thanh-tra-vo-hinh-shadow-detective-2971
    python lay_link_tap.py https://tvhay.buzz/phim/ho-so-mat --tai D:\\Phim
"""
import argparse
import json
import os
import re
import sys

import downloader
from sites import find_site
from sites.common import UA  # noqa: F401  (giữ cho mã cũ import)


def get_episodes(page_url):
    """Danh sách tập: [{"tap", "so", "url", ...}]"""
    site = find_site(page_url)
    eps = site.get_episodes(page_url)
    for e in eps:
        e["site"] = site.NAME
    return eps


def get_video_url(ep):
    """Link video của một tập. `ep` là dict từ get_episodes() hoặc URL trang xem.
    Trả về {"sd", "hd", "du_phong", "loai": "mp4" | "m3u8" | "embed"}."""
    if isinstance(ep, str):
        ep = {"url": ep}
    return find_site(ep["url"]).get_video_url(ep)


def safe_filename(name):
    return re.sub(r'[\\/:*?"<>|]+', "_", name).strip(" .")


def movie_slug(page_url):
    """Tên phim lấy từ link. Chỉ bỏ số cuối ở trang gắn mã phim vào link
    (phimvn2: kiem-sac-hoa-hong-4368 -> kiem-sac-hoa-hong); trang khác giữ
    nguyên vì số là một phần tên (ac-nhan-2025, tai-xe-an-danh-phan-3)."""
    m = re.search(r"/(?:phim|xem)/([^/?#]+)", page_url)
    if not m:
        return "phim"
    slug = m.group(1)
    try:
        strip_id = getattr(find_site(page_url), "SLUG_HAS_ID", False)
    except Exception:
        strip_id = False
    if strip_id:
        slug = re.sub(r"-\d+$", "", slug) or slug
    return slug


def movie_folder(root, page_url):
    """Thư mục con theo tên phim (bỏ số ID cuối link), vd
    .../phim/kiem-sac-hoa-hong-4368 -> <root>/kiem-sac-hoa-hong.
    Nếu người dùng đã chọn đúng thư mục đó rồi thì không lồng thêm."""
    name = safe_filename(movie_slug(page_url))
    if os.path.basename(os.path.normpath(root)).lower() == name.lower():
        return root
    return os.path.join(root, name)


def episode_base(folder, slug, ep):
    """Đường dẫn file (chưa có đuôi) cho một tập."""
    return os.path.join(folder, safe_filename(f"{slug} - Tap {ep['tap']}"))


def download_all(page_url, folder):
    sys.stdout.reconfigure(encoding="utf-8")
    folder = movie_folder(folder, page_url)
    os.makedirs(folder, exist_ok=True)
    print(f"Lưu vào: {folder}")
    slug = movie_slug(page_url)
    eps = get_episodes(page_url)
    if not downloader.find_ffmpeg():
        print("(Không có ffmpeg: phim dạng .m3u8 sẽ được lưu thành file .ts)")
    for ep in eps:
        base = episode_base(folder, slug, ep)
        if downloader.existing_file(base):
            print(f"Tập {ep['tap']}: đã có, bỏ qua")
            continue
        try:
            vid = get_video_url(ep)
            last = [-1]

            def prog(pct, detail, tap=ep["tap"]):
                if pct is None or pct != last[0]:
                    last[0] = pct
                    print(f"\rTập {tap}: {pct if pct is not None else '?'}% "
                          f"({detail})" + " " * 10, end="", flush=True)

            path = downloader.download(vid, base, prog)
            print(f"\rTập {ep['tap']}: xong -> {path}" + " " * 30)
        except Exception as e:
            print(f"\nTập {ep['tap']}: lỗi - {e}")


def main():
    ap = argparse.ArgumentParser(description="Lấy link tất cả các tập phim")
    ap.add_argument("url", help="URL trang phim")
    ap.add_argument("-o", "--output", help="Ghi kết quả ra file")
    ap.add_argument("--json", action="store_true", help="Xuất dạng JSON")
    ap.add_argument("--only-links", action="store_true",
                    help="Chỉ in link, mỗi dòng một link")
    ap.add_argument("--video", action="store_true",
                    help="Lấy thêm link file video (.mp4/.m3u8) của từng tập")
    ap.add_argument("--tai", metavar="THU_MUC",
                    help="Tải video của tất cả các tập về thư mục này")
    args = ap.parse_args()

    if args.tai:
        return download_all(args.url, args.tai)

    try:
        eps = get_episodes(args.url)
    except Exception as e:
        sys.exit(f"Lỗi: {e}")

    if not eps:
        sys.exit("Không tìm thấy tập nào trên trang này.")

    if args.video:
        for e in eps:
            try:
                e["video"] = get_video_url(e)
            except Exception as ex:
                e["video"] = {"sd": "", "hd": "", "du_phong": "", "loi": str(ex)}
            print(f"  đã lấy video tập {e['tap']}", file=sys.stderr)

    key = (lambda e: e["video"]["sd"]) if args.video else (lambda e: e["url"])
    if args.json:
        out = json.dumps(eps, ensure_ascii=False, indent=2)
    elif args.only_links:
        out = "\n".join(key(e) for e in eps)
    else:
        out = "\n".join(f"Tập {e['tap']}: {key(e) or e['video'].get('loi')}"
                        for e in eps)

    if args.output:
        with open(args.output, "w", encoding="utf-8") as f:
            f.write(out + "\n")
        print(f"Đã lưu {len(eps)} tập vào {args.output}")
    else:
        sys.stdout.reconfigure(encoding="utf-8")
        print(out)
        print(f"\nTổng: {len(eps)} tập", file=sys.stderr)


if __name__ == "__main__":
    main()
