#!/usr/bin/env python3
"""
trace_full_path.py
Traces the COMPLETE path from master wallet to Binance hot wallet.
Finds every transaction hash at every hop.

Usage:
    python trace_full_path.py
"""

import urllib.request
import json
import time

# ── Config ────────────────────────────────────────────────────
MASTER_WALLET    = "TWF1bcuNLXtwQCRxb6Qn6uKqWCuXRatiei"
INTERMEDIATE     = "TPcgRgCbc34BoFgLG4QSy4S9PYcFAe63W4"  # found from previous trace
BINANCE_HOT_7    = "TDqSquXBgUCLYvYC4XZgrprLK589dkhSCf"

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

# ── Helpers ───────────────────────────────────────────────────
def fetch(url):
    req = urllib.request.Request(url, headers=HEADERS)
    try:
        with urllib.request.urlopen(req, timeout=15) as r:
            return json.loads(r.read().decode("utf-8"))
    except Exception as e:
        print(f"  ! {e}")
        return None

def get_transfers(address, direction="out", limit=100):
    url = (
        f"{TRONSCAN}/token_trc20/transfers"
        f"?address={address}"
        f"&direction={direction}"
        f"&limit={limit}"
        f"&trc20Id={USDT}"
    )
    return fetch(url)

def get_tx_detail(tx_hash):
    url = f"{TRONSCAN}/transaction-info?hash={tx_hash}"
    return fetch(url)

def short(addr):
    return addr[:8] + "..." + addr[-6:]

def fmt_amount(raw):
    try:
        return float(raw) / 1_000_000
    except:
        return 0

# ── Step 1: Find tx from Master → Intermediate ────────────────
def find_master_to_intermediate():
    print(f"\n{'='*65}")
    print(f"  STEP 1: Finding tx from Master Wallet → Intermediate")
    print(f"  From : {MASTER_WALLET}")
    print(f"  To   : {INTERMEDIATE}")
    print(f"{'='*65}")

    results = []
    for start in range(0, 300, 100):
        print(f"  Fetching transfers (offset {start})...")
        data = get_transfers(MASTER_WALLET, direction="out", limit=100)
        time.sleep(DELAY)

        if not data:
            break

        transfers = data.get("token_transfers", [])
        if not transfers:
            break

        for tx in transfers:
            to = tx.get("to_address", "")
            tx_hash = tx.get("transaction_id", "")
            amount = fmt_amount(tx.get("quant", 0))
            timestamp = tx.get("block_ts", "")

            if to == INTERMEDIATE:
                results.append({
                    "hop": 1,
                    "from": MASTER_WALLET,
                    "from_label": "Master Collection Wallet",
                    "to": INTERMEDIATE,
                    "to_label": "Intermediate Mixer",
                    "tx_hash": tx_hash,
                    "amount": amount,
                    "timestamp": timestamp,
                    "tronscan": f"https://tronscan.org/#/transaction/{tx_hash}"
                })
                print(f"  ✅ Found! TX: {tx_hash[:32]}...")
                print(f"     Amount: {amount:,.2f} USDT")

        if results:
            break

    if not results:
        print(f"  ! Not found in first 300 txs — trying reverse lookup...")
        # Try getting incoming to intermediate from master
        data2 = get_transfers(INTERMEDIATE, direction="in", limit=100)
        time.sleep(DELAY)
        if data2:
            for tx in data2.get("token_transfers", []):
                frm = tx.get("from_address", "")
                tx_hash = tx.get("transaction_id", "")
                amount = fmt_amount(tx.get("quant", 0))
                if frm == MASTER_WALLET:
                    results.append({
                        "hop": 1,
                        "from": MASTER_WALLET,
                        "from_label": "Master Collection Wallet",
                        "to": INTERMEDIATE,
                        "to_label": "Intermediate Mixer",
                        "tx_hash": tx_hash,
                        "amount": amount,
                        "tronscan": f"https://tronscan.org/#/transaction/{tx_hash}"
                    })
                    print(f"  ✅ Found via reverse! TX: {tx_hash[:32]}...")

    return results

# ── Step 2: Intermediate → Binance (already found) ───────────
def find_intermediate_to_binance():
    print(f"\n{'='*65}")
    print(f"  STEP 2: Finding txs from Intermediate → Binance Hot 7")
    print(f"  From : {INTERMEDIATE}")
    print(f"  To   : {BINANCE_HOT_7}")
    print(f"{'='*65}")

    results = []
    data = get_transfers(INTERMEDIATE, direction="out", limit=100)
    time.sleep(DELAY)

    if data:
        for tx in data.get("token_transfers", []):
            to = tx.get("to_address", "")
            tx_hash = tx.get("transaction_id", "")
            amount = fmt_amount(tx.get("quant", 0))
            timestamp = tx.get("block_ts", "")

            if to in BINANCE_HOT_WALLETS:
                results.append({
                    "hop": 2,
                    "from": INTERMEDIATE,
                    "from_label": "Intermediate Mixer",
                    "to": to,
                    "to_label": BINANCE_HOT_WALLETS[to],
                    "tx_hash": tx_hash,
                    "amount": amount,
                    "timestamp": timestamp,
                    "tronscan": f"https://tronscan.org/#/transaction/{tx_hash}"
                })

    print(f"  Found {len(results)} transactions to Binance hot wallets")
    return results

