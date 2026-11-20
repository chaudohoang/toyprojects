#!/usr/bin/env python3
"""
generate_master_reports.py
Generates one PDF per master wallet.
Each PDF shows every chain path with full transaction details
including intermediate hops.

Usage:
    python generate_master_reports.py

Requires: pip install reportlab
Reads:    all_exchanges_evidence.json  (from trace_all_exchanges.py)
Outputs:  master_reports/ folder
"""

import json, os
from datetime import datetime
from collections import defaultdict

try:
    from reportlab.lib.pagesizes import A4
    from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
    from reportlab.lib.colors import HexColor, white
    from reportlab.lib.units import mm
    from reportlab.platypus import (SimpleDocTemplate, Paragraph, Spacer,
                                     Table, TableStyle, HRFlowable, PageBreak)
except ImportError:
    print("Run: pip install reportlab"); exit(1)

# ── Config ────────────────────────────────────────────────────
INPUT_FILE = "all_exchanges_evidence.json"
OUTPUT_DIR = "master_reports"

if not os.path.exists(INPUT_FILE):
    print(f"! {INPUT_FILE} not found — run trace_all_exchanges.py first")
    exit(1)

os.makedirs(OUTPUT_DIR, exist_ok=True)
all_results = json.load(open(INPUT_FILE))

# Support both old 'amount' and new 'amount_usdt' field
def get_amt(tx):
    v = float(tx.get('amount_usdt', tx.get('amount', 0)))
    # Catch old raw SUN values that weren't converted
    if v > 1_000_000:
        v = v / 1_000_000
    return v

print(f"Loaded {len(all_results)} transactions")

# ── Exchange wallet reference ─────────────────────────────────
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

MASTER_SET = {
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
}

# ── Colors ────────────────────────────────────────────────────
C_RED     = HexColor('#C00000')
C_DKRED   = HexColor('#7B0000')
C_NAVY    = HexColor('#1F3864')
C_ORANGE  = HexColor('#C55A00')
C_PURPLE  = HexColor('#4A235A')
C_TEAL    = HexColor('#1A5276')
C_GREEN   = HexColor('#1E6B2E')
C_LGRAY   = HexColor('#F5F5F5')
C_MGRAY   = HexColor('#DDDDDD')

ROLE_COLORS = {
    "origin_master":        C_RED,
    "consolidation_master": C_DKRED,
    "intermediate_1":       C_ORANGE,
    "intermediate_2":       C_PURPLE,
    "intermediate_3":       C_TEAL,
    "exchange":             C_GREEN,
}

# ── Role classifier ───────────────────────────────────────────
def classify(addr, origin):
    if addr == origin:
        return "origin_master"
    if addr in MASTER_SET:
        return "consolidation_master"
    if addr in EXCHANGE_WALLETS:
        return "exchange"
    return "intermediate"

def get_labeled_path(tx, origin):
    """Get or rebuild labeled_path."""
    if tx.get('labeled_path'):
        return tx['labeled_path']
    # Rebuild from path_addrs
    addrs = tx.get('path_addrs', [])
    if not addrs:
        addrs = [origin, tx.get('from_address',''), tx['exchange_addr']]
    # Deduplicate
    seen, unique = set(), []
    for a in addrs:
        if a and a not in seen:
            seen.add(a); unique.append(a)

    labeled = []
    inter_n = 0
    for addr in unique:
        role = classify(addr, origin)
        if role == "intermediate":
            inter_n += 1
            label = f"Intermediate {inter_n}"
        elif role == "origin_master":
            label = "Master Wallet"
        elif role == "consolidation_master":
            label = "Consolidation Master"
        else:
            ex, hot = EXCHANGE_WALLETS.get(addr, ('Exchange',''))
            label = f"{ex} {hot}"
        labeled.append({"addr": addr, "role": role, "label": label})
    return labeled

# ══════════════════════════════════════════════════════════════
# PDF BUILDER
# ══════════════════════════════════════════════════════════════
ss = getSampleStyleSheet()
W  = 186*mm

def sty(n, **kw):
    return ParagraphStyle(n+str(abs(hash(str(kw)))), parent=ss['Normal'], **kw)

def banner(text, color=C_NAVY, fs=13):
    s = sty('bn', fontSize=fs, textColor=white, fontName='Helvetica-Bold', alignment=1)
    t = Table([[Paragraph(text, s)]], colWidths=[W])
    t.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,-1),color),
                            ('TOPPADDING',(0,0),(-1,-1),8),
                            ('BOTTOMPADDING',(0,0),(-1,-1),8)]))
    return t

