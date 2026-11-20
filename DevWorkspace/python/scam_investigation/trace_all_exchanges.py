#!/usr/bin/env python3
"""
trace_all_exchanges.py
Traces fund flow from ALL master wallets to ALL exchange hot wallets.
Handles patterns:
  - Master → [1-2 Inter] → Exchange
  - Master → [1-2 Inter] → ConsolMaster → [1-2 Inter] → Exchange

Outputs:
  - all_exchanges_evidence.json
  - All_Exchanges_Evidence.pdf  (summary report)

Usage:
    python trace_all_exchanges.py

Requirements:
    pip install reportlab
"""

import urllib.request
import json
import time
from datetime import datetime
from collections import defaultdict

try:
    from reportlab.lib.pagesizes import A4
    from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
    from reportlab.lib.colors import HexColor, white
    from reportlab.lib.units import mm
    from reportlab.platypus import (SimpleDocTemplate, Paragraph, Spacer,
                                     Table, TableStyle, HRFlowable, PageBreak)
    REPORTLAB = True
except ImportError:
    REPORTLAB = False
    print("  ! pip install reportlab  (needed for PDF)")

# ══════════════════════════════════════════════════════════════
# CONFIG
# ══════════════════════════════════════════════════════════════

MASTER_WALLETS = [
    "TWF1bcuNLXtwQCRxb6Qn6uKqWCuXRatiei",
    "TFBXCAz2uoFUchiYxDCWA8NmKxwCmMJNwg",
    "TLHn4hvwcRcbT8T5zz4icZSYP9ycKQeMya",
    "TX9MCjMWpJpm4qRCpbbbwWXYGwBab1Xp7a",
    "TTDkfRZ83VfXW4M3ayf6QSrteH6EkG3ZVn",
    "TPiDX9JrcHSxh9ekZCxEcAAR4MGMWYFoft",
    "TBPQSHyoRprSADr9uDJzzYFUse6r5fUFvZ",
    "TVu2uAeZfixfSQtKZeh4ztS8Y9zszHo8MZ",
    "TRDjFwWXN9XjPxeBEwXjABncKzdqSuLxsa",
    "TF1n6ykE17g8M9apkRgx3yJZpMwicPmPmk",
    "TK7PTtpEaoXiHdX4qW7gHieKcEJdZNnaPK",
    "THhqGS6izRWFWHNM3kaupCsNBDwEmbrUcq",
    "TX3hfkdQF3gqr8Vz4DyVneWfBi7HFeGfPT",
    "TXSVyoshhSD71vZEbJesK11VMRQN69UP1C",
    "TAkZnpPoRX5ttbQbQJTrXNJUBci1XcgGub",
]

MASTER_SET = set(MASTER_WALLETS)

EXCHANGE_WALLETS = {
    "TDqSquXBgUCLYvYC4XZgrprLK589dkhSCf": ("Binance",  "Hot 7"),
    "TJDENsfBJs4RFETt1X1W8wMDc8M5XnJhCe": ("Binance",  "Hot 6"),
    "TAzsQ9Gx8eqFNFSKbeXrbi45CuVPHzA8wr": ("Binance",  "Hot 5"),
    "TJqwA7SoZnERE4zW5uDEiPkbz4B66h9TFj": ("Binance",  "Hot 11"),
    "TNXoiAJ3dct8Fjg4M9fkLFh9S2v9TXc32G": ("Binance",  "Hot 4"),
    "TYASr5UV6HEcXatwdFQfmLVUqQQQMUxHLS": ("Binance",  "Hot 3"),
    "TU4vEruvZwLLkSfV9bNw12EJTPvNr7Pvaa": ("Bybit",    "Hot"),
    "TBA6CypYJizwA9XdC7Ubgc5F1bxrQ7SqPt": ("Gate.io",  "Hot"),
    "TLaGjwhvA8XQYSxFAcAXy7Dvuue9eGYitv": ("OKX",      "Hot 8"),
    "TM1zzNDZD2DPASbKcgdVoTYhfmYgtfwx9R": ("OKX",      "Hot 1"),
    "TJ7hhYhVhaxNx6BPyq7yFpqZrQULL3JSdb": ("Bitget",   "Hot 9"),
    "TEPSrSYPDSQ7yXpMFPq91Fb1QEWpMkRGfn": ("MEXC",     "Hot"),
    "TUpHuDkiCCmwaTZBHZvQdwWzGNm5t8J2b9": ("KuCoin",   "Hot 4"),
    "TFTWNgDBkQ5wQoP8RXpRznnHvAVV8x5jLu": ("HTX",      "Hot 4"),
    "TXFBqBbqJommqZf7BV8NNYzePh97UmJodJ": ("Bitfinex", "Hot"),
}

