# I Reverse-Engineered BG Wealth Sharing: Inside the Infrastructure of a Billion-Dollar Pig Butchering Operation

*A software engineer's technical deep-dive into BG Wealth Sharing, DSJEX, and the infrastructure behind one of the most sophisticated pig butchering networks currently operating*

> **Update — May 2026:** The platform has shut down all victim accounts and is demanding additional "unlock fees" to restore access. This is the final phase of the scam before operators disappear. All evidence documented here has been submitted to 9 exchanges, Alibaba Cloud, FBI IC3, Vietnam NCSC, and Chainabuse.

---

## Who Should Read This

- Anyone who has been approached about "BG Wealth Sharing" or "DSJEX"
- Crypto security researchers and blockchain investigators
- Journalists covering organized cybercrime in Southeast Asia
- Law enforcement agencies monitoring financial fraud networks
- Anyone curious about how modern scam operations actually work under the hood

---

## Background — How I Got Here

I was targeted by this operation personally. After being defrauded and verifying on Tronscan that deposited funds were swept within minutes, I decided not to just walk away. I'm a software engineer with experience in web development, APIs, and networking. Instead of filing a report that would likely go nowhere, I spent time systematically reverse-engineering the entire technical infrastructure of the operation.

This post documents what I found. Everything here is based on direct technical investigation — network traffic analysis, JavaScript deobfuscation, API endpoint mapping, and on-chain transaction tracing. No speculation beyond what the evidence shows.

---

## What Is BG Wealth Sharing / DSJEX?

On the surface, BG Wealth Sharing presents itself as a community investment group that provides "trading signals" for a cryptocurrency exchange called DSJEX (DSJ Exchange). Members are told they can earn consistent daily profits by following these signals on the platform.

The pitch is polished and professional:

- A named figurehead — "Professor Stephen Beard" — who presents as a legitimate financial expert
- Weekly Zoom meetings with hundreds of attendees globally
- Offline events hosted across multiple countries, each with dozens to hundreds of participants
- A structured 12-level agent hierarchy with defined salaries and promotion requirements
- Slick marketing materials in English, Vietnamese, Chinese, Korean, German, French, Polish, Lithuanian, Indonesian, Hungarian, Portuguese, Spanish, and Italian — 14 languages targeting a global victim pool
- A private encrypted communication platform (BonChat) used exclusively for official communications
- Fabricated profit tables showing perfectly smooth daily gains across 25+ consecutive days with zero losing sessions

In reality, DSJEX is a fake exchange. The balances displayed are numbers in a database. The trading signals lead to simulated trades. Withdrawals are locked by default. The money deposited goes directly to scammer-controlled cryptocurrency wallets and is moved within minutes.

This is a pig butchering scam — known in Chinese as 杀猪盘 (shā zhū pán). The term refers to the process of fattening a pig before slaughter — building trust and showing fake profits before executing the final theft.

---

## The Technical Investigation

### Step 1 — The Frontend

The scam uses an uncountable number of rotating frontend domains across multiple TLDs. While some follow patterns like `dspp[number].tw`, others are completely random — confirmed examples include `32dde.com`, `wdw3.vip`, `bgbg66.net`, `rfrt5.vip`, and dozens more. These domains are entirely disposable and rotate every 24–48 hours or faster. When Cloudflare blocks one, a new domain is live within hours and victims are silently redirected via BonChat announcements. There is no point trying to track or report individual domains — the operation generates new ones faster than any takedown process. The domain is nothing more than a throwaway entry point. The persistent infrastructure underneath — `api.ddjea.com`, the Alibaba Cloud OSS bucket, the master wallets — is what actually identifies this operation and never changes.

The frontend is a Vue.js Single Page Application, heavily obfuscated using javascript-obfuscator with a custom base64 string encoding scheme. The source contains 32 JavaScript chunks totaling approximately 18MB of obfuscated code. Key libraries embedded include:

- `npm.web3` and `npm.ethereumjs` — for connecting to cryptocurrency wallets
- `npm.klinecharts` — for rendering fake trading charts
- `npm.echarts` — for fabricated profit visualizations
- `npm.qrcode` — for generating deposit QR codes
- `ali-oss` — Alibaba Cloud OSS SDK for storing victim KYC documents

The version string `P2.9.3` appears consistently in every request header (`app-version: P2.9.3`), suggesting this is a versioned scam kit sold or distributed to multiple operators.

### Step 2 — JavaScript Deobfuscation

After downloading all 18 application JavaScript files, I reverse-engineered the custom base64 encoding used by the obfuscator. Each file contains a string array of thousands of encoded values. By extracting the decoder function and running it against the array, I recovered **68,098 decoded strings** across all files.