def sec_hdr(text, color=C_NAVY):
    s = sty('sh', fontSize=11, textColor=white, fontName='Helvetica-Bold')
    t = Table([[Paragraph(text, s)]], colWidths=[W])
    t.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,-1),color),
                            ('TOPPADDING',(0,0),(-1,-1),5),
                            ('BOTTOMPADDING',(0,0),(-1,-1),5)]))
    return t

def kv_table(rows, c1=44*mm):
    ls = sty('kl', fontSize=9, fontName='Helvetica-Bold')
    vs = sty('kv', fontSize=8, fontName='Helvetica', leading=12, wordWrap='LTR')
    data = [[Paragraph(k, ls), Paragraph(v, vs)] for k,v in rows]
    t = Table(data, colWidths=[c1, W-c1])
    t.setStyle(TableStyle([
        ('BACKGROUND',   (0,0),(0,-1), C_LGRAY),
        ('GRID',         (0,0),(-1,-1), 0.3, C_MGRAY),
        ('TOPPADDING',   (0,0),(-1,-1), 4),
        ('BOTTOMPADDING',(0,0),(-1,-1), 4),
        ('LEFTPADDING',  (0,0),(-1,-1), 5),
        ('VALIGN',       (0,0),(-1,-1), 'TOP'),
    ]))
    return t

def wallet_card(label, addr, role_key, subtitle=''):
    color = ROLE_COLORS.get(role_key, C_NAVY)
    title_s  = sty('wt', fontSize=9,  fontName='Helvetica-Bold', textColor=white)
    addr_s   = sty('wa', fontSize=8,  fontName='Courier',        textColor=white, wordWrap='LTR')
    sub_s    = sty('ws', fontSize=7,  fontName='Helvetica',      textColor=HexColor('#DDDDDD'), leading=10)
    link_s   = sty('wl', fontSize=7,  fontName='Courier',        textColor=HexColor('#AADDFF'), leading=10)
    rows = [
        [Paragraph(label, title_s)],
        [Paragraph(addr,  addr_s)],
    ]
    if subtitle:
        rows.append([Paragraph(subtitle, sub_s)])
    rows.append([Paragraph(f'https://tronscan.org/#/address/{addr}', link_s)])
    inner = Table(rows, colWidths=[W-4*mm])
    inner.setStyle(TableStyle([
        ('BACKGROUND',    (0,0),(-1,-1), color),
        ('TOPPADDING',    (0,0),(-1,-1), 4),
        ('BOTTOMPADDING', (0,0),(-1,-1), 4),
        ('LEFTPADDING',   (0,0),(-1,-1), 8),
        ('RIGHTPADDING',  (0,0),(-1,-1), 8),
        ('LINEBELOW',     (0,0),(-1,0),  0.5, HexColor('#FFFFFF44')),
        ('LINEBELOW',     (0,1),(-1,1),  0.5, HexColor('#FFFFFF44')),
    ]))
    return inner

def tx_card(hop_label, from_addr, to_addr, tx_hash, amount_usdt, hop_color):
    """Show one transaction: from → to with hash and link."""
    hs   = sty('th', fontSize=8,  fontName='Helvetica-Bold', textColor=white)
    ms   = sty('tm', fontSize=7,  fontName='Courier',        textColor=white, wordWrap='LTR')
    ls   = sty('tl', fontSize=7,  fontName='Courier',        textColor=HexColor('#AADDFF'), leading=10)
    ss2  = sty('ts', fontSize=7,  fontName='Helvetica',      textColor=HexColor('#DDDDDD'), leading=10)

    rows = [
        [Paragraph(f'{hop_label}  |  ${amount_usdt:,.2f} USDT', hs)],
        [Paragraph(f'From : {from_addr}', ms)],
        [Paragraph(f'To   : {to_addr}',   ms)],
        [Paragraph(f'TX   : {tx_hash}',   ms)],
        [Paragraph(f'Link : https://tronscan.org/#/transaction/{tx_hash}', ls)],
    ]
    inner = Table(rows, colWidths=[W-4*mm])
    inner.setStyle(TableStyle([
        ('BACKGROUND',    (0,0),(-1,-1), hop_color),
        ('TOPPADDING',    (0,0),(-1,-1), 3),
        ('BOTTOMPADDING', (0,0),(-1,-1), 3),
        ('LEFTPADDING',   (0,0),(-1,-1), 8),
        ('RIGHTPADDING',  (0,0),(-1,-1), 8),
        ('LINEBELOW',     (0,0),(-1,0),  0.5, HexColor('#FFFFFF44')),
    ]))
    return inner