MAX_HOPS        = 5     # max chain depth
MAX_PER_MASTER  = 30    # stop after finding N txs per master wallet
MIN_AMOUNT_USDT = 500   # ignore hops smaller than $500 USDT
FETCH_LIMIT     = 30    # transfers per API call
DELAY           = 0.6   # seconds between requests

TRONSCAN = "https://apilist.tronscanapi.com/api"
USDT     = "TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t"
HEADERS  = {"User-Agent": "Mozilla/5.0", "Accept": "application/json"}

# ══════════════════════════════════════════════════════════════
# HELPERS
# ══════════════════════════════════════════════════════════════

def fetch(url):
    req = urllib.request.Request(url, headers=HEADERS)
    try:
        with urllib.request.urlopen(req, timeout=15) as r:
            return json.loads(r.read().decode("utf-8"))
    except Exception as e:
        print(f"    ! {e}")
        return None

def get_transfers(address, direction="out", start=0):
    url = (f"{TRONSCAN}/token_trc20/transfers"
           f"?address={address}&direction={direction}"
           f"&limit={FETCH_LIMIT}&start={start}"
           f"&trc20Id={USDT}")
    return fetch(url)

def to_usdt(quant):
    """
    TronScan token_trc20/transfers returns 'quant' in SUN.
    1 USDT = 1,000,000 SUN (USDT has 6 decimals on Tron).
    Always divide by 1,000,000.
    """
    try:
        return float(quant) / 1_000_000
    except:
        return 0.0

def short(addr):
    return addr[:10] + "..." + addr[-6:]

def classify(addr, origin):
    if addr == origin:          return "origin_master"
    if addr in MASTER_SET:      return "consolidation_master"
    if addr in EXCHANGE_WALLETS: return "exchange"
    return "intermediate"

def build_labeled_path(path_addrs, origin):
    labeled = []
    inter_n = 0
    for addr in path_addrs:
        role = classify(addr, origin)
        if role == "origin_master":
            label = "Master"
        elif role == "consolidation_master":
            label = "ConsolMaster"
        elif role == "intermediate":
            inter_n += 1
            label = f"Inter{inter_n}"
        else:
            ex, hot = EXCHANGE_WALLETS[addr]
            label = f"{ex} {hot}"
        labeled.append({"addr": addr, "role": role, "label": label})
    pattern = " → ".join(h["label"] for h in labeled)
    return labeled, pattern

# ══════════════════════════════════════════════════════════════
# TRACER
# ══════════════════════════════════════════════════════════════