Key findings from deobfuscation:

**141 API endpoints** — more than triple the 42 initially found through traffic analysis alone. The full endpoint list reveals a complete fake exchange ecosystem including perpetual futures, options trading, casino/gambling features, C2C trading, loan products, team leaderboards, a demo account system, and more.

**Three backend domains** (not one):
- `api.ddjea.com` — primary backend
- `api.ddjeb.com` — secondary/backup backend
- `api.dsjhout.com` — third backend domain
- `ds.dsjhout.com` — media and file server

**Internal identifiers unique to this codebase:**
- `yongxuDemo` — hardcoded test account username, present verbatim in the production binary
- `xchain.wxtome.link` — an unidentified linked domain, possibly another operation in the same network
- `pcchain.wxpass.net` — internal test domain found in plain text

**WebSocket endpoints for real-time data:**
- `/ws/app/perpetual` — live perpetual futures price feed
- `/ws/app/chat/` — live chat system
- `/ws/sub-quotes` — market data feed

**Alibaba Cloud OSS strings** — see Step 4.

The `yongxuDemo` identifier is particularly useful for fingerprinting: any other site returning this string in its JavaScript is running the same scam kit, regardless of domain or branding.

### Step 3 — The Backend API

By intercepting network traffic while browsing the live site, I identified the backend API. The API runs on Java Spring Boot, identifiable from the pagination response structure and numeric error code conventions.

All requests include custom headers:
```
app-login-token: [session token]
app-version: P2.9.3
app-client-timezone: +8
app-analog: false
aws-check: true
```

The timezone hardcoding (`+8`) combined with the Alibaba Cloud infrastructure (see Step 4) strongly suggests operators based in mainland China.

I mapped **141 distinct API endpoints** covering the full platform functionality:

**User, authentication, and KYC:**
- `/api/app/user/login` and `/api/app/user/register`
- `/api/app/user/certification/ordinary` — KYC document submission
- `/api/app/user/certification/advanced` — advanced verification
- `/api/app/user/google/auth` — Google 2FA
- `/api/app/user/withdrawal/password/status` — withdrawal lock status

**Funds and wallet:**
- `/api/app/funds/overview` — fake balance display
- `/api/app/funds/wallet/money` — per-coin wallet details
- `/api/app/funds/exchange/submit` — internal currency conversion
- `/api/app/upload/s3/pre-signed` — issues Alibaba Cloud OSS credentials for KYC document upload

**Deposits:**
- `/api/app/recharge/payment/submit`
- `/api/app/recharge/payment/address/normal/pay`
- `/api/app/prompt/recharge` — deposit notifications

**Withdrawals (locked by design):**
- `/api/app/withdraw/submit`
- `/api/app/withdraw/address/coin`
- `/api/app/prompt/withdraw` — withdrawal notifications

**Trading (all simulated):**
- `/api/app/second/order/submit/by/time` — options order
- `/api/app/perpetual/order/submit` — perpetual futures
- `/api/app/spot/submit` — spot trading
- `/api/app/futures/product/submit` — futures
- `/api/app/quotes/trade` — fake market data

**Casino/gambling:**
- `/api/app/game/turn` — spin/bet
- `/api/app/game/prize/type` — prize categories
- `/api/app/game/history/type` — gambling history

**MLM/referral infrastructure:**
- `/api/app/share/team` — downline management
- `/api/app/rank/team` — team leaderboard
- `/api/app/second/share/user/add` — referral registration

**Demo account system:**
- `/api/app/demo/reset/assets` — confirms a demo account system used to show fake profits to prospects before asking for real deposits

**Chat:**
- `/api/app/chat/send` and `/api/app/chat/record`
- `/api/app/chat/welcome_message`

This endpoint map confirms the platform is a fully built fake exchange ecosystem targeting victims across every possible financial product category.

### Step 4 — Infrastructure Fingerprinting

| Component | Detail |
|---|---|
| Frontend CDN | Cloudflare (104.21.x.x) |
| Backend APIs | api.ddjea.com · api.ddjeb.com · api.dsjhout.com |
| Media server | ds.dsjhout.com |
| **File storage** | **Alibaba Cloud OSS — oss-cn-hangzhou (Hangzhou, China)** |
| Communication | BonChat (self-hosted encrypted messaging) |
| Backend framework | Java Spring Boot |
| Frontend framework | Vue.js + Webpack |
| Obfuscation | javascript-obfuscator with custom base64 encoding |
| Kit version | P2.9.3 |
| API endpoints | 141 fully mapped |
| WebSocket feeds | /ws/app/perpetual · /ws/app/chat/ · /ws/sub-quotes |

