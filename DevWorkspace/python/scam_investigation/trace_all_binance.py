#!/usr/bin/env python3
"""
trace_all_binance.py
Traces fund flow from master wallet to ALL 6 Binance hot wallets,
then automatically generates a full PDF evidence report.

Usage:
    python trace_all_binance.py

Requirements:
    pip install reportlab
"""

import urllib.request
import json
import time
import sys
from datetime import datetime

# ── Try importing reportlab ───────────────────────────────────
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
    print("  ! reportlab not installed — run: pip install reportlab")
    print("  ! Will save JSON only, no PDF")

# ── Config ────────────────────────────────────────────────────
MASTER_WALLET = "TWF1bcuNLXtwQCRxb6Qn6uKqWCuXRatiei"

BINANCE_HOT_WALLETS = {
    "TDqSquXBgUCLYvYC4XZgrprLK589dkhSCf": "Binance Hot 7",
    "TJDENsfBJs4RFETt1X1W8wMDc8M5XnJhCe": "Binance Hot 6",
    "TAzsQ9Gx8eqFNFSKbeXrbi45CuVPHzA8wr": "Binance Hot 5",
    "TJqwA7SoZnERE4zW5uDEiPkbz4B66h9TFj": "Binance Hot 11",
    "TNXoiAJ3dct8Fjg4M9fkLFh9S2v9TXc32G": "Binance Hot 4",
    "TYASr5UV6HEcXatwdFQfmLVUqQQQMUxHLS": "Binance Hot 3",
}

TRONSCAN = "https://apilist.tronscanapi.com/api"
USDT     = "TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t"
HEADERS  = {"User-Agent": "Mozilla/5.0", "Accept": "application/json"}
DELAY    = 0.8
MAX_HOPS = 3
MAX_PER_WALLET = 25  # stop after finding this many txs per hot wallet

# ── Network ───────────────────────────────────────────────────
def fetch(url):
    req = urllib.request.Request(url, headers=HEADERS)
    try:
        with urllib.request.urlopen(req, timeout=15) as r:
            return json.loads(r.read().decode("utf-8"))
    except Exception as e:
        print(f"    ! {e}")
        return None

def get_transfers(address, direction="out", limit=30):
    url = (f"{TRONSCAN}/token_trc20/transfers"
           f"?address={address}&direction={direction}"
           f"&limit={limit}&trc20Id={USDT}")
    return fetch(url)

def fmt_amt(raw):
    try: return float(raw) / 1_000_000
    except: return 0.0

def short(addr):
    return addr[:10] + "..." + addr[-6:]

# ── Tracer ────────────────────────────────────────────────────
def trace_all():
    all_found = []   # all transactions found across all hot wallets
    visited   = set()
    queue     = [(MASTER_WALLET, [], 0)]

    print(f"\n{'='*65}")
    print(f"  TRACING ALL BINANCE HOT WALLETS")
    print(f"  Start : {MASTER_WALLET}")
    print(f"  Targets: {len(BINANCE_HOT_WALLETS)} Binance hot wallets")
    print(f"  Max depth: {MAX_HOPS} hops")
    print(f"{'='*65}\n")

    while queue:
        current, path, depth = queue.pop(0)
        if depth > MAX_HOPS or current in visited:
            continue

        # Check if ALL wallets already have enough — stop entirely
        all_done = all(
            sum(1 for t in all_found if t['binance_wallet'] == addr) >= MAX_PER_WALLET
            for addr in BINANCE_HOT_WALLETS
        )
        if all_done:
            print("\n  ✅ All wallets have enough transactions — stopping scan")
            break

        visited.add(current)

        indent = "  " * depth
        is_master = current == MASTER_WALLET
        is_binance = current in BINANCE_HOT_WALLETS

        if is_binance:
            continue  # already logged as a hit when queued

        label = "Master Wallet" if is_master else short(current)
        print(f"{indent}[depth {depth}] {label}")

        data = get_transfers(current, direction="out", limit=100)
        time.sleep(DELAY)

        if not data:
            continue

        transfers = data.get("token_transfers", [])
        print(f"{indent}  {len(transfers)} outgoing transfers")

        for tx in transfers:
            to_addr = tx.get("to_address", "")
            tx_hash = tx.get("transaction_id", "")
            amount  = fmt_amt(tx.get("quant", 0))
            ts      = tx.get("block_ts", "")

            if not to_addr or not tx_hash:
                continue

            if to_addr in BINANCE_HOT_WALLETS:
                label_name = BINANCE_HOT_WALLETS[to_addr]
                # Count how many we already have for this wallet
                existing = sum(1 for t in all_found if t['binance_wallet'] == to_addr)
                if existing >= MAX_PER_WALLET:
                    continue  # skip — already have enough for this wallet
                print(f"\n{indent}  🎯 HIT → {label_name}")
                print(f"{indent}     TX    : {tx_hash}")
                print(f"{indent}     Amount: ${amount:,.2f} USDT")
                print(f"{indent}     From  : {current}")

                all_found.append({
                    "binance_wallet":  to_addr,
                    "binance_label":   label_name,
                    "tx_hash":         tx_hash,
                    "amount":          amount,
                    "timestamp":       ts,
                    "from_address":    current,
                    "depth":           depth + 1,
                    "path":            [p[0] for p in path] + [current, to_addr],
                    "tronscan":        f"https://tronscan.org/#/transaction/{tx_hash}",
                })
                # Check if we have enough for this wallet
                found_count = sum(1 for t in all_found if t['binance_wallet'] == to_addr)
                if found_count >= MAX_PER_WALLET:
                    print(f"\n  ✅ Reached {MAX_PER_WALLET} txs for {label_name} — moving to next wallet")

            elif to_addr not in visited and depth < MAX_HOPS and amount >= 500:
                queue.append((
                    to_addr,
                    path + [(current, tx_hash, amount)],
                    depth + 1
                ))

    return all_found