def trace_master(origin):
    """
    BFS from origin master wallet.
    Path rules:
      - Max 2 intermediates before ConsolMaster
      - Max 1 ConsolMaster per path
      - Max 2 intermediates after ConsolMaster
      - Stop master after MAX_PER_MASTER found
    """
    results = []
    visited = set()
    # queue: (address, path_addresses_so_far, depth)
    # path stores list of {"addr":, "tx_hash":, "amount_usdt":} dicts
    # Master wallet has no incoming tx (it's the origin)
    queue   = [(origin, [{"addr": origin, "tx_hash": None, "amount_usdt": 0}], 0)]

    while queue and len(results) < MAX_PER_MASTER:
        current, path, depth = queue.pop(0)

        if current in visited or depth > MAX_HOPS:
            continue
        visited.add(current)

        role   = classify(current, origin)
        indent = "  " * depth

        # Count hops in current path
        # path is list of dicts {"addr":, "tx_hash":, "amount_usdt":}
        roles_in_path = [classify(a["addr"], origin) for a in path]
        n_consol = sum(1 for r in roles_in_path if r == "consolidation_master")
        n_inter  = sum(1 for r in roles_in_path if r == "intermediate")

        # Find split point at consolidation master
        passed_consol = "consolidation_master" in roles_in_path
        if passed_consol:
            consol_idx   = roles_in_path.index("consolidation_master")
            inter_before = sum(1 for r in roles_in_path[:consol_idx]   if r == "intermediate")
            inter_after  = sum(1 for r in roles_in_path[consol_idx+1:] if r == "intermediate")
        else:
            inter_before = n_inter
            inter_after  = 0

        label_disp = {
            "origin_master":        f"[Master]",
            "consolidation_master": f"[ConsolMaster]",
            "intermediate":         f"[Inter]",
            "exchange":             f"[Exchange]",
        }.get(role, "")
        print(f"{indent}[d{depth}] {label_disp} {short(current)}", end="")

        # Paginate for consolidation masters (more transfers)
        if role == "consolidation_master":
            transfers = []
            for page in range(5):
                data = get_transfers(current, start=page * FETCH_LIMIT)
                time.sleep(DELAY)
                if not data:
                    break
                batch = data.get("token_transfers", [])
                transfers.extend(batch)
                if len(batch) < FETCH_LIMIT:
                    break
        else:
            data = get_transfers(current)
            time.sleep(DELAY)
            transfers = data.get("token_transfers", []) if data else []

        print(f" — {len(transfers)} transfers")

        for tx in transfers:
            to_addr = tx.get("to_address", "")
            tx_hash = tx.get("transaction_id", "")
            # Always use 'quant' field and divide by 1e6
            amount  = to_usdt(tx.get("quant", 0))

            if not to_addr or not tx_hash:
                continue

            to_role  = classify(to_addr, origin)
            new_path = path + [{"addr": to_addr, "tx_hash": tx_hash, "amount_usdt": round(amount, 6)}]

            # ── HIT: reached exchange ─────────────────────────
            if to_role == "exchange":
                ex, hot = EXCHANGE_WALLETS[to_addr]
                path_addrs = [h["addr"] for h in new_path]
                labeled, pattern = build_labeled_path(path_addrs, origin)
                cross_masters = [h["addr"] for h in labeled
                                 if h["role"] == "consolidation_master"]

                # Merge path hops with labels
                path_hops = []
                for pi, ph in enumerate(new_path):
                    hop_label = labeled[pi]["label"] if pi < len(labeled) else ""
                    hop_role  = labeled[pi]["role"]  if pi < len(labeled) else "intermediate"
                    path_hops.append({
                        "hop":         pi,
                        "addr":        ph["addr"],
                        "tx_hash":     ph["tx_hash"],
                        "amount_usdt": ph["amount_usdt"],
                        "label":       hop_label,
                        "role":        hop_role,
                        "tronscan_addr": f"https://tronscan.org/#/address/{ph['addr']}",
                        "tronscan_tx":   f"https://tronscan.org/#/transaction/{ph['tx_hash']}"
                                         if ph["tx_hash"] else None,
                    })

                print(f"\n{indent}  🎯 {ex} {hot}  |  {pattern}  |  "
                      f"${amount:,.2f}  |  {tx_hash[:16]}...")

                results.append({
                    "master_wallet":  origin,
                    "exchange":       ex,
                    "hot_wallet":     hot,
                    "exchange_addr":  to_addr,
                    "tx_hash":        tx_hash,
                    "amount_usdt":    round(amount, 6),
                    "from_address":   current,
                    "depth":          depth + 1,
                    "pattern":        pattern,
                    "path_addrs":     path_addrs,
                    "path_hops":      path_hops,
                    "labeled_path":   labeled,
                    "cross_masters":  cross_masters,
                    "tronscan":       f"https://tronscan.org/#/transaction/{tx_hash}",
                })
                if len(results) >= MAX_PER_MASTER:
                    print(f"\n  ✅ Reached {MAX_PER_MASTER} txs — next master")
                    break
                continue

            # ── QUEUE next hop if allowed ─────────────────────
            if to_addr in visited:
                continue
            if amount < MIN_AMOUNT_USDT:
                continue
            if depth >= MAX_HOPS:
                continue

            # Apply path rules
            skip = False
            if to_role == "consolidation_master":
                if n_consol >= 1:           # max 1 consol per path
                    skip = True
                elif inter_before > 2:      # max 2 inter before consol
                    skip = True
            elif to_role == "intermediate":
                if passed_consol and inter_after >= 2:    # max 2 inter after consol
                    skip = True
                elif not passed_consol and inter_before >= 2:  # max 2 inter before consol
                    skip = True
            elif to_role == "origin_master":
                skip = True  # don't loop back to self

            if not skip:
                if to_role == "consolidation_master":
                    print(f"{indent}  ↪ ConsolMaster {short(to_addr)} (${amount:,.2f})")
                queue.append((to_addr, new_path, depth + 1))  # new_path is list of dicts

    return results