**Important correction from an earlier version of this post:** the file storage backend is **Alibaba Cloud OSS**, not AWS S3. The upload endpoint `/api/app/upload/s3/pre-signed` is misleadingly named, but deobfuscation of the JavaScript reveals the actual implementation:

```
oss-cn-hangzhou               ← Hangzhou, China data center
vpc100-oss-cn-[region]        ← VPC-based private OSS access
OSSAccessKeyId                ← Alibaba authentication
x-oss-security-token          ← STS temporary token
OSS4-HMAC-SHA256              ← Alibaba v4 signing algorithm
ali-oss                       ← Alibaba OSS npm SDK
refreshSTSTokenInterval       ← Tokens auto-refresh client-side
```

Victims upload government ID documents and selfies during a fake verification process. These documents are stored in an Alibaba Cloud OSS bucket in Hangzhou, China — under an account registered with a real Chinese identity. The bucket name is issued dynamically per session via the `/api/app/upload/s3/pre-signed` endpoint.

An abuse report was filed with Alibaba Cloud: **Case #20260502710120867026**.

### Step 5 — Confirming the Fake Balance

Using the captured session token and correct request headers, I probed the live API directly.

The `/api/app/funds/overview` endpoint returned:
```json
{
  "resultCode": true,
  "errCode": 0,
  "data": {
    "usdtTotal": 1129.5,
    "usdtAvailable": 1129.5,
    "usdtUnavailable": 0.0
  }
}
```

The `/api/app/user/withdrawal/password/status` endpoint returned:
```json
{
  "resultCode": true,
  "errCode": 0,
  "data": false
}
```

This confirms the two-layer trap: the balance shown is a fabricated number stored in their database, and withdrawals are administratively locked. Victims see money they cannot access and are subsequently told they must pay "taxes," "verification fees," or "unlock fees" to release it — all of which go directly to the scammers.

The demo account endpoint `/api/app/demo/reset/assets` confirms that prospects are shown a demo account with fake growing profits before being asked to deposit real money — a standard pig butchering grooming technique.

---

## The Organizational Structure

Beyond the technical infrastructure, the promotional materials I obtained reveal a sophisticated multi-level organizational structure:

**The pyramid hierarchy** runs 12 levels deep, from Level 1 (5 direct recruits, $30/month minimum salary) to Level 12 (300,000 team members, $1,000,000/month coverage). The structure is mathematically identical to a pyramid scheme — sustainable only through continuous new recruitment.

**The recruitment mechanics** use three psychological levers simultaneously:
- Financial incentives (referral bonuses up to 10% of new member deposits)
- Social pressure (weekly Zoom calls, offline events, team-building requirements)
- Fear of missing out (time-limited IPO campaigns, exclusive signal tiers unlocked by recruitment)

**The inoculation technique** is particularly sophisticated. BG distributes "Anti-Scam Tips" to all members warning them to ignore anyone outside BonChat channels, distrust urgent warnings, and verify everything through official channels only. This is a documented cult recruitment technique — it causes victims to dismiss legitimate warnings from family and friends as "exactly what BG warned us about."

**The bot infrastructure** maintains the illusion of an active community 24/7 through automated messages in group chats — "Together we achieve financial freedom," "Every new member's addition is a testament to the team's growth," "Your joining makes the team stronger." These run continuously to create social proof and maintain emotional engagement between signal sessions.

**The "Professor Stephen Beard" persona** serves as a trusted authority figure and point of contact for high-value members. This is almost certainly a paid actor or constructed identity. The real operators remain anonymous behind this frontman.

---

## Why This Operation Is Different

I have seen many similar scam operations. Most last weeks to months before collapsing or getting blocked. BG Wealth Sharing operated for years. The differences that explain its longevity:

**Operational security:** No real identities exposed. Domain rotation means no persistent frontend surface. Cloudflare means no server exposure. BonChat means no Telegram message leaks. Chain layering means no clean on-chain trail.

**Organizational depth:** The 12-level hierarchy means thousands of people globally have a financial stake in recruiting new victims. This creates a self-sustaining recruitment machine that operates independently of the core operators.

**Franchise model:** The `P2.9.3` version string and the polished kit quality suggest this infrastructure is sold or licensed to other operations. Multiple similar scams running shorter lifecycles appear to use the same underlying platform — BG Wealth Sharing is likely the source operation while others are franchisees.

**Psychological sophistication:** The anti-scam inoculation, the bot social proof, the fake professor authority figure, the "IPO" urgency narrative — these are not improvised. They reflect knowledge of cult recruitment methodology and behavioral psychology applied at scale.

