"""Làm sạch văn bản OCR và chống đọc lặp."""
from __future__ import annotations

import difflib
import re
import unicodedata

_WS = re.compile(r"\s+")
_LEADING_DASH = re.compile(r"^[\-–—•·]+\s*")
_HAS_LETTER = re.compile(r"[^\W\d_]", re.UNICODE)
# Một "chữ" hợp lệ: chỉ gồm chữ cái (hoặc số), có thể kèm dấu câu ở đầu/cuối
_GOOD_TOKEN = re.compile(r"^[\"'“”‘’(\[-]*([^\W\d_]+|\d+([.,:/]\d+)*[^\W\d_]{0,3}|[^\W\d_]{1,3}\d+)[.,!?…:;\"'“”‘’)\]-]*$", re.UNICODE)


_TONES = {"̀", "́", "̃", "̉", "̣"}  # huyền, sắc, ngã, hỏi, nặng
_SYLLABLE = re.compile(
    r"^(ngh|ng|nh|ch|gh|gi|kh|ph|qu|th|tr|[bcdđghklmnprstvx])?"  # phụ âm đầu
    r"[aăâeêioôơuưy]{1,3}"                                        # vần (nguyên âm)
    r"(ch|ng|nh|[cmnpt])?$"                                        # phụ âm cuối
)


def is_vietnamese_syllable(word: str) -> bool:
    """Chữ có ghép được thành âm tiết tiếng Việt không ("rồi", "nghiêng" có; "mm", "xx", "nz" không)."""
    nfd = unicodedata.normalize("NFD", word.lower())
    if sum(ch in _TONES for ch in nfd) > 1:
        return False
    base = unicodedata.normalize("NFC", "".join(ch for ch in nfd if ch not in _TONES))
    return bool(_SYLLABLE.match(base))


def _token_ok(t: str) -> bool:
    m = _GOOD_TOKEN.match(t)
    if not m:
        return False
    core = m.group(1)
    if any(a.islower() and b.isupper() for a, b in zip(core, core[1:])):
        return False  # "xX", "bỀ", "ưG": hoa/thường lẫn lộn -> rác OCR
    return any(ch.isdigit() for ch in core) or is_vietnamese_syllable(core)


def _trim_junk(line: str) -> str:
    """Bỏ các mẩu rác ở đầu/cuối dòng ("` ỄN I -~ Bác sĩ nói rồi, _ c" -> "Bác sĩ nói rồi,")."""
    toks = line.split()
    symbol = lambda t: t not in ("-", "–", "—") and not _GOOD_TOKEN.match(t)  # "-~", "`", "_"; gạch thoại thì giữ
    junk = lambda t: symbol(t) or (len(t.strip(".,!?…:;-")) == 1 and not _token_ok(t))
    # ký hiệu rác nằm trong 3 chữ đầu/cuối -> bỏ luôn phần mép đó ("ỄN I -~ Bác sĩ..." -> "Bác sĩ...")
    while True:
        head = [i for i, t in enumerate(toks[:3]) if symbol(t)]
        tail = [i for i, t in enumerate(toks) if symbol(t) and i >= len(toks) - 3]
        if head:
            toks = toks[head[-1] + 1:]
        elif tail:
            toks = toks[:tail[0]]
        else:
            break
    while toks and junk(toks[0]):
        toks.pop(0)
    while toks and junk(toks[-1]):
        toks.pop()
    return " ".join(toks)


def looks_vietnamese(text: str, min_ratio: float = 0.65) -> bool:
    """Loại chuỗi rác do OCR chữ Hán/logo/hiệu ứng (vd "ni ¿c>* 2h JR", "IIG. mm ụw u. xx").

    Đếm tỉ lệ chữ là âm tiết tiếng Việt hợp lệ (số được tính là hợp lệ).
    """
    tokens = text.split()
    if not tokens:
        return False
    return sum(_token_ok(t) for t in tokens) / len(tokens) >= min_ratio


