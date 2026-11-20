#!/usr/bin/env python3
"""
Scam Site Crawler - Security Research Tool
Zero dependencies -- pure Python 3 stdlib only.

Usage:
    python scam_crawler.py https://target-site.com
    python scam_crawler.py https://target-site.com --file page.html
    python scam_crawler.py https://target-site.com --cookies "cf_clearance=abc; session=xyz"
    python scam_crawler.py https://target-site.com --output ./case1

Bypassing Cloudflare:
  Option A (easiest) - save page source from browser:
    1. Open site in Chrome while unblocked
    2. Press Ctrl+U -> Ctrl+A -> Ctrl+S -> save as page.html
    3. python scam_crawler.py https://site.com --file page.html

  Option B - copy cookies from DevTools:
    1. Open site in Chrome, pass Cloudflare check
    2. F12 -> Network tab -> click any request -> Request Headers -> copy Cookie: value
    3. python scam_crawler.py https://site.com --cookies "cf_clearance=abc123..."
"""

import re
import sys
import json
import time
import argparse
import urllib.request
import urllib.parse
import urllib.error
import html.parser
import collections
from pathlib import Path


# ── Config ────────────────────────────────────────────────────────────────────

HEADERS = {
    "User-Agent": (
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
        "AppleWebKit/537.36 (KHTML, like Gecko) "
        "Chrome/124.0.0.0 Safari/537.36"
    ),
    "Accept-Language": "en-US,en;q=0.9",
    "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
}
TIMEOUT = 15
DELAY   = 0.3


# ── Colors ────────────────────────────────────────────────────────────────────

class C:
    RESET  = "\033[0m"
    CYAN   = "\033[96m"
    GREEN  = "\033[92m"
    YELLOW = "\033[93m"
    RED    = "\033[91m"
    MAG    = "\033[95m"

def ok(m):   print(f"{C.GREEN}  +  {m}{C.RESET}")
def info(m): print(f"{C.CYAN}  .  {m}{C.RESET}")
def warn(m): print(f"{C.YELLOW}  !  {m}{C.RESET}")
def hit(m):  print(f"{C.MAG}  *  {m}{C.RESET}")
def err(m):  print(f"{C.RED}  x  {m}{C.RESET}")
def hdr(m):  print(f"{C.CYAN}{m}{C.RESET}")


# ── Regex patterns for static analysis ───────────────────────────────────────

PATTERNS = {
    "api_endpoints": [
        r"""["'`](/api/[^\s"'`<>{}\[\]]{3,80})""",
        r"""["'`](/v\d+/[^\s"'`<>{}\[\]]{3,80})""",
        r"""axios\.(?:get|post|put|delete|patch)\(["']([^"']+)["']""",
        r"""fetch\(["']([^"']{8,})["']""",
        r"""baseURL\s*[:=]\s*["']([^"']{5,})["']""",
        r"""baseUrl\s*[:=]\s*["']([^"']{5,})["']""",
        r"""["'`](wss?://[^\s"'`<>]+)""",
    ],
    "crypto_wallets": [
        r"\b(0x[a-fA-F0-9]{40})\b",
        r"\b([13][a-km-zA-HJ-NP-Z1-9]{25,34})\b",
        r"\b(bc1[a-zA-HJ-NP-Z0-9]{6,87})\b",
        r"\b(T[A-Za-z1-9]{33})\b",
    ],
    "secrets": [
        r"""(?i)(?:api[_-]?key|secret|token|password|auth)\s*[:=]\s*["']([A-Za-z0-9_\-\.]{16,})["']""",
        r"""(?i)private[_-]?key\s*[:=]\s*["']([A-Za-z0-9_\-\.]{16,})["']""",
        r"""(?i)bearer\s+([A-Za-z0-9\-_\.]{20,})""",
    ],
    "emails": [
        r"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}",
    ],
    "external_domains": [
        r"""["'`](https?://(?![^\s"']*(?:cloudflare|gstatic|googleapis|w3\.org))[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,}/[^\s"'`<>]{3,})["'`]""",
    ],
}


# ── HTML parser ───────────────────────────────────────────────────────────────