def arrow():
    s = sty('ar', fontSize=14, fontName='Helvetica-Bold',
            textColor=C_MGRAY, alignment=1, spaceBefore=1, spaceAfter=1)
    return Paragraph('▼', s)

# ══════════════════════════════════════════════════════════════
# MAKE ONE PDF
# ══════════════════════════════════════════════════════════════
def make_pdf(master_addr, txs, num, total):
    date_str = datetime.now().strftime("%Y-%m-%d %H:%M UTC")
    pdf_path = os.path.join(OUTPUT_DIR,
                            f"Master_{num:02d}_{master_addr}.pdf")

    doc = SimpleDocTemplate(pdf_path, pagesize=A4,
                            leftMargin=12*mm, rightMargin=12*mm,
                            topMargin=12*mm,  bottomMargin=12*mm)

    body_s = sty('bo', fontSize=9,  fontName='Helvetica', leading=13)
    mono_s = sty('mo', fontSize=7,  fontName='Courier',   leading=11, wordWrap='LTR')
    lbl_s  = sty('lb', fontSize=9,  fontName='Helvetica-Bold')
    sm_s   = sty('sm', fontSize=7,  fontName='Helvetica', textColor=HexColor('#555'), leading=10)
    red_s  = sty('re', fontSize=9,  fontName='Helvetica-Bold', textColor=C_RED)
    h2_s   = sty('h2', fontSize=10, textColor=C_NAVY, fontName='Helvetica-Bold',
                 spaceBefore=5, spaceAfter=3)

    total_usdt  = sum(get_amt(t) for t in txs)
    exchanges   = sorted(set(t['exchange'] for t in txs))
    story       = []

    # ── Cover page ────────────────────────────────────────────
    story.append(Spacer(1, 8*mm))
    story.append(banner('FRAUD FUND FLOW REPORT', C_RED, 15))
    story.append(Spacer(1, 2*mm))
    story.append(banner(
        f'Master Wallet {num} of {total}  |  BG Wealth Sharing / DSJEX',
        C_NAVY, 10))
    story.append(Spacer(1, 6*mm))

    story.append(kv_table([
        ('Report Date',   date_str),
        ('Master Wallet', master_addr),
        ('TronScan',      f'https://tronscan.org/#/address/{master_addr}'),
        ('Txs Found',     str(len(txs))),
        ('Total USDT',    f'${total_usdt:,.2f} confirmed'),
        ('Exchanges Hit', ', '.join(exchanges)),
    ]))
    story.append(Spacer(1, 5*mm))
    story.append(HRFlowable(width='100%', thickness=1, color=C_RED))
    story.append(Spacer(1, 3*mm))
    story.append(Paragraph(
        'This report documents the complete fund flow chain from this master '
        'collection wallet to exchange hot wallets. Each path below shows every '
        'intermediate hop with full transaction hash and TronScan link.',
        body_s))

    # ── Group txs by unique path signature ────────────────────
    # Each unique path = one chain section in the PDF
    path_groups = defaultdict(list)
    for t in txs:
        labeled = get_labeled_path(t, master_addr)
        sig = tuple(h['addr'] for h in labeled)
        t['_labeled'] = labeled  # cache it
        path_groups[sig].append(t)

    # Sort groups by total amount descending
    sorted_groups = sorted(
        path_groups.items(),
        key=lambda x: sum(get_amt(t) for t in x[1]),
        reverse=True
    )

    # ── One section per unique path ───────────────────────────
    for g_idx, (sig, group_txs) in enumerate(sorted_groups, 1):
        story.append(PageBreak())

        sample   = group_txs[0]
        labeled  = sample['_labeled']
        pattern  = sample.get('pattern', ' → '.join(h['label'] for h in labeled))
        g_total  = sum(get_amt(t) for t in group_txs)
        ex_label = labeled[-1]['label'] if labeled else ''

        story.append(sec_hdr(
            f'PATH {g_idx}: {pattern}  |  {len(group_txs)} txs  |  ${g_total:,.2f} USDT',
            C_NAVY))
        story.append(Spacer(1, 4*mm))

        # ── Visual chain: wallet boxes ─────────────────────────
        story.append(Paragraph('Fund Flow Chain:', h2_s))
        story.append(Spacer(1, 2*mm))

        inter_n = 0
        for hop_i, hop in enumerate(labeled):
            role = hop['role']
            addr = hop['addr']

            # Determine display role key for color
            if role == 'intermediate':
                inter_n += 1
                role_key = f'intermediate_{inter_n}'
            else:
                role_key  = role
                inter_n_reset = 0

            # Subtitle for card
            if role == 'origin_master':
                subtitle = f'Master collection wallet | {len(txs)} total txs from this wallet'
            elif role == 'consolidation_master':
                subtitle = 'Consolidation — aggregates from multiple masters'
            elif role == 'intermediate':
                subtitle = f'Intermediate hop {inter_n} — mixer wallet'
            else:
                ex, hot = EXCHANGE_WALLETS.get(addr, ('Exchange',''))
                subtitle = f'{len(group_txs)} txs | ${g_total:,.2f} USDT | KYC identifies account owner'

            story.append(wallet_card(hop['label'], addr, role_key, subtitle))

            # Arrow between hops (not after last)
            if hop_i < len(labeled) - 1:
                story.append(arrow())

        story.append(Spacer(1, 6*mm))
        story.append(HRFlowable(width='100%', thickness=0.5, color=C_MGRAY))
        story.append(Spacer(1, 4*mm))

        # ── Transaction details ────────────────────────────────
        story.append(Paragraph(
            f'All {len(group_txs)} transactions — with full details:', h2_s))
        story.append(Spacer(1, 2*mm))

        for tx_i, tx in enumerate(group_txs, 1):
            tx_labeled = tx['_labeled']
            amt        = get_amt(tx)

            story.append(Paragraph(
                f'<b>Transaction {tx_i} of {len(group_txs)}</b>  —  '
                f'${amt:,.2f} USDT  —  Path: {pattern}',
                sty('th2', fontSize=9, fontName='Helvetica-Bold',
                    textColor=C_NAVY, spaceBefore=4, spaceAfter=2)))

            # Show each hop in this transaction
            # We know the final hop (from_address → exchange_addr)
            # For intermediate hops we show what we know from the path
            inter_count = 0
            for h_i, hop in enumerate(tx_labeled):
                if h_i == 0:
                    continue  # skip master (shown in wallet card above)

                role = hop['role']
                addr = hop['addr']
                prev_addr = tx_labeled[h_i - 1]['addr']

                if role == 'intermediate':
                    inter_count += 1
                    role_key = f'intermediate_{inter_count}'
                else:
                    role_key = role

                hop_color = ROLE_COLORS.get(role_key, C_NAVY)

                # For the FINAL hop (to exchange), we have the actual tx_hash
                if role == 'exchange':
                    hop_label = f'Hop {h_i}: {tx_labeled[h_i-1]["label"]} → {hop["label"]}'
                    story.append(tx_card(
                        hop_label,
                        tx['from_address'],
                        addr,
                        tx['tx_hash'],
                        amt,
                        hop_color
                    ))
                else:
                    # Intermediate hops — show address and link, tx_hash unknown
                    # (tracer only records the final tx_hash)
                    story.append(tx_card(
                        f'Hop {h_i}: {tx_labeled[h_i-1]["label"]} → {hop["label"]}',
                        prev_addr,
                        addr,
                        '(see TronScan for individual hop tx)',
                        0,
                        hop_color
                    ))

                if h_i < len(tx_labeled) - 1:
                    story.append(arrow())

            story.append(Spacer(1, 3*mm))

            # Quick reference row
            ref_data = [[
                Paragraph('<b>TX Hash:</b>', lbl_s),
                Paragraph(tx['tx_hash'], mono_s),
            ],[
                Paragraph('<b>TronScan:</b>', lbl_s),
                Paragraph(f'https://tronscan.org/#/transaction/{tx["tx_hash"]}', mono_s),
            ]]
            ref_tbl = Table(ref_data, colWidths=[22*mm, W-22*mm])
            ref_tbl.setStyle(TableStyle([
                ('BACKGROUND', (0,0),(0,-1), C_LGRAY),
                ('GRID',       (0,0),(-1,-1), 0.3, C_MGRAY),
                ('TOPPADDING', (0,0),(-1,-1), 3),
                ('BOTTOMPADDING',(0,0),(-1,-1), 3),
                ('LEFTPADDING',(0,0),(-1,-1), 5),
                ('VALIGN',    (0,0),(-1,-1), 'TOP'),
            ]))
            story.append(ref_tbl)
            story.append(Spacer(1, 4*mm))
            story.append(HRFlowable(width='100%', thickness=0.3, color=C_MGRAY))
            story.append(Spacer(1, 2*mm))

    # ── Summary table (last page) ─────────────────────────────
    story.append(PageBreak())
    story.append(sec_hdr('SUMMARY — ALL TRANSACTIONS FROM THIS MASTER', C_RED))
    story.append(Spacer(1, 4*mm))

    sum_hdr = [[
        Paragraph('<b>#</b>',          lbl_s),
        Paragraph('<b>TX Hash</b>',    lbl_s),
        Paragraph('<b>USDT</b>',       lbl_s),
        Paragraph('<b>Exchange</b>',   lbl_s),
        Paragraph('<b>Path</b>',       lbl_s),
    ]]
    for i, tx in enumerate(txs, 1):
        sum_hdr.append([
            Paragraph(str(i), body_s),
            Paragraph(tx['tx_hash'], mono_s),
            Paragraph(f"${get_amt(tx):,.2f}", body_s),
            Paragraph(f"{tx['exchange']} {tx['hot_wallet']}", body_s),
            Paragraph(tx.get('pattern','—'), sm_s),
        ])
    total_row = [
        Paragraph('', body_s),
        Paragraph('<b>TOTAL</b>', red_s),
        Paragraph(f'<b>${total_usdt:,.2f}</b>', red_s),
        Paragraph('', body_s),
        Paragraph('', body_s),
    ]
    sum_hdr.append(total_row)

    st = Table(sum_hdr, colWidths=[8*mm, 72*mm, 22*mm, 26*mm, W-128*mm])
    st.setStyle(TableStyle([
        ('BACKGROUND',     (0,0),(-1,0),  C_RED),
        ('TEXTCOLOR',      (0,0),(-1,0),  white),
        ('FONTSIZE',       (0,0),(-1,-1), 7),
        ('GRID',           (0,0),(-1,-1), 0.3, C_MGRAY),
        ('TOPPADDING',     (0,0),(-1,-1), 3),
        ('BOTTOMPADDING',  (0,0),(-1,-1), 3),
        ('LEFTPADDING',    (0,0),(-1,-1), 4),
        ('ROWBACKGROUNDS', (0,1),(-1,-2), [white, C_LGRAY]),
        ('BACKGROUND',     (0,-1),(-1,-1), HexColor('#FFE0E0')),
        ('FONTNAME',       (0,-1),(-1,-1), 'Helvetica-Bold'),
        ('TEXTCOLOR',      (0,-1),(-1,-1), C_RED),
        ('VALIGN',         (0,0),(-1,-1), 'MIDDLE'),
        ('FONTNAME',       (1,1),(1,-2),   'Courier'),
    ]))
    story.append(st)
    story.append(Spacer(1, 4*mm))
    story.append(HRFlowable(width='100%', thickness=0.5, color=C_MGRAY))
    story.append(Spacer(1, 3*mm))
    story.append(Paragraph(
        f'All data verifiable on TronScan. '
        f'Report generated: {date_str}. '
        f'BG Wealth Sharing / DSJEX fraud investigation.',
        sm_s))

    doc.build(story)
    return pdf_path

