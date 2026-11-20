#!/usr/bin/env python3
"""
trace_to_binance.py
Traces fund flow from master collection wallet to Binance hot wallets
using TronScan public API.

Usage:
    python trace_to_binance.py

No dependencies beyond Python stdlib.
"""

import urllib.request
import urllib.parse
import json
import time
import sys

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

ALL_KNOWN_WALLETS = {
    MASTER_WALLET: "Master Collection Wallet",
    # Layer 2
    "TZ4QWj3MNggo1oWFNEti5QvM2ffMigm37X": "Layer2-1",
    "TKKPzgPzbz6Lwg6RToEwXaGnVeYiccvF1t": "Layer2-2",
    "TUAMyXHzdHJXGcpjqsQpdFt24y4VBBcfu3": "Layer2-3",
    "TCEFdHjSTe9VgG6647eqqFJ7j9EKgYhGLQ": "Layer2-4",
    "TK4RQYpcrPFpf9WU5vruqrgSoDt4bcYzVD": "Layer2-5",
    **BINANCE_HOT_WALLETS,
}

TRONSCAN_API = "https://apilist.tronscanapi.com/api"
HEADERS = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
    "Accept": "application/json",
}
DELAY = 0.8  # seconds between requests

# ── Helpers ───────────────────────────────────────────────────
def fetch(url):
    req = urllib.request.Request(url, headers=HEADERS)
    try:
        with urllib.request.urlopen(req, timeout=15) as r:
            return json.loads(r.read().decode("utf-8"))
    except Exception as e:
        print(f"  ! API error: {e}")
        return None

def get_outgoing_transfers(address, limit=100, start=0):
    """Get outgoing USDT TRC20 transfers from an address."""
    url = (
        f"{TRONSCAN_API}/token_trc20/transfers"
        f"?address={address}"
        f"&direction=out"
        f"&limit={limit}"
        f"&start={start}"
        f"&trc20Id=TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t"  # USDT contract
    )
    return fetch(url)

def get_address_transfers(address, limit=100):
    """Get all transfers involving an address."""
    url = (
        f"{TRONSCAN_API}/token_trc20/transfers"
        f"?relatedAddress={address}"
        f"&limit={limit}"
        f"&trc20Id=TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t"
    )
    return fetch(url)

def label(addr):
    return ALL_KNOWN_WALLETS.get(addr, addr[:12] + "...")