# ══════════════════════════════════════════════════════════════
# PDF SUMMARY REPORT
# ══════════════════════════════════════════════════════════════

def make_summary_pdf(all_results, path):
    if not REPORTLAB:
        return

    RED   = HexColor('#C00000')
    NAVY  = HexColor('#1F3864')
    LGRAY = HexColor('#F5F5F5')
    MGRAY = HexColor('#DDDDDD')
    GREEN = HexColor('#1E6B2E')

    doc = SimpleDocTemplate(path, pagesize=A4,
                            leftMargin=12*mm, rightMargin=12*mm,
                            topMargin=12*mm,  bottomMargin=12*mm)
    ss  = getSampleStyleSheet()
    W   = 186*mm

    def sty(n, **kw):
        return ParagraphStyle(n+str(id(kw)), parent=ss['Normal'], **kw)

    def banner(text, color=NAVY, fs=13):
        s = sty('B', fontSize=fs, textColor=white, fontName='Helvetica-Bold', alignment=1)
        t = Table([[Paragraph(text, s)]], colWidths=[W])
        t.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,-1),color),
                                ('TOPPADDING',(0,0),(-1,-1),7),
                                ('BOTTOMPADDING',(0,0),(-1,-1),7)]))
        return t

    def sec(text, color=NAVY):
        s = sty('S', fontSize=11, textColor=white, fontName='Helvetica-Bold')
        t = Table([[Paragraph(text, s)]], colWidths=[W])
        t.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,-1),color),
                                ('TOPPADDING',(0,0),(-1,-1),5),
                                ('BOTTOMPADDING',(0,0),(-1,-1),5)]))
        return t

    body_s = sty('Bo', fontSize=9,  fontName='Helvetica', leading=13)
    mono_s = sty('Mo', fontSize=7,  fontName='Courier',   leading=11, wordWrap='LTR')
    lbl_s  = sty('Lb', fontSize=9,  fontName='Helvetica-Bold')
    sm_s   = sty('Sm', fontSize=7,  fontName='Helvetica', textColor=HexColor('#555'), leading=10)
    red_s  = sty('Re', fontSize=9,  fontName='Helvetica-Bold', textColor=RED)
    mono_l = sty('Ml', fontSize=8,  fontName='Courier', leading=12, wordWrap='LTR')

    total_usdt   = sum(r['amount_usdt'] for r in all_results)
    date_str     = datetime.now().strftime("%Y-%m-%d %H:%M UTC")

    by_exchange  = defaultdict(list)
    by_master    = defaultdict(list)
    for r in all_results:
        by_exchange[r['exchange_addr']].append(r)
        by_master[r['master_wallet']].append(r)

    story = []

    # ── Cover ─────────────────────────────────────────────────
    story.append(Spacer(1, 10*mm))
    story.append(banner('MULTI-EXCHANGE FRAUD TRANSACTION EVIDENCE', RED, 14))
    story.append(Spacer(1, 2*mm))
    story.append(banner('BG Wealth Sharing / DSJEX — Pig Butchering Scam', NAVY, 10))
    story.append(Spacer(1, 6*mm))

    def kv(rows):
        data = [[Paragraph(k, lbl_s), Paragraph(v, sty('V',fontSize=8,fontName='Helvetica',
                leading=12,wordWrap='LTR'))] for k,v in rows]
        t = Table(data, colWidths=[44*mm, W-44*mm])
        t.setStyle(TableStyle([
            ('BACKGROUND',(0,0),(0,-1),LGRAY),
            ('GRID',(0,0),(-1,-1),0.3,MGRAY),
            ('TOPPADDING',(0,0),(-1,-1),4),
            ('BOTTOMPADDING',(0,0),(-1,-1),4),
            ('LEFTPADDING',(0,0),(-1,-1),5),
            ('VALIGN',(0,0),(-1,-1),'TOP'),
        ]))
        return t

    story.append(kv([
        ('Report Date',      date_str),
        ('Master Wallets',   f'{len(MASTER_WALLETS)} traced'),
        ('Exchange Targets', f'{len(EXCHANGE_WALLETS)} hot wallets across 9 exchanges'),
        ('Txs Found',        str(len(all_results))),
        ('Total USDT',       f'${total_usdt:,.2f} confirmed'),
        ('Total Stolen',     '~$1,000,000,000+ USDT (all victims)'),
        ('Status',           'Platform shut down May 2026 — demanding unlock fees'),
        ('Full Report',      'https://medium.com/@hades280389/i-reverse-engineered-bg-wealth-sharing-inside-the-infrastructure-of-a-billion-dollar-pig-60b7de582483'),
    ]))

    # ── By exchange ───────────────────────────────────────────
    story.append(PageBreak())
    story.append(sec('1. RESULTS BY EXCHANGE', NAVY))
    story.append(Spacer(1, 4*mm))

    hdr = [[Paragraph(h, lbl_s) for h in
            ['Exchange','Wallet','Txs','Total USDT','Address']]]
    for addr, (ex, hot) in EXCHANGE_WALLETS.items():
        txs  = by_exchange.get(addr, [])
        tot  = sum(r['amount_usdt'] for r in txs)
        hdr.append([
            Paragraph(ex, body_s),
            Paragraph(hot, body_s),
            Paragraph(f'{"✅ " if txs else "—"}{len(txs)}', body_s),
            Paragraph(f'${tot:,.2f}' if txs else '—', body_s),
            Paragraph(addr, mono_s),
        ])

    t = Table(hdr, colWidths=[20*mm,16*mm,18*mm,26*mm,W-80*mm])
    t.setStyle(TableStyle([
        ('BACKGROUND',(0,0),(-1,0),NAVY),
        ('TEXTCOLOR',(0,0),(-1,0),white),
        ('FONTNAME',(0,0),(-1,0),'Helvetica-Bold'),
        ('FONTSIZE',(0,0),(-1,-1),8),
        ('GRID',(0,0),(-1,-1),0.3,MGRAY),
        ('TOPPADDING',(0,0),(-1,-1),4),
        ('BOTTOMPADDING',(0,0),(-1,-1),4),
        ('LEFTPADDING',(0,0),(-1,-1),4),
        ('ROWBACKGROUNDS',(0,1),(-1,-1),[white,LGRAY]),
        ('VALIGN',(0,0),(-1,-1),'MIDDLE'),
        ('FONTNAME',(4,1),(4,-1),'Courier'),
    ]))
    story.append(t)

    # ── By master wallet ──────────────────────────────────────
    story.append(Spacer(1, 5*mm))
    story.append(sec('2. RESULTS BY MASTER WALLET', NAVY))
    story.append(Spacer(1, 4*mm))

    hdr2 = [[Paragraph(h, lbl_s) for h in
             ['#','Master Wallet Address','Txs','Total USDT','Exchanges']]]
    for i, master in enumerate(MASTER_WALLETS, 1):
        txs  = by_master.get(master, [])
        tot  = sum(r['amount_usdt'] for r in txs)
        exs  = ', '.join(sorted(set(r['exchange'] for r in txs))) if txs else '—'
        hdr2.append([
            Paragraph(str(i), body_s),
            Paragraph(master, mono_s),
            Paragraph(f'{"✅ " if txs else "—"}{len(txs)}', body_s),
            Paragraph(f'${tot:,.2f}' if txs else '—', body_s),
            Paragraph(exs, body_s),
        ])

    t2 = Table(hdr2, colWidths=[8*mm,82*mm,14*mm,26*mm,W-130*mm])
    t2.setStyle(TableStyle([
        ('BACKGROUND',(0,0),(-1,0),NAVY),
        ('TEXTCOLOR',(0,0),(-1,0),white),
        ('FONTNAME',(0,0),(-1,0),'Helvetica-Bold'),
        ('FONTSIZE',(0,0),(-1,-1),8),
        ('GRID',(0,0),(-1,-1),0.3,MGRAY),
        ('TOPPADDING',(0,0),(-1,-1),4),
        ('BOTTOMPADDING',(0,0),(-1,-1),4),
        ('LEFTPADDING',(0,0),(-1,-1),4),
        ('ROWBACKGROUNDS',(0,1),(-1,-1),[white,LGRAY]),
        ('VALIGN',(0,0),(-1,-1),'MIDDLE'),
        ('FONTNAME',(1,1),(1,-1),'Courier'),
    ]))
    story.append(t2)

    # ── All transactions ──────────────────────────────────────
    story.append(PageBreak())
    story.append(sec('3. ALL CONFIRMED TRANSACTIONS', RED))
    story.append(Spacer(1, 3*mm))

    tx_hdr = [[Paragraph(h, lbl_s) for h in
               ['#','Transaction Hash (full 64 chars)','USDT','Exchange','From']]]
    for i, r in enumerate(all_results, 1):
        tx_hdr.append([
            Paragraph(str(i), body_s),
            Paragraph(r['tx_hash'], mono_s),
            Paragraph(f"${r['amount_usdt']:,.2f}", body_s),
            Paragraph(f"{r['exchange']} {r['hot_wallet']}", body_s),
            Paragraph(r['from_address'], mono_s),
        ])

    tt = Table(tx_hdr, colWidths=[8*mm,72*mm,22*mm,22*mm,W-124*mm])
    tt.setStyle(TableStyle([
        ('BACKGROUND',(0,0),(-1,0),RED),
        ('TEXTCOLOR',(0,0),(-1,0),white),
        ('FONTSIZE',(0,0),(-1,-1),7),
        ('GRID',(0,0),(-1,-1),0.3,MGRAY),
        ('TOPPADDING',(0,0),(-1,-1),3),
        ('BOTTOMPADDING',(0,0),(-1,-1),3),
        ('LEFTPADDING',(0,0),(-1,-1),4),
        ('ROWBACKGROUNDS',(0,1),(-1,-1),[white,LGRAY]),
        ('VALIGN',(0,0),(-1,-1),'MIDDLE'),
    ]))
    story.append(tt)

    # ── Requests ──────────────────────────────────────────────
    story.append(PageBreak())
    story.append(sec('4. FORMAL REQUESTS TO ALL EXCHANGES', RED))
    story.append(Spacer(1, 3*mm))
    reqs = [
        ('Request 1','Identify all internal accounts that received funds traceable to the 15 master wallets listed above'),
        ('Request 2','Freeze all identified accounts and associated accounts immediately'),
        ('Request 3','Preserve all KYC records, IP logs, device fingerprints, and transaction history'),
        ('Request 4','Share account identities with law enforcement: Vietnam NCSC, FBI IC3, Interpol'),
        ('Request 5','Provide case reference number for follow-up'),
        ('URGENCY','Platform shut down May 2026 — operators may be preparing to disappear'),
    ]
    story.append(kv(reqs))

    story.append(Spacer(1, 4*mm))
    story.append(HRFlowable(width='100%', thickness=0.5, color=MGRAY))
    story.append(Spacer(1, 3*mm))
    story.append(Paragraph(
        f'All blockchain data publicly verifiable on TronScan. '
        f'Generated: {date_str}.',
        sm_s))

    doc.build(story)
    print(f"  PDF -> {path}")