# Tesseract hay mất dấu huyền/ngã chồng lên mũ (ồ, ầ, ẫ) với font phụ đề nhỏ. Chỉ sửa các chữ mà dạng sai
# hầu như không dùng trong lời thoại (đo trên phim thật: "rồi" -> "rôi" chiếm 4/7 câu sai).
_OCR_FIXES = {"rôi": "rồi", "rât": "rất", "vân": "vẫn", "cân": "cần", "se": "sẽ"}
# Sửa theo chữ đứng trước: (chữ trước, chữ sai) -> chữ đúng
_PAIR_FIXES = {("theo", "đuối"): "đuổi", ("vớ", "vấn"): "vẩn", ("quản", "nối"): "nổi", ("biết", "đầu"): "đâu"}
# "đâu" bị đọc thành "đầu" (cả Tesseract lẫn VietOCR): đổi lại khi rõ là "đâu" theo ngữ cảnh
_DAU_KEEP_BEFORE = {"bắt", "từ", "lúc", "ban", "cái", "đau", "mở", "cúi", "gật", "lắc", "quay", "hàng", "trên",
                    "dưới", "đứng", "dẫn", "khởi", "nhức", "đầu", "trọc", "chặt", "đâm", "ngóc", "ngẩng", "đỉnh"}
_DAU_AFTER = {"có", "cũng", "phải", "chứ", "ạ", "mà", "nhé", "nhỉ", "nào", "đấy", "được", "hết", "nữa", "đâu", "ra"}
_WORD_CORE = re.compile(r"^(\W*)(.*?)(\W*)$", re.UNICODE)


def fix_common_ocr(text: str) -> str:
    toks = text.split()
    for i, t in enumerate(toks):
        lead, core, trail = _WORD_CORE.match(t).groups()
        fixed = _OCR_FIXES.get(core.lower())
        if not fixed or (core != core.lower() and core.lower() in ("vân", "cân")):
            continue  # "Vân" viết hoa có thể là tên người
        if core.lower() == "se" and i > 0 and toks[i - 1].lower() == "chia":
            fixed = "sẻ"  # chia sẻ
        if core[0].isupper():
            fixed = fixed[0].upper() + fixed[1:]
        toks[i] = lead + fixed + trail
    for i in range(1, len(toks)):
        prev = toks[i - 1].lower().strip(".,!?…:;\"'()[]")
        lead, core, trail = _WORD_CORE.match(toks[i]).groups()
        fixed = _PAIR_FIXES.get((prev, core.lower()))
        if fixed is None and core.lower() == "đầu" and prev not in _DAU_KEEP_BEFORE and not toks[i - 1][-1:] in ".!?":
            nxt = toks[i + 1].lower().strip(".,!?…:;\"'()[]") if i + 1 < len(toks) else ""
            if trail[:1] in (",", ".", "?", "!", "…") or nxt in _DAU_AFTER or not nxt:
                fixed = "đâu"  # "nói gì đầu," / "Không đầu ạ." / "đến đầu cũng"
        if fixed:
            toks[i] = lead + (fixed[0].upper() + fixed[1:] if core[:1].isupper() else fixed) + trail
    return " ".join(toks)


def clean_text(raw: str, blacklist: list[str], min_chars: int = 2) -> str:
    text = unicodedata.normalize("NFC", raw)
    lines = []
    for line in text.splitlines():
        line = _WS.sub(" ", line).strip()
        if not line:
            continue
        low = line.lower()
        if any(b and b.lower() in low for b in blacklist):
            continue
        line = _trim_junk(_LEADING_DASH.sub("", line))  # "- Chào anh" (hội thoại 2 người)
        if line:
            lines.append(line)
    text = " ".join(lines)
    text = text.replace("|", "I").replace(" ,", ",").replace(" .", ".").replace(" ?", "?").replace(" !", "!")
    text = _WS.sub(" ", text).strip()
    letters = len(_HAS_LETTER.findall(text))
    if letters < min_chars or not looks_vietnamese(text):
        return ""
    return fix_common_ocr(text)


def similarity(a: str, b: str) -> float:
    if not a or not b:
        return 0.0
    return difflib.SequenceMatcher(None, a.lower(), b.lower()).ratio()