# ══════════════════════════════════════════════════════════════
# MAIN
# ══════════════════════════════════════════════════════════════
def main():
    # Group by master wallet
    by_master = defaultdict(list)
    for tx in all_results:
        by_master[tx['master_wallet']].append(tx)

    # Sort by total amount descending
    masters = sorted(by_master.keys(),
                     key=lambda m: sum(get_amt(t) for t in by_master[m]),
                     reverse=True)

    print(f"\n{'='*60}")
    print(f"  GENERATING {len(masters)} MASTER WALLET REPORTS")
    print(f"  Output: {OUTPUT_DIR}/")
    print(f"{'='*60}\n")

    generated = []
    for i, master in enumerate(masters, 1):
        txs   = by_master[master]
        total = sum(get_amt(t) for t in txs)
        exs   = sorted(set(t['exchange'] for t in txs))
        print(f"  [{i}/{len(masters)}] {master}")
        print(f"           {len(txs)} txs  ${total:,.2f} USDT  → {', '.join(exs)}")
        pdf = make_pdf(master, txs, i, len(masters))
        print(f"  ✅ {pdf}\n")
        generated.append(pdf)

    print(f"{'='*60}")
    print(f"  DONE — {len(generated)} PDFs generated in {OUTPUT_DIR}/")
    print(f"{'='*60}\n")

if __name__ == "__main__":
    main()