# ── PDF Generator ─────────────────────────────────────────────
def make_pdf(all_found, output_path):
    if not REPORTLAB:
        print("  ! Skipping PDF — reportlab not available")
        return

    RED   = HexColor('#C00000')
    NAVY  = HexColor('#1F3864')
    LGRAY = HexColor('#F5F5F5')
    MGRAY = HexColor('#DDDDDD')
    GREEN = HexColor('#1E7E34')

    doc = SimpleDocTemplate(output_path, pagesize=A4,
                            leftMargin=15*mm, rightMargin=15*mm,
                            topMargin=15*mm,  bottomMargin=15*mm)

    ss = getSampleStyleSheet()
    def sty(name, **kw):
        return ParagraphStyle(name, parent=ss['Normal'], **kw)

    title_s = sty('T',  fontSize=15, textColor=white, fontName='Helvetica-Bold', alignment=1)
    h1_s    = sty('H1', fontSize=12, textColor=white, fontName='Helvetica-Bold')
    h2_s    = sty('H2', fontSize=10, textColor=NAVY,  fontName='Helvetica-Bold', spaceBefore=4, spaceAfter=3)
    body_s  = sty('B',  fontSize=9,  fontName='Helvetica', spaceAfter=3, leading=13)
    mono_s  = sty('M',  fontSize=7,  fontName='Courier', textColor=HexColor('#222222'), leading=11)
    lbl_s   = sty('L',  fontSize=9,  fontName='Helvetica-Bold')
    sm_s    = sty('S',  fontSize=7,  fontName='Helvetica', textColor=HexColor('#555555'), leading=11)
    red_s   = sty('R',  fontSize=9,  fontName='Helvetica-Bold', textColor=RED)

    def banner(text, color=NAVY, size=15):
        s = sty('BS', fontSize=size, textColor=white, fontName='Helvetica-Bold', alignment=1)
        t = Table([[Paragraph(text, s)]], colWidths=[180*mm])
        t.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,-1),color),('ROWPADDING',(0,0),(-1,-1),8)]))
        return t

    def sec_hdr(text, color=NAVY):
        s = sty('SH', fontSize=11, textColor=white, fontName='Helvetica-Bold')
        t = Table([[Paragraph(text, s)]], colWidths=[180*mm])
        t.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,-1),color),('ROWPADDING',(0,0),(-1,-1),6)]))
        return t

    def kv(rows, c1=55*mm, c2=125*mm):
        data = [[Paragraph(k, lbl_s), Paragraph(v, body_s)] for k,v in rows]
        t = Table(data, colWidths=[c1, c2])
        t.setStyle(TableStyle([
            ('BACKGROUND',(0,0),(0,-1),LGRAY),
            ('GRID',(0,0),(-1,-1),0.3,MGRAY),
            ('ROWPADDING',(0,0),(-1,-1),5),
            ('VALIGN',(0,0),(-1,-1),'TOP'),
        ]))
        return t

    # Group by wallet
    by_wallet = {}
    for tx in all_found:
        w = tx['binance_wallet']
        if w not in by_wallet:
            by_wallet[w] = []
        by_wallet[w].append(tx)

    total_txs   = len(all_found)
    total_usdt  = sum(tx['amount'] for tx in all_found)
    wallets_hit = len(by_wallet)
    date_str    = datetime.now().strftime("%Y-%m-%d %H:%M UTC")

    story = []

    # ── Cover ────────────────────────────────────────────────
    story.append(Spacer(1, 15*mm))
    story.append(banner('BINANCE FRAUD TRANSACTION EVIDENCE', RED))
    story.append(Spacer(1, 3*mm))
    story.append(banner('BG Wealth Sharing / DSJEX — Pig Butchering Scam (杀猪盘)', NAVY, 12))
    story.append(Spacer(1, 8*mm))

    story.append(kv([
        ('Report Date',       date_str),
        ('Master Wallet',     MASTER_WALLET),
        ('Victim Count',      '~100,000+ (101,902 transactions on master wallet)'),
        ('Total Stolen',      '~$1,000,000,000+ USDT'),
        ('Binance Wallets Hit', f'{wallets_hit} of 6 hot wallets'),
        ('Transactions Found', f'{total_txs} confirmed transactions'),
        ('Total to Binance',  f'${total_usdt:,.2f} USDT (confirmed sample)'),
        ('Report Status',     'Operation shut down May 2026 — demanding unlock fees from victims'),
        ('Full Investigation', 'https://medium.com/@hades280389/i-reverse-engineered-bg-wealth-sharing-inside-the-infrastructure-of-a-billion-dollar-pig-60b7de582483'),
    ]))

    story.append(Spacer(1, 6*mm))
    story.append(HRFlowable(width='100%', thickness=1, color=RED))
    story.append(Spacer(1, 4*mm))
    story.append(Paragraph(
        '<b>Purpose:</b> This report provides Binance compliance with the complete '
        'transaction evidence linking the scam master wallet to Binance hot wallets, '
        'enabling identification and freezing of the accounts that received fraud proceeds.',
        body_s))

    # ── Summary table ─────────────────────────────────────────
    story.append(PageBreak())
    story.append(sec_hdr('1. SUMMARY — TRANSACTIONS BY BINANCE HOT WALLET', NAVY))
    story.append(Spacer(1, 4*mm))

    sum_data = [['Binance Wallet', 'Label', 'Transactions', 'Total USDT', 'Wallet Address']]
    for addr, label in BINANCE_HOT_WALLETS.items():
        txs = by_wallet.get(addr, [])
        total = sum(t['amount'] for t in txs)
        sum_data.append([
            label,
            '✅ FOUND' if txs else '— not found',
            str(len(txs)),
            f'${total:,.2f}' if txs else '—',
            addr,
        ])

    sum_tbl = Table(sum_data, colWidths=[22*mm, 22*mm, 24*mm, 24*mm, 88*mm])
    sum_tbl.setStyle(TableStyle([
        ('BACKGROUND',    (0,0),  (-1,0),  NAVY),
        ('TEXTCOLOR',     (0,0),  (-1,0),  white),
        ('FONTNAME',      (0,0),  (-1,0),  'Helvetica-Bold'),
        ('FONTSIZE',      (0,0),  (-1,-1), 8),
        ('FONTNAME',      (4,1),  (4,-1),  'Courier'),
        ('GRID',          (0,0),  (-1,-1), 0.3, MGRAY),
        ('ROWPADDING',    (0,0),  (-1,-1), 5),
        ('ROWBACKGROUNDS',(0,1),  (-1,-1), [white, LGRAY]),
    ]))
    story.append(sum_tbl)
    story.append(Spacer(1, 4*mm))

    # ── Transaction chain ─────────────────────────────────────
    story.append(sec_hdr('2. TRANSACTION CHAIN', NAVY))
    story.append(Spacer(1, 4*mm))

    chain_data = [
        ['Step', 'Description', 'Address'],
        ['0', 'Victims deposit to platform', 'Platform-assigned TRC20 addresses (~100,000 victims)'],
        ['1', 'Master collection wallet\n(aggregates all victim funds)', MASTER_WALLET],
        ['2', 'Intermediate mixer wallets\n(multiple, distribute in batches)', 'e.g. TPcgRgCbc34BoFgLG4QSy4S9PYcFAe63W4'],
        ['3', 'Binance hot wallets\n(final cashout)', '\n'.join([f"{v}: {k}" for k,v in BINANCE_HOT_WALLETS.items()])],
    ]
    ch_tbl = Table(chain_data, colWidths=[12*mm, 50*mm, 118*mm])
    ch_tbl.setStyle(TableStyle([
        ('BACKGROUND',  (0,0),  (-1,0),  NAVY),
        ('TEXTCOLOR',   (0,0),  (-1,0),  white),
        ('FONTNAME',    (0,0),  (-1,0),  'Helvetica-Bold'),
        ('FONTNAME',    (2,1),  (2,-1),  'Courier'),
        ('FONTSIZE',    (0,0),  (-1,-1), 8),
        ('GRID',        (0,0),  (-1,-1), 0.3, MGRAY),
        ('ROWPADDING',  (0,0),  (-1,-1), 6),
        ('VALIGN',      (0,0),  (-1,-1), 'TOP'),
        ('ALIGN',       (0,0),  (0,-1),  'CENTER'),
        ('FONTNAME',    (0,0),  (0,-1),  'Helvetica-Bold'),
        ('BACKGROUND',  (0,1),  (-1,1),  HexColor('#FFF0F0')),
        ('BACKGROUND',  (0,2),  (-1,2),  HexColor('#FFF8F0')),
        ('BACKGROUND',  (0,3),  (-1,3),  HexColor('#FFFDE0')),
        ('BACKGROUND',  (0,4),  (-1,4),  HexColor('#F0FFF0')),
    ]))
    story.append(ch_tbl)

    # ── Per-wallet detail pages ───────────────────────────────
    for addr, label in BINANCE_HOT_WALLETS.items():
        txs = by_wallet.get(addr, [])
        story.append(PageBreak())

        color = RED if txs else MGRAY
        story.append(sec_hdr(f'3. {label.upper()} — {"TRANSACTIONS FOUND" if txs else "NO TRANSACTIONS FOUND"}', color))
        story.append(Spacer(1, 4*mm))

        wallet_total = sum(t['amount'] for t in txs)
        story.append(kv([
            ('Wallet Address',    addr),
            ('Label',             label),
            ('TronScan',          f'https://tronscan.org/#/address/{addr}'),
            ('Transactions',      str(len(txs))),
            ('Total Amount',      f'${wallet_total:,.2f} USDT' if txs else '—'),
            ('Status',            '✅ FRAUD PROCEEDS CONFIRMED' if txs else 'Not found in this trace'),
        ]))

        if not txs:
            story.append(Spacer(1, 4*mm))
            story.append(Paragraph('No transactions found to this wallet in the current trace depth.', body_s))
            continue

        story.append(Spacer(1, 4*mm))
        story.append(Paragraph(f'All {len(txs)} transaction hashes:', h2_s))

        # Transaction table
        tx_hdr = [['#', 'Transaction Hash', 'Amount (USDT)', 'From Address']]
        for i, tx in enumerate(txs, 1):
            tx_hdr.append([
                str(i),
                tx['tx_hash'][:34] + '...',
                f"${tx['amount']:,.2f}",
                tx['from_address'][:28] + '...',
            ])
        tx_hdr.append(['', 'TOTAL', f"${wallet_total:,.2f}", ''])

        tt = Table(tx_hdr, colWidths=[8*mm, 78*mm, 28*mm, 66*mm])
        tt.setStyle(TableStyle([
            ('BACKGROUND',     (0,0),  (-1,0),  NAVY),
            ('TEXTCOLOR',      (0,0),  (-1,0),  white),
            ('FONTNAME',       (0,0),  (-1,0),  'Helvetica-Bold'),
            ('FONTSIZE',       (0,0),  (-1,-1), 7),
            ('FONTNAME',       (1,1),  (1,-2),  'Courier'),
            ('FONTNAME',       (3,1),  (3,-2),  'Courier'),
            ('BACKGROUND',     (0,-1), (-1,-1), HexColor('#FFE0E0')),
            ('FONTNAME',       (0,-1), (-1,-1), 'Helvetica-Bold'),
            ('TEXTCOLOR',      (0,-1), (-1,-1), RED),
            ('GRID',           (0,0),  (-1,-1), 0.3, MGRAY),
            ('ROWPADDING',     (0,0),  (-1,-1), 4),
            ('ROWBACKGROUNDS', (0,1),  (-1,-2), [white, LGRAY]),
            ('ALIGN',          (2,0),  (2,-1),  'RIGHT'),
        ]))
        story.append(tt)
        story.append(Spacer(1, 4*mm))

        # Full hashes
        story.append(Paragraph('Full 64-character transaction hashes (for Binance internal lookup):', h2_s))
        for i, tx in enumerate(txs, 1):
            story.append(Paragraph(f'<b>[{i}]</b>  ${tx["amount"]:,.2f} USDT', sm_s))
            story.append(Paragraph(f'TX: {tx["tx_hash"]}', mono_s))
            story.append(Paragraph(f'https://tronscan.org/#/transaction/{tx["tx_hash"]}', sm_s))
            story.append(Spacer(1, 1.5*mm))

    # ── Requests page ─────────────────────────────────────────
    story.append(PageBreak())
    story.append(sec_hdr('4. FORMAL REQUESTS TO BINANCE COMPLIANCE', RED))
    story.append(Spacer(1, 4*mm))

    reqs = [
        ('Request 1', f'Identify all internal Binance accounts that deposited funds traceable to master wallet {MASTER_WALLET}'),
        ('Request 2', 'Freeze all identified accounts and any associated accounts immediately'),
        ('Request 3', 'Preserve all KYC records, IP logs, device fingerprints, and transaction history'),
        ('Request 4', 'Share account identities with law enforcement: Vietnam NCSC, FBI IC3, Interpol'),
        ('Request 5', f'Cross-reference all {len(BINANCE_HOT_WALLETS)} hot wallets listed in this report'),
        ('Request 6', 'Provide case reference number for follow-up'),
        ('Urgency',   'Operation has SHUT DOWN and is demanding unlock fees — operators may be preparing to disappear'),
    ]
    story.append(kv(reqs, c1=22*mm, c2=158*mm))
    story.append(Spacer(1, 6*mm))

    story.append(sec_hdr('5. ADDITIONAL INFRASTRUCTURE EVIDENCE', NAVY))
    story.append(Spacer(1, 4*mm))
    story.append(kv([
        ('Scam Type',        'Pig Butchering (杀猪盘) — fake crypto exchange DSJEX'),
        ('Backend API',      'api.ddjea.com  |  api.ddjeb.com  |  api.dsjhout.com'),
        ('File Storage',     'Alibaba Cloud OSS — oss-cn-hangzhou (victim KYC documents)'),
        ('Alibaba Report',   'Case #20260502710120867026 filed with Alibaba Cloud'),
        ('Kit Version',      'P2.9.3 (in all HTTP request headers)'),
        ('API Endpoints',    '141 endpoints mapped via JS deobfuscation (18 files, 68,098 strings)'),
        ('Also Reported To', 'Bybit, OKX, Gate.io, Bitget, MEXC, KuCoin, HTX, Bitfinex, FBI IC3, Vietnam NCSC, Chainabuse'),
    ]))

    story.append(Spacer(1, 6*mm))
    story.append(HRFlowable(width='100%', thickness=0.5, color=MGRAY))
    story.append(Spacer(1, 3*mm))
    story.append(Paragraph(
        'All blockchain data is publicly verifiable on TronScan (tronscan.org). '
        'This report was prepared by an independent researcher and fraud victim using standard '
        'blockchain analysis and JavaScript deobfuscation techniques. '
        f'Report generated: {date_str}',
        sm_s))

    doc.build(story)
    print(f"  PDF saved -> {output_path}")