**Global targeting:** Marketing in 14 languages, offline recruitment events across multiple continents, and timezone hardcoding to +8 (China Standard Time) while targeting victims in Europe, Southeast Asia, Korea, and North America simultaneously.

---

## What I Cannot Determine Alone

I want to be clear about the limits of this investigation:

- The real server IP behind Cloudflare requires a Cloudflare subpoena
- The Alibaba Cloud OSS account owner requires Alibaba cooperation with law enforcement
- The exchange account linked to the final deposit addresses requires exchange cooperation
- The full money flow requires professional blockchain analytics tools (Chainalysis, TRM Labs level)
- The identity of "Professor Stephen Beard" requires investigative resources I do not have

All of these are achievable — but require legal authority or professional tooling.

---

## Reports Filed

The following reports have been submitted as of May 2026:

**Exchanges (all 9 with specific transaction evidence):**
Binance · Bybit · OKX · Gate.io · Bitget · MEXC · KuCoin · HTX · Bitfinex

**Infrastructure:**
- Alibaba Cloud abuse — Case **#20260502710120867026** (`infosec@service.aliyun.com`)
- Domain abuse — `DomainAbuse@service.aliyun.com`

**Law enforcement and databases:**
- FBI IC3: `https://ic3.gov`
- Vietnam NCSC: `https://canhbao.ncsc.gov.vn`
- Chainabuse: `https://www.chainabuse.com`
- Global Anti-Scam Organization: `https://www.globalantiscam.org`

---

## What Can Be Done

**If you have been targeted or victimized:**
- Report to Chainabuse: `https://www.chainabuse.com`
- Report to Global Anti-Scam Organization: `https://www.globalantiscam.org`
- Report to your national cybercrime unit
- If Vietnamese: Ministry of Public Security cybercrime division
- If US-based: FBI IC3 at `https://ic3.gov`

**If you are a researcher or journalist:**
I have documented API endpoint maps, infrastructure fingerprints, promotional materials, organizational structure documents, JavaScript deobfuscation output (68,098 decoded strings), and full on-chain transaction traces to 7 exchanges. I am willing to share the complete technical documentation with credentialed researchers, journalists, or law enforcement.

**If you are in law enforcement or a blockchain analytics firm:**
The backend domains `api.ddjea.com`, `api.ddjeb.com`, `api.dsjhout.com` are the persistent infrastructure anchors. The Alibaba Cloud OSS bucket in `oss-cn-hangzhou` contains KYC documents from victims. The on-chain aggregator wallet addresses are identifiable through the Tronscan traces mapped below. The `P2.9.3` kit version string and `yongxuDemo` internal identifier may link to other deployments of the same platform.

---

## Final Note

This post exists because I believe sunlight is the most effective disinfectant for operations like this. Formal reports disappear into queues. Public documentation attracts researchers, journalists, and investigators who have the tools to go further than I can alone.

If you know more about this operation, have been affected by it, or have investigative resources that could help — please reach out.

The people running BG Wealth Sharing have stolen from tens of thousands of victims across multiple countries, operated for years without consequence, and built infrastructure specifically designed to make them untraceable to individuals. They are not untraceable to a coordinated effort.

---

*This investigation was conducted using open source tools, public blockchain data, and standard web development techniques. No systems were compromised. All API probing was done using a valid session token from my own account on the platform.*

*Technical documentation available upon request to verified researchers and law enforcement.*

---

## On-Chain Evidence — The Numbers

*The following data was obtained through direct blockchain tracing using TronScan's public API.*

### Victim Deposit Flow (On-Chain Verification)

Deposit address `TMU5v2zLvYeouqyQKtG1AA3AFRSFUdxTLb` shows the platform's standard sweep pattern — funds moved automatically to the master collection wallet within minutes of deposit, with no manual step. This is consistent with the automated sweeping behavior observed across all 101,902 traced transactions on the same master wallet.

### The Master Collection Wallet

`TWF1bcuNLXtwQCRxb6Qn6uKqWCuXRatiei`

- Incoming transfers: **101,902 transactions**
- Total received: **~$1,000,000,000+ USDT**
- Active: October 2025 – May 2026

101,902 incoming transactions means approximately 100,000 victims, each depositing around $1,000. This single wallet is the aggregation point for the entire victim pool.

### Fund Flow Architecture

Traced transaction analysis across 15 master wallets and 450+ confirmed transactions reveals two distinct laundering patterns:

**Pattern A (simple):**
```
Master Wallet → [1–2 Intermediate Wallets] → Exchange Hot Wallet
```