# ── Main tracer ───────────────────────────────────────────────
def trace():
    found_paths = []

    print(f"\n{'='*65}")
    print(f"  TRACING: {MASTER_WALLET}")
    print(f"  TARGET:  Binance hot wallets")
    print(f"{'='*65}\n")

    # ── Step 1: Get outgoing from master wallet ────────────────
    print(f"[1] Fetching outgoing transfers from master wallet...")
    visited = set()
    queue = [(MASTER_WALLET, [], 0)]  # (address, path_so_far, depth)
    max_depth = 10

    while queue:
        current_addr, path, depth = queue.pop(0)

        if depth > max_depth:
            continue
        if current_addr in visited:
            continue
        visited.add(current_addr)

        indent = "  " * depth
        print(f"{indent}[depth {depth}] Checking {label(current_addr)}")

        # Check if this is a Binance hot wallet
        if current_addr in BINANCE_HOT_WALLETS:
            print(f"\n  🎯 FOUND BINANCE HOT WALLET: {BINANCE_HOT_WALLETS[current_addr]}")
            print(f"  Path: {' -> '.join([label(p[0]) for p in path] + [label(current_addr)])}")
            # The transaction that brought us here
            if path:
                last_tx = path[-1]
                print(f"  Transaction Hash: {last_tx[1]}")
                print(f"  Amount: {last_tx[2]} USDT")
                print(f"  TronScan: https://tronscan.org/#/transaction/{last_tx[1]}")
                found_paths.append({
                    "binance_wallet": current_addr,
                    "binance_label": BINANCE_HOT_WALLETS[current_addr],
                    "tx_hash": last_tx[1],
                    "amount": last_tx[2],
                    "from_address": last_tx[3],
                    "path": [p[0] for p in path] + [current_addr],
                    "tronscan": f"https://tronscan.org/#/transaction/{last_tx[1]}"
                })
            continue

        # Get outgoing transfers
        data = get_outgoing_transfers(current_addr, limit=100)
        time.sleep(DELAY)

        if not data or "token_transfers" not in data:
            # Try alternate endpoint
            data = get_address_transfers(current_addr, limit=100)
            time.sleep(DELAY)
            if not data:
                continue

        transfers = data.get("token_transfers", data.get("data", []))
        if not transfers:
            print(f"{indent}  (no transfers found)")
            continue

        print(f"{indent}  Found {len(transfers)} transfers")

        for tx in transfers[:50]:
            to_addr = tx.get("to_address", tx.get("toAddress", ""))
            tx_hash = tx.get("transaction_id", tx.get("transactionId", tx.get("hash", "")))
            amount_raw = tx.get("quant", tx.get("amount", tx.get("value", 0)))

            # Convert amount (USDT has 6 decimals)
            try:
                amount = float(amount_raw) / 1_000_000
            except:
                amount = 0

            if not to_addr or not tx_hash:
                continue

            # Check if directly to Binance
            if to_addr in BINANCE_HOT_WALLETS:
                print(f"\n  🎯 DIRECT HIT! {label(current_addr)} -> {BINANCE_HOT_WALLETS[to_addr]}")
                print(f"  TX Hash  : {tx_hash}")
                print(f"  Amount   : {amount:,.2f} USDT")
                print(f"  TronScan : https://tronscan.org/#/transaction/{tx_hash}")
                found_paths.append({
                    "binance_wallet": to_addr,
                    "binance_label": BINANCE_HOT_WALLETS[to_addr],
                    "tx_hash": tx_hash,
                    "amount": amount,
                    "from_address": current_addr,
                    "path": [p[0] for p in path] + [current_addr, to_addr],
                    "tronscan": f"https://tronscan.org/#/transaction/{tx_hash}"
                })
            elif to_addr not in visited and depth < max_depth:
                # Queue intermediate address for further tracing
                queue.append((
                    to_addr,
                    path + [(current_addr, tx_hash, amount, current_addr)],
                    depth + 1
                ))

        if len(found_paths) >= 3:
            print(f"\n  Found {len(found_paths)} path(s) — stopping search")
            break

    # ── Results ───────────────────────────────────────────────
    print(f"\n{'='*65}")
    print(f"  RESULTS")
    print(f"{'='*65}")

    if found_paths:
        print(f"\n  ✅ Found {len(found_paths)} transaction(s) to Binance\n")
        for i, p in enumerate(found_paths, 1):
            print(f"  [{i}] {p['binance_label']}")
            print(f"       TX Hash : {p['tx_hash']}")
            print(f"       Amount  : {p['amount']:,.2f} USDT")
            print(f"       From    : {p['from_address']}")
            print(f"       To      : {p['binance_wallet']}")
            print(f"       Link    : {p['tronscan']}")
            print()

        # Save results
        with open("binance_tx_evidence.json", "w") as f:
            json.dump(found_paths, f, indent=2)
        print(f"  Saved -> binance_tx_evidence.json")
        print(f"\n  Send this to Binance support:")
        print(f"  TX Hash: {found_paths[0]['tx_hash']}")
        print(f"  Link: {found_paths[0]['tronscan']}")

    else:
        print(f"\n  ⚠️  No direct path found in {max_depth} hops")
        print(f"  The mixer hops may be deeper than {max_depth} levels")
        print(f"  Try tracing manually from Layer 2 wallets:")
        for addr, label_name in {
            "TZ4QWj3MNggo1oWFNEti5QvM2ffMigm37X": "Layer2-1",
            "TKKPzgPzbz6Lwg6RToEwXaGnVeYiccvF1t": "Layer2-2",
            "TUAMyXHzdHJXGcpjqsQpdFt24y4VBBcfu3": "Layer2-3",
        }.items():
            print(f"  https://tronscan.org/#/address/{addr}/transfers")

    print(f"\n{'='*65}\n")
    return found_paths

if __name__ == "__main__":
    trace()