class AssetParser(html.parser.HTMLParser):
    """Extracts JS and CSS URLs from HTML, handles preload/prefetch/stylesheet."""

    def __init__(self, base_url):
        super().__init__()
        self.base_url = base_url
        self.js  = []
        self.css = []
        self.img = []

    def _abs(self, url):
        return urllib.parse.urljoin(self.base_url, url)

    def handle_starttag(self, tag, attrs):
        a = dict(attrs)
        if tag == "script" and a.get("src"):
            self.js.append(self._abs(a["src"]))

        elif tag == "link":
            href = a.get("href", "").strip()
            rel  = a.get("rel", "")
            as_  = a.get("as", "")
            if not href:
                return
            full = self._abs(href)
            # stylesheet link
            if "stylesheet" in rel:
                self.css.append(full)
            # preload/prefetch with explicit as=
            elif as_ == "script":
                self.js.append(full)
            elif as_ == "style":
                self.css.append(full)
            # fallback: infer from extension (handles cache-busted names like foo.abc123.js)
            elif re.search(r'\.js([?#]|$)', href):
                self.js.append(full)
            elif re.search(r'\.css([?#]|$)', href):
                self.css.append(full)

        elif tag == "img" and a.get("src"):
            self.img.append(self._abs(a["src"]))


def extract_assets_fallback(html_text, base_url):
    """
    Regex fallback: find JS/CSS URLs that the HTML parser might miss
    (e.g. dynamically injected script tags or full absolute URLs in attributes).
    """
    js, css = [], []

    # full absolute JS urls in any attribute
    for m in re.finditer(r'(https?://[^\s"\'<>]+\.js(?:[?#][^\s"\'<>]*)?)', html_text):
        js.append(m.group(1))

    # full absolute CSS urls in any attribute
    for m in re.finditer(r'(https?://[^\s"\'<>]+\.css(?:[?#][^\s"\'<>]*)?)', html_text):
        css.append(m.group(1))

    return js, css


# ── Network ───────────────────────────────────────────────────────────────────

def fetch(url, extra_headers=None):
    h = dict(HEADERS)
    if extra_headers:
        h.update(extra_headers)
    req = urllib.request.Request(url, headers=h)
    try:
        with urllib.request.urlopen(req, timeout=TIMEOUT) as r:
            return r.read()
    except urllib.error.HTTPError as e:
        warn(f"HTTP {e.code} -> {url}")
    except urllib.error.URLError as e:
        err(f"URL error -> {url}  ({e.reason})")
    except Exception as e:
        err(f"Failed -> {url}  ({e})")
    return None


# ── Analysis ──────────────────────────────────────────────────────────────────

def analyze(content):
    results = collections.defaultdict(set)
    for category, patterns in PATTERNS.items():
        for pat in patterns:
            for m in re.finditer(pat, content):
                groups = [g for g in m.groups() if g]
                value  = (groups[-1] if groups else m.group(0)).strip()
                if len(value) > 4:
                    results[category].add(value)
    return {k: sorted(v) for k, v in results.items()}


def safe_name(url):
    p = urllib.parse.urlparse(url)
    name = (p.path.lstrip("/") or "index").replace("/", "__")
    if p.query:
        # append a short hash of query to keep filename unique
        name += "__" + str(abs(hash(p.query)))[:8]
    return name


# ── Crawler ───────────────────────────────────────────────────────────────────

