# Alibaba Cloud Abuse Report — Pig Butchering Scam (BG Wealth Sharing / DSJEX)

**Report Type:** Phishing Scam / Financial Fraud  
**Date:** May 2026  
**Status:** Operation still active as of May 2026  

---

## 1. Summary

This report documents an active large-scale cryptocurrency fraud operation
(pig butchering / 杀猪盘) that is using Alibaba Cloud OSS infrastructure
to store stolen victim KYC documents. The platform operates as a fake crypto
exchange called DSJEX under the brand "BG Wealth Sharing."

The operation has defrauded approximately **100,000+ victims** of an estimated
**$1,000,000,000+ USDT** and remains fully active.

---

## 2. Infrastructure Domains

| Domain | Role |
|---|---|
| api.ddjea.com | PRIMARY backend API |
| api.ddjeb.com | SECONDARY backup backend |
| api.dsjhout.com | THIRD backend domain |
| ds.dsjhout.com | Media / file server |

Frontend domains rotate daily (pattern: dspp[n].tw) — the backend
domains above are the persistent infrastructure anchors.

---

## 3. Alibaba Cloud OSS Evidence

The following strings were extracted directly from the platform's frontend
JavaScript through deobfuscation of 18 JS files (68,098 total strings decoded):

### Region and Endpoint
- **OSS Region:** `oss-cn-hangzhou` — Hangzhou, China data center
- **Endpoint pattern:** `vpc100-oss-cn-[region].aliyuncs.com`
- **Internal endpoint:** `-internal.aliyuncs.com`

### Authentication
- **Method:** STS temporary tokens
- **Header:** `x-oss-security-token`
- **Token refresh:** `refreshSTSTokenInterval` — auto-refreshing client-side
- **Access key field:** `OSSAccessKeyId`
- **Credential header:** `x-oss-credential`

### Signing
- **Algorithm:** `OSS4-HMAC-SHA256`
- **Version:** `aliyun_v4` / `aliyun_v4_request`

### SDK
- **Package:** `ali-oss` npm package embedded in frontend JS
- **ARN format:** `acs:oss:::` confirms genuine Alibaba Cloud usage

### Upload Endpoint
- `/api/app/upload/s3/pre-signed` — issues per-session OSS credentials
- Bucket name is dynamically issued per session (not hardcoded in JS)

### OSS Headers Found in JS
```
x-oss-security-token
x-oss-credential
x-oss-signature
x-oss-callback
x-oss-callback-var
x-oss-meta-
x-oss-object-acl
x-oss-copy-source
x-oss-date
x-oss-expires
x-oss-process
x-oss-request-id
x-oss-user-agent
x-oss-worm-id
x-oss-err
x-oss-next-append-position
x-oss-storage-class
x-oss-server-side-encryption
x-oss-symlink-target
```

### Alibaba SDK Warning (found verbatim in JS)
> "Please use STS Token for safety, see more details at
> https://help.aliyun.com/document_detail/32077.html"

This confirms the genuine Alibaba OSS SDK is in use, not a spoofed implementation.

---

## 4. What Is Stored in the OSS Bucket

The upload endpoint `/api/app/upload/s3/pre-signed` is called during
victim KYC (identity verification). Documents stored include:

- Government-issued ID photos (front and back)
- Selfie photos holding ID documents
- Personal information submitted during fake verification process

These documents belong to fraud victims and constitute sensitive personal
data that should be preserved as legal evidence.

---

## 5. Victim On-Chain Evidence

- **Victim deposit address:** `TMU5v2zLvYeouqyQKtG1AA3AFRSFUdxTLb`
- **TronScan:** https://tronscan.org/#/address/TMU5v2zLvYeouqyQKtG1AA3AFRSFUdxTLb
- **Master collection wallet:** `TWF1bcuNLXtwQCRxb6Qn6uKqWCuXRatiei`
- **Master wallet TronScan:** https://tronscan.org/#/address/TWF1bcuNLXtwQCRxb6Qn6uKqWCuXRatiei
- **Incoming transactions:** 101,902
- **Total received:** ~$1,000,000,000+ USDT
- **Sweep pattern:** Victim funds moved to master wallet within minutes of deposit

---

## 6. Technical Infrastructure Summary

| Component | Detail |
|---|---|
| Backend framework | Java Spring Boot |
| Frontend framework | Vue.js + Webpack |
| CDN / Proxy | Cloudflare |
| File storage | Alibaba Cloud OSS (oss-cn-hangzhou) |
| Communication | BonChat self-hosted encrypted messaging |
| Obfuscation | javascript-obfuscator with custom base64 encoding |
| Kit version | P2.9.3 (in all request headers) |
| API endpoints | 141 fully mapped |
| WebSocket feeds | wss://[domain]/ws/app/perpetual, /ws/app/chat/, /ws/sub-quotes |

---

## 7. Unique Codebase Fingerprints

These identifiers are unique to this scam kit and can be used to identify
other deployments using the same infrastructure:

- **`yongxuDemo`** — hardcoded test account username
- **`P2.9.3`** — kit version in all HTTP request headers
- **`pcchain.wxpass.net`** — internal test domain in plain text
- **`xchain.wxtome.link`** — linked domain, possibly another operation
- **`app-version: P2.9.3`** — custom HTTP header sent with every API request

---

## 8. Scale of Operation

- Targeting victims in 14+ languages: English, Traditional Chinese,
  Vietnamese, Korean, German, French, Polish, Lithuanian, Indonesian,
  Hungarian, Portuguese, Spanish, Italian, and more
- 12-level MLM pyramid structure with defined salary tiers
- Weekly Zoom meetings with hundreds of global attendees
- Offline recruitment events across multiple countries
- Automated bot system maintaining fake community activity 24/7
- Demo account system to show fake profits before real deposits

---

## 9. Requests to Alibaba Cloud

1. Identify the OSS bucket and account owner receiving uploads from `api.ddjea.com`
2. Preserve all victim KYC documents stored in the OSS bucket as legal evidence
3. Suspend / freeze the OSS account and all associated services
4. Share account registration details, payment records, and IP logs
   with relevant law enforcement agencies
5. Identify and suspend services for related domains:
   `api.ddjeb.com`, `api.dsjhout.com`, `ds.dsjhout.com`
6. Provide a case reference number for follow-up

---

## 10. Other Reports Filed

This case has also been reported to:
- Binance, Bybit, OKX, Gate.io, Bitget, MEXC, KuCoin, HTX, Bitfinex
- Chainabuse: https://www.chainabuse.com
- Global Anti-Scam Organization: https://www.globalantiscam.org
- Vietnam NCSC: https://canhbao.ncsc.gov.vn
- FBI IC3: https://ic3.gov

---

## 11. Attached Evidence Files

- `Alibaba_Cloud_Fraud_Report.xlsx` — structured evidence including all
  OSS strings, 141 API endpoints, and infrastructure details
- `MASTER_REPORT.json` — raw output from JS deobfuscation
  (68,098 strings decoded from 18 JavaScript files)

---

*This report was prepared by an independent security researcher and
fraud victim. All blockchain data is publicly verifiable on TronScan.
All JavaScript strings were extracted through standard deobfuscation
of publicly served frontend code.*