def vote(candidates: list[str]) -> str:
    """Gộp nhiều kết quả OCR của cùng một câu (nhiều khung hình / nhiều cách xử lý ảnh).

    Lấy nhóm có cùng số chữ đông nhất, rồi chọn từng chữ theo số đông.
    Hoà phiếu thì ưu tiên kết quả đứng trước (cách xử lý chính).
    """
    from collections import Counter

    cands = [c for c in candidates if c]
    if not cands:
        return ""
    groups: dict[int, list[list[str]]] = {}
    for c in cands:
        groups.setdefault(len(c.split()), []).append(c.split())
    best_len = max(groups, key=lambda n: (len(groups[n]), -min(i for i, c in enumerate(cands) if len(c.split()) == n)))
    rows = groups[best_len]
    words = []
    for i in range(best_len):
        counts = Counter(r[i] for r in rows)
        top = max(counts.values())
        words.append(next(r[i] for r in rows if counts[r[i]] == top))  # hoà -> theo thứ tự ưu tiên
    return " ".join(words)


_TONE_MARKS = {"̀", "́", "̃", "̉", "̣"}   # huyền, sắc, ngã, hỏi, nặng
_HAT_MARKS = {"̂", "̆", "̛"}                          # mũ (â ê ô), trăng (ă), móc (ơ ư)
_MARKS = _TONE_MARKS | _HAT_MARKS


def _letters_and_marks(core: str) -> tuple[str, set]:
    """("lần") -> ("lan", {(1, mũ), (1, huyền)}): chữ cái gốc + các dấu kèm vị trí."""
    base, marks, i = [], set(), -1
    for c in unicodedata.normalize("NFD", core.lower()):
        if c in _MARKS:
            marks.add((i, c))
        else:
            base.append("d" if c == "đ" else c)
            i += 1
    return "".join(base), marks


def merge_ocr(tess: str, viet: str, tess_candidates: list[str] | tuple = ()) -> str:
    """Ghép kết quả Tesseract (đã bỏ phiếu) với VietOCR.

    Mặc định tin Tesseract (ít sai hơn khi đứng riêng). Chữ nào hai bên giống hệt phần chữ cái mà Tesseract
    THIẾU dấu so với VietOCR (lân/lần, tiên/tiền, Ong/Ông) thì lấy của VietOCR: Tesseract hay làm rơi dấu chồng,
    còn khi VietOCR sai thì thường là nhầm loại dấu chứ ít khi thêm dấu thừa.
    Số chữ khác nhau (Tesseract đọc thiếu nửa dòng) -> chỉ đổi sang một lần đọc khác của Tesseract có cùng số chữ
    với VietOCR, tránh lấy chữ VietOCR "bịa" ra từ nền ảnh.
    Đo trên 150 câu phim thật (xem README).
    """
    viet = " ".join(w for w in viet.split() if w not in ("-", "–", "—"))
    tw, vw = tess.split(), viet.split()
    if not tess or not viet:
        return tess or viet
    if len(tw) != len(vw):
        # ...và lần đọc đó phải có thêm chữ thật (>= 2 chữ cái), không phải mẩu rác như "HN", "Ầ"
        real = lambda t: sum(1 for w in t.split() if len(_WORD_CORE.match(w).group(2)) >= 2
                             and is_vietnamese_syllable(_WORD_CORE.match(w).group(2)))
        alt = [c for c in tess_candidates if c and len(c.split()) == len(vw) and similarity(c, viet) >= 0.6
               and real(c) > real(tess)]
        if not alt:
            return tess
        tw = alt[0].split()
    out = []
    for a, b in zip(tw, vw):
        lead, ca, trail = _WORD_CORE.match(a).groups()
        cb = _WORD_CORE.match(b).group(2)
        (ba, ma), (bb, mb) = _letters_and_marks(ca), _letters_and_marks(cb)
        extra = mb - ma
        # Chỉ nhận dấu THANH mà Tesseract làm rơi trên nguyên âm có mũ/móc (lân→lần, tiên→tiền, Hông→Hồng).
        # Thêm mũ/trăng hoặc dấu thanh trên nguyên âm thường thì VietOCR hay sai (nói→nối, anh→ănh, nghe→nghè).
        hats = {i for i, m in ma if m in _HAT_MARKS}
        if ca and ca != cb and ba == bb and ma < mb and all(m in _TONE_MARKS and i in hats for i, m in extra):
            fixed = unicodedata.normalize("NFC", cb.lower())
            if ca[:1].isupper():
                fixed = fixed[:1].upper() + fixed[1:]
            out.append(lead + fixed + trail)
        else:
            out.append(a)
    return " ".join(out)