# ── Main ──────────────────────────────────────────────────────
def main():
    # Step 1: Trace
    all_found = trace_all()

    # Step 2: Save JSON
    with open("all_binance_evidence.json", "w") as f:
        json.dump(all_found, f, indent=2)
    print(f"\n  JSON saved -> all_binance_evidence.json ({len(all_found)} transactions)")

    # Step 3: Print summary
    print(f"\n{'='*65}")
    print(f"  SUMMARY")
    print(f"{'='*65}")

    by_wallet = {}
    for tx in all_found:
        w = tx['binance_wallet']
        if w not in by_wallet:
            by_wallet[w] = []
        by_wallet[w].append(tx)

    total = sum(tx['amount'] for tx in all_found)
    print(f"\n  Total transactions found : {len(all_found)}")
    print(f"  Total USDT confirmed     : ${total:,.2f}")
    print(f"  Wallets hit              : {len(by_wallet)}/{len(BINANCE_HOT_WALLETS)}\n")

    for addr, label in BINANCE_HOT_WALLETS.items():
        txs = by_wallet.get(addr, [])
        wallet_total = sum(t['amount'] for t in txs)
        status = f"✅ {len(txs)} txs  ${wallet_total:,.2f} USDT" if txs else "— not found"
        print(f"  {label:15} {addr[:20]}...  {status}")

    # Step 4: Generate PDF
    print(f"\n{'='*65}")
    print("  GENERATING PDF REPORT...")
    print(f"{'='*65}")
    pdf_path = "Binance_All_Wallets_Evidence.pdf"
    make_pdf(all_found, pdf_path)

    print(f"\n{'='*65}")
    print(f"  DONE")
    print(f"  JSON -> all_binance_evidence.json")
    print(f"  PDF  -> {pdf_path}")
    print(f"{'='*65}\n")

if __name__ == "__main__":
    main()