**Pattern B (consolidated):**
```
Master Wallet → [1–2 Intermediate Wallets] → Consolidation Master
             → [1–2 Intermediate Wallets] → Exchange Hot Wallet
```

In Pattern B, multiple smaller master wallets feed into a central consolidation master before reaching exchanges. This adds an extra layering step specifically designed to break chain-specific tracing.

**Confirmed intermediate wallet example:**
`TPcgRgCbc34BoFgLG4QSy4S9PYcFAe63W4` — 24 confirmed transactions to Binance Hot 7, totaling $87,689 USDT. Sample transaction:
`6698a824c83d51c6333d2c50e63de1c6994a4018d5200c50d9f31e53a5f048e8`

### Final Cashout — 9 Exchanges, 15 Wallets

The laundered funds ultimately reach these regulated exchange hot wallets:

| Exchange | Label | Address | Approx Balance |
|---|---|---|---|
| Binance | Hot 7 | TDqSquXBgUCLYvYC4XZgrprLK589dkhSCf | $455,686,673 |
| Binance | Hot 6 | TJDENsfBJs4RFETt1X1W8wMDc8M5XnJhCe | $65,268,735 |
| Binance | Hot 5 | TAzsQ9Gx8eqFNFSKbeXrbi45CuVPHzA8wr | $64,459,689 |
| Binance | Hot 11 | TJqwA7SoZnERE4zW5uDEiPkbz4B66h9TFj | $46,506,000 |
| Binance | Hot 4 | TNXoiAJ3dct8Fjg4M9fkLFh9S2v9TXc32G | $45,189,581 |
| Binance | Hot 3 | TYASr5UV6HEcXatwdFQfmLVUqQQQMUxHLS | $65,160,131 |
| Bybit | Hot | TU4vEruvZwLLkSfV9bNw12EJTPvNr7Pvaa | $304,996,832 |
| Gate.io | Hot | TBA6CypYJizwA9XdC7Ubgc5F1bxrQ7SqPt | $235,865,678 |
| OKX | Hot 8 | TLaGjwhvA8XQYSxFAcAXy7Dvuue9eGYitv | $159,892,199 |
| OKX | Hot 1 | TM1zzNDZD2DPASbKcgdVoTYhfmYgtfwx9R | $234,505 |
| Bitget | Hot 9 | TJ7hhYhVhaxNx6BPyq7yFpqZrQULL3JSdb | $58,078,026 |
| MEXC | Hot | TEPSrSYPDSQ7yXpMFPq91Fb1QEWpMkRGfn | $77,353,677 |
| KuCoin | Hot 4 | TUpHuDkiCCmwaTZBHZvQdwWzGNm5t8J2b9 | $34,850,867 |
| HTX | Hot 4 | TFTWNgDBkQ5wQoP8RXpRznnHvAVV8x5jLu | $5,812,996 |
| Bitfinex | Hot | TXFBqBbqJommqZf7BV8NNYzePh97UmJodJ | $18,861,054 |

**Note:** These are exchange hot wallets — shared infrastructure serving millions of users. The scammers' personal deposit addresses sit one hop before these. The exchanges have KYC records for those personal accounts. Reports with specific transaction evidence have been filed with all 9 exchanges.

All wallet addresses and transaction flows are publicly verifiable on [TronScan](https://tronscan.org).

### 15 Master Wallets Identified

Beyond the primary master wallet, tracing identified 14 additional collection wallets in the same network:

`TFBXCAz2uoFUchiYxDCWA8NmKxwCmMJNwg` · `TLHn4hvwcRcbT8T5zz4icZSYP9ycKQeMya` · `TX9MCjMWpJpm4qRCpbbbwWXYGwBab1Xp7a` · `TTDkfRZ83VfXW4M3ayf6QSrteH6EkG3ZVn` · `TPiDX9JrcHSxh9ekZCxEcAAR4MGMWYFoft` · `TBPQSHyoRprSADr9uDJzzYFUse6r5fUFvZ` · `TVu2uAeZfixfSQtKZeh4ztS8Y9zszHo8MZ` · `TRDjFwWXN9XjPxeBEwXjABncKzdqSuLxsa` · `TF1n6ykE17g8M9apkRgx3yJZpMwicPmPmk` · `TK7PTtpEaoXiHdX4qW7gHieKcEJdZNnaPK` · `THhqGS6izRWFWHNM3kaupCsNBDwEmbrUcq` · `TX3hfkdQF3gqr8Vz4DyVneWfBi7HFeGfPT` · `TXSVyoshhSD71vZEbJesK11VMRQN69UP1C` · `TAkZnpPoRX5ttbQbQJTrXNJUBci1XcgGub`

---

*Last updated: May 2026*
