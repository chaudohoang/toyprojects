#!/usr/bin/env python3
"""
generate_exchange_reports.py
Reads all_exchanges_evidence.json and generates one PDF per exchange wallet.
Each PDF shows: Master Wallet → Intermediate Wallet → Exchange Wallet chain.

Usage:
    python generate_exchange_reports.py

Requirements:
    pip install reportlab
"""

import json
import os
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
    print("Run: pip install reportlab")
    exit(1)

# ── Load evidence ─────────────────────────────────────────────
INPUT_FILE = "all_exchanges_evidence.json"
OUTPUT_DIR = "exchange_reports"

if not os.path.exists(INPUT_FILE):
    print(f"! {INPUT_FILE} not found — run trace_all_exchanges.py first")
    exit(1)

os.makedirs(OUTPUT_DIR, exist_ok=True)
all_results = json.load(open(INPUT_FILE))
print(f"Loaded {len(all_results)} transactions from {INPUT_FILE}")

# ── Exchange wallet info ──────────────────────────────────────
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

COMPLIANCE_EMAIL = {
    "Binance":  "support@binance.com",
    "Bybit":    "support@bybit.com",
    "Gate.io":  "support@gate.io",
    "OKX":      "support@okx.com",
    "Bitget":   "support@bitget.com",
    "MEXC":     "support@mexc.com",
    "KuCoin":   "support@kucoin.com",
    "HTX":      "support@htx.com",
    "Bitfinex": "support@bitfinex.com",
}

# ── Colors ────────────────────────────────────────────────────
RED   = HexColor('#C00000')
NAVY  = HexColor('#1F3864')
LGRAY = HexColor('#F5F5F5')
MGRAY = HexColor('#DDDDDD')
GREEN = HexColor('#1E6B2E')
ORANGE= HexColor('#E65C00')

# ── Style helpers ─────────────────────────────────────────────
ss = getSampleStyleSheet()
W  = 186*mm

def sty(name, **kw):
    return ParagraphStyle(name + str(id(kw)), parent=ss['Normal'], **kw)

def banner(text, color=NAVY, fs=14):
    s = sty('BS', fontSize=fs, textColor=white, fontName='Helvetica-Bold', alignment=1)
    t = Table([[Paragraph(text, s)]], colWidths=[W])
    t.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,-1),color),
                            ('TOPPADDING',(0,0),(-1,-1),8),
                            ('BOTTOMPADDING',(0,0),(-1,-1),8)]))
    return t

def sec(text, color=NAVY):
    s = sty('SH', fontSize=11, textColor=white, fontName='Helvetica-Bold')
    t = Table([[Paragraph(text, s)]], colWidths=[W])
    t.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,-1),color),
                            ('TOPPADDING',(0,0),(-1,-1),5),
                            ('BOTTOMPADDING',(0,0),(-1,-1),5)]))
    return t

def kv(rows, c1=42*mm):
    lbl_s = sty('L', fontSize=9,  fontName='Helvetica-Bold')
    val_s = sty('V', fontSize=8,  fontName='Helvetica', leading=12, wordWrap='LTR')
    data  = [[Paragraph(k, lbl_s), Paragraph(v, val_s)] for k,v in rows]
    t = Table(data, colWidths=[c1, W-c1])
    t.setStyle(TableStyle([
        ('BACKGROUND',    (0,0),(0,-1), LGRAY),
        ('GRID',          (0,0),(-1,-1), 0.3, MGRAY),
        ('TOPPADDING',    (0,0),(-1,-1), 4),
        ('BOTTOMPADDING', (0,0),(-1,-1), 4),
        ('LEFTPADDING',   (0,0),(-1,-1), 5),
        ('VALIGN',        (0,0),(-1,-1), 'TOP'),
    ]))
    return t

def wallet_box(label, address, role, color=NAVY):
    """Visual wallet box showing address prominently"""
    lbl_s  = sty('WL', fontSize=9,  fontName='Helvetica-Bold', textColor=white)
    addr_s = sty('WA', fontSize=9,  fontName='Courier',        textColor=white, wordWrap='LTR')
    role_s = sty('WR', fontSize=8,  fontName='Helvetica',      textColor=HexColor('#DDDDDD'))
    inner  = Table([
        [Paragraph(label, lbl_s)],
        [Paragraph(address, addr_s)],
        [Paragraph(role, role_s)],
    ], colWidths=[W-20*mm])
    inner.setStyle(TableStyle([
        ('BACKGROUND', (0,0),(-1,-1), color),
        ('TOPPADDING', (0,0),(-1,-1), 5),
        ('BOTTOMPADDING',(0,0),(-1,-1),5),
        ('LEFTPADDING', (0,0),(-1,-1), 8),
        ('LINEABOVE',   (0,1),(-1,1), 0.5, HexColor('#FFFFFF55')),
    ]))
    return inner