# ══════════════════════════════════════════════════════════════
# MAIN
# ══════════════════════════════════════════════════════════════

def main():
    all_results = []

    print(f"\n{'='*65}")
    print(f"  TRACING {len(MASTER_WALLETS)} MASTER WALLETS")
    print(f"  Targets : {len(EXCHANGE_WALLETS)} exchange hot wallets")
    print(f"  Max hops: {MAX_HOPS}  |  Max/master: {MAX_PER_MASTER}  |  Min: ${MIN_AMOUNT_USDT}")
    print(f"{'='*65}\n")

    for i, master in enumerate(MASTER_WALLETS, 1):
        print(f"\n[{i}/{len(MASTER_WALLETS)}] {master}")
        print('─' * 55)
        results = trace_master(master)
        all_results.extend(results)
        tot = sum(r['amount_usdt'] for r in results)
        exs = sorted(set(r['exchange'] for r in results))
        print(f"\n  → {len(results)} txs  ${tot:,.2f} USDT  → {', '.join(exs) or 'none'}")

    # ── Summary ───────────────────────────────────────────────
    print(f"\n{'='*65}")
    print(f"  FINAL SUMMARY")
    print(f"{'='*65}")
    print(f"  Total transactions: {len(all_results)}")
    print(f"  Total USDT:         ${sum(r['amount_usdt'] for r in all_results):,.2f}")
    print()

    by_ex = defaultdict(lambda: {"count":0,"total":0})
    for r in all_results:
        k = f"{r['exchange']} {r['hot_wallet']}"
        by_ex[k]["count"] += 1
        by_ex[k]["total"] += r['amount_usdt']
    for k, v in sorted(by_ex.items()):
        print(f"  {k:22} {v['count']:4} txs   ${v['total']:,.2f} USDT")

    # ── Save JSON ─────────────────────────────────────────────
    with open("all_exchanges_evidence.json", "w") as f:
        json.dump(all_results, f, indent=2)
    print(f"\n  JSON -> all_exchanges_evidence.json")

    # ── Generate PDF ──────────────────────────────────────────
    print("\n  Generating PDF...")
    make_summary_pdf(all_results, "All_Exchanges_Evidence.pdf")

    print(f"\n{'='*65}")
    print(f"  DONE")
    print(f"  JSON -> all_exchanges_evidence.json")
    print(f"  PDF  -> All_Exchanges_Evidence.pdf")
    print(f"  Next -> python generate_master_reports.py")
    print(f"{'='*65}\n")

if __name__ == "__main__":
    main()