def crawl(target_url, out, local_html=None, cookies=None):
    parsed   = urllib.parse.urlparse(target_url)
    base_url = f"{parsed.scheme}://{parsed.netloc}"

    extra_headers = {}
    if cookies:
        extra_headers["Cookie"] = cookies

    js_dir  = out / "js";  js_dir.mkdir(parents=True, exist_ok=True)
    css_dir = out / "css"; css_dir.mkdir(parents=True, exist_ok=True)

    all_findings = collections.defaultdict(set)

    hdr(f"\n{'='*60}")
    hdr(f"  TARGET : {target_url}")
    hdr(f"  OUTPUT : {out}")
    hdr(f"{'='*60}\n")

    # ── 1. Get HTML ───────────────────────────────────────────────────────────
    if local_html:
        info(f"Loading local HTML: {local_html}")
        html_text = Path(local_html).read_text(encoding="utf-8", errors="replace")
        (out / "index.html").write_text(html_text, encoding="utf-8")
        ok(f"Loaded  ({len(html_text):,} bytes)")
    else:
        info("Fetching homepage...")
        raw = fetch(target_url, extra_headers)
        if not raw:
            err("Could not fetch homepage. Try --file or --cookies to bypass Cloudflare.")
            sys.exit(1)
        html_text = raw.decode("utf-8", errors="replace")
        if len(html_text) < 6000 and "challenge" in html_text.lower():
            warn("Looks like a Cloudflare challenge page — use --file or --cookies!")
        (out / "index.html").write_text(html_text, encoding="utf-8")
        ok(f"Homepage saved  ({len(html_text):,} bytes)")

    # ── 2. Parse assets ───────────────────────────────────────────────────────
    parser = AssetParser(base_url)
    parser.feed(html_text)

    # Merge with regex fallback so nothing is missed
    fb_js, fb_css = extract_assets_fallback(html_text, base_url)
    all_js  = list(dict.fromkeys(parser.js  + fb_js))
    all_css = list(dict.fromkeys(parser.css + fb_css))

    info(f"Found  {len(all_js)} JS  |  {len(all_css)} CSS  |  {len(parser.img)} images")

    # ── 3. Download JS ────────────────────────────────────────────────────────
    print(f"\n{C.YELLOW}[ Downloading JS ({len(all_js)} files) ]{C.RESET}")
    js_texts, dl_js = [], []
    for url in all_js:
        info(f"GET {url}")
        data = fetch(url, extra_headers)
        if data:
            dest = js_dir / safe_name(url)
            dest.write_bytes(data)
            js_texts.append(data.decode("utf-8", errors="replace"))
            dl_js.append(url)
            ok(f"{dest.name}  ({len(data):,} bytes)")
        time.sleep(DELAY)

    # ── 4. Download CSS ───────────────────────────────────────────────────────
    print(f"\n{C.YELLOW}[ Downloading CSS ({len(all_css)} files) ]{C.RESET}")
    dl_css = []
    for url in all_css:
        info(f"GET {url}")
        data = fetch(url, extra_headers)
        if data:
            dest = css_dir / safe_name(url)
            dest.write_bytes(data)
            dl_css.append(url)
            ok(f"{dest.name}  ({len(data):,} bytes)")
        time.sleep(DELAY)

    # ── 5. Analyze all JS ─────────────────────────────────────────────────────
    print(f"\n{C.YELLOW}[ Analyzing {len(js_texts)} JS files... ]{C.RESET}")
    combined = "\n".join(js_texts) + "\n" + html_text
    for cat, items in analyze(combined).items():
        all_findings[cat].update(items)

    # ── 6. Print findings ─────────────────────────────────────────────────────
    labels = {
        "api_endpoints":    "API Endpoints",
        "crypto_wallets":   "Crypto Wallet Addresses",
        "secrets":          "Possible Keys / Tokens",
        "emails":           "Email Addresses",
        "external_domains": "External Domains",
    }

    print(f"\n{C.MAG}{'='*60}")
    print(f"  FINDINGS")
    print(f"{'='*60}{C.RESET}")

    report_findings = {}
    for cat, label in labels.items():
        items = sorted(all_findings.get(cat, set()))
        report_findings[cat] = items
        if items:
            print(f"\n{C.MAG}  [{label}]  ({len(items)} found){C.RESET}")
            for item in items:
                hit(item)
        else:
            print(f"\n  [{label}]  -- none found")

    # ── 7. Save report ────────────────────────────────────────────────────────
    report = {
        "target":     target_url,
        "js_files":   all_js,
        "css_files":  all_css,
        "downloaded": {"js": dl_js, "css": dl_css},
        "findings":   report_findings,
    }
    rpath = out / "report.json"
    rpath.write_text(json.dumps(report, indent=2), encoding="utf-8")

    print(f"\n{C.GREEN}{'='*60}")
    print(f"  JS   : {len(dl_js)}/{len(all_js)} downloaded")
    print(f"  CSS  : {len(dl_css)}/{len(all_css)} downloaded")
    print(f"  Report -> {rpath}")
    print(f"{'='*60}{C.RESET}\n")


# ── Entry ─────────────────────────────────────────────────────────────────────

def main():
    ap = argparse.ArgumentParser(
        description="Scam site crawler -- pure Python, no dependencies",
    )
    ap.add_argument("url",            help="Target URL")
    ap.add_argument("--output",  "-o", default=None, help="Output directory")
    ap.add_argument("--file",    "-f", default=None, help="Local HTML file (bypass Cloudflare)")
    ap.add_argument("--cookies", "-c", default=None, help='Cookie string e.g. "cf_clearance=abc; session=xyz"')
    args = ap.parse_args()

    url = args.url
    if not url.startswith("http"):
        url = "https://" + url

    domain = urllib.parse.urlparse(url).netloc.replace(".", "_")
    out    = Path(args.output) if args.output else Path(f"crawl_{domain}")

    crawl(url, out, local_html=args.file, cookies=args.cookies)


if __name__ == "__main__":
    main()