def arrow():
    s = sty('AR', fontSize=14, fontName='Helvetica-Bold',
            textColor=RED, alignment=1, spaceBefore=2, spaceAfter=2)
    return Paragraph('▼', s)

# ══════════════════════════════════════════════════════════════
# Generate one PDF per exchange wallet
# ══════════════════════════════════════════════════════════════
def make_report(exchange_addr, txs):
    ex, hot = EXCHANGE_WALLETS[exchange_addr]
    date_str = datetime.now().strftime("%Y-%m-%d %H:%M UTC")
    safe_name = f"{ex.replace('.','').replace('/','_')}_{hot.replace(' ','_')}"
    pdf_path  = os.path.join(OUTPUT_DIR, f"Report_{safe_name}.pdf")

    doc = SimpleDocTemplate(pdf_path, pagesize=A4,
                            leftMargin=12*mm, rightMargin=12*mm,
                            topMargin=12*mm,  bottomMargin=12*mm)

    body_s  = sty('B',  fontSize=9,  fontName='Helvetica', spaceAfter=3, leading=13)
    mono_s  = sty('M',  fontSize=7,  fontName='Courier',   leading=11,   wordWrap='LTR')
    lbl_s   = sty('L',  fontSize=9,  fontName='Helvetica-Bold')
    sm_s    = sty('S',  fontSize=7,  fontName='Helvetica', textColor=HexColor('#555'), leading=10)
    red_s   = sty('R',  fontSize=9,  fontName='Helvetica-Bold', textColor=RED)
    h2_s    = sty('H2', fontSize=10, textColor=NAVY, fontName='Helvetica-Bold',
                  spaceBefore=5, spaceAfter=3)

    # ── Aggregate stats ───────────────────────────────────────
    total_usdt    = sum(t['amount'] for t in txs)
    masters_used  = list(dict.fromkeys(t['master_wallet'] for t in txs))
    intermediates = list(dict.fromkeys(t['from_address']  for t in txs))

    story = []

    # ── PAGE 1: Cover ─────────────────────────────────────────
    story.append(Spacer(1, 8*mm))
    story.append(banner(f'FRAUD EVIDENCE REPORT — {ex.upper()}', RED, 14))
    story.append(Spacer(1, 2*mm))
    story.append(banner(f'{ex} {hot}  |  BG Wealth Sharing / DSJEX Pig Butchering Scam', NAVY, 10))
    story.append(Spacer(1, 6*mm))

    story.append(kv([
        ('Report Date',       date_str),
        ('Target Exchange',   f'{ex}'),
        ('Target Wallet',     f'{hot}'),
        ('Wallet Address',    exchange_addr),
        ('Compliance Email',  COMPLIANCE_EMAIL.get(ex, f'compliance@{ex.lower()}.com')),
        ('Transactions Found',str(len(txs))),
        ('Total USDT',        f'${total_usdt:,.2f} confirmed'),
        ('Master Wallets',    str(len(masters_used))),
        ('Scam Operation',    'BG Wealth Sharing / DSJEX — Fake crypto exchange (Pig Butchering)'),
        ('Status',            'Shut down May 2026 — demanding unlock fees from victims'),
        ('Full Report',       'https://medium.com/@hades280389/i-reverse-engineered-bg-wealth-sharing-inside-the-infrastructure-of-a-billion-dollar-pig-60b7de582483'),
    ]))
    story.append(Spacer(1, 5*mm))
    story.append(HRFlowable(width='100%', thickness=1, color=RED))
    story.append(Spacer(1, 3*mm))
    story.append(Paragraph(
        f'<b>Purpose:</b> To provide {ex} compliance with transaction evidence proving that '
        f'fraud proceeds from the BG Wealth Sharing pig butchering scam were deposited into '
        f'{ex} via the {hot} hot wallet. We request immediate account identification, '
        f'freezing, and cooperation with law enforcement.',
        body_s))

    # ── PAGE 2: Transaction Chain Visual ─────────────────────
    story.append(PageBreak())
    story.append(sec('1. TRANSACTION CHAIN', NAVY))
    story.append(Spacer(1, 5*mm))

    # Master wallets box
    story.append(wallet_box(
        '🔴  MASTER COLLECTION WALLETS  (victim funds aggregated here)',
        '\n'.join(masters_used),
        f'Receives deposits from ~100,000+ victims | 101,902+ incoming transactions | ~$1,000,000,000+ USDT total',
        RED
    ))
    story.append(arrow())

    # Intermediate wallets box
    story.append(wallet_box(
        '🟠  INTERMEDIATE MIXER WALLETS  (distributes to exchange)',
        '\n'.join(intermediates),
        f'Receives from master wallets in batches | Sends directly to {ex} {hot}',
        ORANGE
    ))
    story.append(arrow())

    # Exchange wallet box
    story.append(wallet_box(
        f'🟢  {ex.upper()} — {hot}  (final cashout destination)',
        exchange_addr,
        f'Receives fraud proceeds | {len(txs)} confirmed transactions | ${total_usdt:,.2f} USDT confirmed',
        GREEN
    ))

    story.append(Spacer(1, 5*mm))
    story.append(Paragraph(
        f'The internal {ex} account that deposited to {hot} using wallet '
        f'{exchange_addr} is the account we are requesting be identified and frozen.',
        body_s))

    # ── PAGE 3: Master wallet details ────────────────────────
    story.append(PageBreak())
    story.append(sec('2. MASTER COLLECTION WALLET DETAILS', RED))
    story.append(Spacer(1, 3*mm))
    story.append(Paragraph(
        'These wallets aggregate victim deposits before distributing to intermediate wallets:',
        body_s))
    story.append(Spacer(1, 3*mm))

    mst_data = [[Paragraph('<b>#</b>', lbl_s),
                 Paragraph('<b>Master Wallet Address</b>', lbl_s),
                 Paragraph('<b>Txs to This Exchange</b>', lbl_s),
                 Paragraph('<b>TronScan</b>', lbl_s)]]

    master_tx_count = defaultdict(int)
    for t in txs:
        master_tx_count[t['master_wallet']] += 1

    for i, master in enumerate(masters_used, 1):
        count = master_tx_count[master]
        mst_data.append([
            Paragraph(str(i), body_s),
            Paragraph(master, mono_s),
            Paragraph(str(count), body_s),
            Paragraph(f'tronscan.org/#/address/{master[:20]}...', sm_s),
        ])

    mt = Table(mst_data, colWidths=[8*mm, 90*mm, 30*mm, W-128*mm])
    mt.setStyle(TableStyle([
        ('BACKGROUND',     (0,0),(-1,0),  RED),
        ('TEXTCOLOR',      (0,0),(-1,0),  white),
        ('FONTSIZE',       (0,0),(-1,-1), 8),
        ('GRID',           (0,0),(-1,-1), 0.3, MGRAY),
        ('TOPPADDING',     (0,0),(-1,-1), 4),
        ('BOTTOMPADDING',  (0,0),(-1,-1), 4),
        ('LEFTPADDING',    (0,0),(-1,-1), 4),
        ('ROWBACKGROUNDS', (0,1),(-1,-1), [white, LGRAY]),
        ('FONTNAME',       (1,1),(1,-1),  'Courier'),
    ]))
    story.append(mt)

    story.append(Spacer(1, 5*mm))
    story.append(sec('3. INTERMEDIATE WALLET DETAILS', ORANGE))
    story.append(Spacer(1, 3*mm))
    story.append(Paragraph(
        f'These wallets receive from master wallets and send directly to {ex} {hot}:',
        body_s))
    story.append(Spacer(1, 3*mm))

    int_data = [[Paragraph('<b>#</b>', lbl_s),
                 Paragraph('<b>Intermediate Wallet Address</b>', lbl_s),
                 Paragraph('<b>Txs to Exchange</b>', lbl_s),
                 Paragraph('<b>Total USDT</b>', lbl_s)]]

    int_stats = defaultdict(lambda: {"count": 0, "total": 0})
    for t in txs:
        int_stats[t['from_address']]["count"] += 1
        int_stats[t['from_address']]["total"] += t['amount']

    for i, (addr, stats) in enumerate(sorted(int_stats.items(),
                                              key=lambda x: x[1]['total'], reverse=True), 1):
        int_data.append([
            Paragraph(str(i), body_s),
            Paragraph(addr, mono_s),
            Paragraph(str(stats['count']), body_s),
            Paragraph(f"${stats['total']:,.2f}", body_s),
        ])

    it = Table(int_data, colWidths=[8*mm, 90*mm, 30*mm, W-128*mm])
    it.setStyle(TableStyle([
        ('BACKGROUND',     (0,0),(-1,0),  ORANGE),
        ('TEXTCOLOR',      (0,0),(-1,0),  white),
        ('FONTSIZE',       (0,0),(-1,-1), 8),
        ('GRID',           (0,0),(-1,-1), 0.3, MGRAY),
        ('TOPPADDING',     (0,0),(-1,-1), 4),
        ('BOTTOMPADDING',  (0,0),(-1,-1), 4),
        ('LEFTPADDING',    (0,0),(-1,-1), 4),
        ('ROWBACKGROUNDS', (0,1),(-1,-1), [white, LGRAY]),
        ('FONTNAME',       (1,1),(1,-1),  'Courier'),
    ]))
    story.append(it)

    # ── PAGE 4: All transactions ──────────────────────────────
    story.append(PageBreak())
    story.append(sec(f'4. ALL {len(txs)} CONFIRMED TRANSACTIONS TO {ex.upper()} {hot}', GREEN))
    story.append(Spacer(1, 3*mm))
    story.append(Paragraph(
        f'Each transaction goes FROM an intermediate wallet TO {ex} {hot} '
        f'({exchange_addr}). All verifiable on TronScan.',
        body_s))
    story.append(Spacer(1, 3*mm))

    tx_data = [[Paragraph('<b>#</b>', lbl_s),
                Paragraph('<b>Transaction Hash (64 chars)</b>', lbl_s),
                Paragraph('<b>Amount</b>', lbl_s),
                Paragraph('<b>From Intermediate</b>', lbl_s)]]
    for i, tx in enumerate(txs, 1):
        tx_data.append([
            Paragraph(str(i), body_s),
            Paragraph(tx['tx_hash'], mono_s),
            Paragraph(f"${tx['amount']:,.2f}", body_s),
            Paragraph(tx['from_address'], mono_s),
        ])
    tx_data.append([
        Paragraph('', body_s),
        Paragraph('<b>TOTAL</b>', red_s),
        Paragraph(f'<b>${total_usdt:,.2f}</b>', red_s),
        Paragraph('', body_s),
    ])

    tt = Table(tx_data, colWidths=[8*mm, 80*mm, 24*mm, W-112*mm])
    tt.setStyle(TableStyle([
        ('BACKGROUND',     (0,0), (-1,0),  GREEN),
        ('TEXTCOLOR',      (0,0), (-1,0),  white),
        ('FONTSIZE',       (0,0), (-1,-1), 7),
        ('GRID',           (0,0), (-1,-1), 0.3, MGRAY),
        ('TOPPADDING',     (0,0), (-1,-1), 3),
        ('BOTTOMPADDING',  (0,0), (-1,-1), 3),
        ('LEFTPADDING',    (0,0), (-1,-1), 4),
        ('ROWBACKGROUNDS', (0,1), (-1,-2), [white, LGRAY]),
        ('BACKGROUND',     (0,-1),(-1,-1), HexColor('#E8FFE8')),
        ('FONTNAME',       (0,-1),(-1,-1), 'Helvetica-Bold'),
        ('TEXTCOLOR',      (0,-1),(-1,-1), GREEN),
        ('VALIGN',         (0,0), (-1,-1), 'MIDDLE'),
    ]))
    story.append(tt)

    # ── PAGE 5: Full hashes + TronScan links ──────────────────
    story.append(PageBreak())
    story.append(sec('5. FULL TRANSACTION HASHES + TRONSCAN LINKS', NAVY))
    story.append(Spacer(1, 3*mm))
    story.append(Paragraph(
        f'Complete 64-character hashes for {ex} internal transaction lookup:',
        body_s))
    story.append(Spacer(1, 3*mm))

    for i, tx in enumerate(txs, 1):
        story.append(Paragraph(
            f'<b>[{i}]</b>  ${tx["amount"]:,.2f} USDT  |  '
            f'From: {tx["from_address"]}  |  '
            f'Master: {tx["master_wallet"][:20]}...',
            sm_s))
        story.append(Paragraph(f'TX:   {tx["tx_hash"]}', mono_s))
        story.append(Paragraph(
            f'Link: https://tronscan.org/#/transaction/{tx["tx_hash"]}', mono_s))
        story.append(Spacer(1, 2*mm))

    # ── PAGE 6: Requests ──────────────────────────────────────
    story.append(PageBreak())
    story.append(sec(f'6. FORMAL REQUESTS TO {ex.upper()} COMPLIANCE', RED))
    story.append(Spacer(1, 3*mm))

    story.append(kv([
        ('Request 1', f'Identify the internal {ex} account(s) that deposited funds traceable to the master wallets listed in Section 2'),
        ('Request 2', 'Freeze all identified accounts and associated accounts immediately'),
        ('Request 3', 'Preserve all KYC records, IP address logs, device fingerprints, and full transaction history'),
        ('Request 4', 'Share account identity with law enforcement: Vietnam NCSC, FBI IC3, Interpol'),
        ('Request 5', 'Provide a case reference number for follow-up'),
        ('URGENCY',   'Platform shut down May 2026 and demanding unlock fees from victims — operators may be preparing to disappear'),
    ]))

    story.append(Spacer(1, 5*mm))
    story.append(sec('7. SUPPORTING INFRASTRUCTURE EVIDENCE', NAVY))
    story.append(Spacer(1, 3*mm))
    story.append(kv([
        ('Scam Platform',    'DSJEX / BG Wealth Sharing — fake crypto exchange'),
        ('Backend APIs',     'api.ddjea.com  |  api.ddjeb.com  |  api.dsjhout.com'),
        ('File Storage',     'Alibaba Cloud OSS oss-cn-hangzhou (victim KYC documents)'),
        ('Alibaba Report',   'Case #20260502710120867026 filed with Alibaba Cloud'),
        ('Kit Version',      'P2.9.3 in all HTTP request headers'),
        ('API Endpoints',    '141 endpoints mapped from JS deobfuscation (18 files, 68,098 strings)'),
        ('Also Reported To', ', '.join(e for e in COMPLIANCE_EMAIL if e != ex)),
        ('Full Report',      'https://medium.com/@hades280389/i-reverse-engineered-bg-wealth-sharing-inside-the-infrastructure-of-a-billion-dollar-pig-60b7de582483'),
    ]))

    story.append(Spacer(1, 5*mm))
    story.append(HRFlowable(width='100%', thickness=0.5, color=MGRAY))
    story.append(Spacer(1, 3*mm))
    story.append(Paragraph(
        f'All blockchain data is publicly verifiable on TronScan (tronscan.org). '
        f'Report generated: {date_str}. '
        f'Prepared by independent security researcher and fraud victim.',
        sm_s))

    doc.build(story)
    print(f"  ✅ Saved: {pdf_path}")
    return pdf_path