# ── Step 3: Check if there are more hops before intermediate ──
def find_intermediate_source():
    """Check if intermediate itself received from another mixer (deeper chain)"""
    print(f"\n{'='*65}")
    print(f"  STEP 3: Checking source of Intermediate wallet")
    print(f"  Address: {INTERMEDIATE}")
    print(f"{'='*65}")

    data = get_transfers(INTERMEDIATE, direction="in", limit=100)
    time.sleep(DELAY)

    sources = {}
    if data:
        for tx in data.get("token_transfers", []):
            frm = tx.get("from_address", "")
            amount = fmt_amount(tx.get("quant", 0))
            tx_hash = tx.get("transaction_id", "")
            if frm not in sources:
                sources[frm] = {"count": 0, "total": 0, "sample_tx": tx_hash}
            sources[frm]["count"] += 1
            sources[frm]["total"] += amount

    print(f"  Incoming sources ({len(sources)} unique addresses):")
    for addr, info in sorted(sources.items(), key=lambda x: x[1]["total"], reverse=True)[:10]:
        label = "MASTER WALLET ✅" if addr == MASTER_WALLET else short(addr)
        print(f"  {label}: {info['count']} txs, {info['total']:,.2f} USDT total")
        if info["sample_tx"]:
            print(f"    Sample TX: https://tronscan.org/#/transaction/{info['sample_tx']}")

    return sources

# ── Main ──────────────────────────────────────────────────────
def main():
    print(f"\n{'='*65}")
    print(f"  FULL PATH TRACER")
    print(f"  Master → Intermediate → Binance Hot 7")
    print(f"{'='*65}")

    hop1 = find_master_to_intermediate()
    hop2 = find_intermediate_to_binance()
    sources = find_intermediate_source()

    # ── Print full chain ──────────────────────────────────────
    print(f"\n{'='*65}")
    print(f"  COMPLETE TRANSACTION CHAIN")
    print(f"{'='*65}")

    print(f"""
  MASTER COLLECTION WALLET
  {MASTER_WALLET}
  (101,902 victim deposits, ~$1B+ USDT total)
         │
         │ HOP 1 — Master → Intermediate
""")

    if hop1:
        for tx in hop1[:3]:
            print(f"         TX: {tx['tx_hash']}")
            print(f"         Amount: {tx['amount']:,.2f} USDT")
            print(f"         Link: {tx['tronscan']}")
    else:
        print(f"         TX: [see binance_tx_evidence.json for confirmed txs]")
        print(f"         (Master sends to intermediate in batches)")

    print(f"""
         ▼
  INTERMEDIATE MIXER WALLET
  {INTERMEDIATE}
  (Receives from master, sends $1,800-$22,000 chunks to Binance)
         │
         │ HOP 2 — Intermediate → Binance Hot 7
""")

    if hop2:
        for tx in hop2[:5]:
            print(f"         TX: {tx['tx_hash']}")
            print(f"         Amount: {tx['amount']:,.2f} USDT")
            print(f"         Link: {tx['tronscan']}")

    print(f"""
         ▼
  BINANCE HOT WALLET 7
  {BINANCE_HOT_7}
  ($455,686,673 total balance)
  ← Binance has KYC on the account that deposited here
""")

    # ── Save full evidence ────────────────────────────────────
    evidence = {
        "chain_summary": {
            "step1_master_to_intermediate": f"{MASTER_WALLET} → {INTERMEDIATE}",
            "step2_intermediate_to_binance": f"{INTERMEDIATE} → {BINANCE_HOT_7}",
            "binance_label": "Binance Hot 7",
        },
        "hop1_transactions": hop1,
        "hop2_transactions": hop2[:10],
        "intermediate_sources": {
            addr: info for addr, info in list(sources.items())[:10]
        }
    }

    with open("full_path_evidence.json", "w") as f:
        json.dump(evidence, f, indent=2)

    print(f"\n  Saved -> full_path_evidence.json")

    # ── Binance report message ────────────────────────────────
    print(f"\n{'='*65}")
    print(f"  MESSAGE TO SEND BINANCE SUPPORT")
    print(f"{'='*65}")

    hop2_sample = hop2[0] if hop2 else None
    hop1_sample = hop1[0] if hop1 else None

    msg = f"""
The full transaction path from scam master wallet to your Binance 
Hot Wallet 7 is as follows:

STEP 1 — Master collection wallet to intermediate:
  From : {MASTER_WALLET} (scam master wallet, 101,902 victim txs)
  To   : {INTERMEDIATE} (intermediate mixer)
  TX   : {hop1_sample['tx_hash'] if hop1_sample else '[see full_path_evidence.json]'}
  Link : {hop1_sample['tronscan'] if hop1_sample else 'https://tronscan.org/#/address/' + INTERMEDIATE}

STEP 2 — Intermediate to Binance Hot 7:
  From : {INTERMEDIATE}
  To   : {BINANCE_HOT_7} (Binance Hot 7)
  TX   : {hop2_sample['tx_hash'] if hop2_sample else '[see binance_tx_evidence.json]'}
  Link : {hop2_sample['tronscan'] if hop2_sample else ''}

24 confirmed transactions from this intermediate wallet to 
Binance Hot 7 totaling approximately $70,000+ USDT.

Please identify the Binance account that deposited funds 
matching this transaction chain and freeze it immediately.

Full evidence: full_path_evidence.json (attached)
"""
    print(msg)

    with open("binance_message.txt", "w") as f:
        f.write(msg)
    print(f"  Saved -> binance_message.txt")
    print(f"\n{'='*65}\n")

if __name__ == "__main__":
    main()