# ── Main ──────────────────────────────────────────────────────
def main():
    # Group transactions by exchange wallet
    by_exchange = defaultdict(list)
    for tx in all_results:
        by_exchange[tx['exchange_addr']].append(tx)

    print(f"\n{'='*60}")
    print(f"  GENERATING {len(by_exchange)} INDIVIDUAL PDF REPORTS")
    print(f"  Output folder: {OUTPUT_DIR}/")
    print(f"{'='*60}\n")

    generated = []
    skipped   = []

    for addr, (ex, hot) in EXCHANGE_WALLETS.items():
        txs = by_exchange.get(addr, [])
        if not txs:
            print(f"  — Skipping {ex} {hot} (no transactions found)")
            skipped.append(f"{ex} {hot}")
            continue
        print(f"  Generating: {ex} {hot} ({len(txs)} txs, ${sum(t['amount'] for t in txs):,.2f} USDT)")
        pdf = make_report(addr, txs)
        generated.append((f"{ex} {hot}", pdf))

    print(f"\n{'='*60}")
    print(f"  DONE — {len(generated)} PDFs generated")
    print(f"{'='*60}")
    for name, path in generated:
        print(f"  📄 {name:20} → {path}")
    if skipped:
        print(f"\n  Not generated (no transactions found):")
        for s in skipped:
            print(f"  — {s}")
    print()

if __name__ == "__main__":
    main()
