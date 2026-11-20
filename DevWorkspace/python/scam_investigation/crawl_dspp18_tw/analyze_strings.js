#!/usr/bin/env node
/**
 * analyze_strings.js
 * Analyzes the 67k decoded strings dumped from browser memory.
 *
 * Usage:
 *   node analyze_strings.js decoded_strings.json
 */

const fs   = require('fs');
const path = require('path');

const inputFile = process.argv[2] || 'decoded_strings.json';
const outDir    = 'deob_output';
if (!fs.existsSync(outDir)) fs.mkdirSync(outDir);

console.log(`\n${'='.repeat(60)}`);
console.log(`  DECODED STRING ANALYZER`);
console.log(`  Input : ${inputFile}`);
console.log(`${'='.repeat(60)}\n`);

// ── Load strings ──────────────────────────────────────────────
let strings;
try {
    strings = JSON.parse(fs.readFileSync(inputFile, 'utf8'));
    console.log(`  . Loaded ${strings.length} strings`);
} catch (e) {
    console.error(`  x Failed to load ${inputFile}: ${e.message}`);
    process.exit(1);
}

// ── Categorize ────────────────────────────────────────────────
const categories = {
    api_endpoints:    [],
    urls:             [],
    aws_credentials:  [],
    crypto_wallets:   [],
    secrets_tokens:   [],
    emails:           [],
    domains:          [],
    ip_addresses:     [],
    phone_numbers:    [],
    json_field_names: [],
    languages:        [],
    error_messages:   [],
    interesting:      [],
};

const seen = new Set();

for (const raw of strings) {
    if (typeof raw !== 'string') continue;
    const s = raw.trim();
    if (!s || s.length < 2 || seen.has(s)) continue;
    seen.add(s);

    // API endpoints
    if (/^\/api\//.test(s))
        categories.api_endpoints.push(s);

    // Full URLs
    else if (/^https?:\/\//.test(s) && !s.includes('ethereum') && !s.includes('github') && !s.includes('eips'))
        categories.urls.push(s);

    // AWS
    else if (/^(AKIA|ASIA)[A-Z0-9]{16}$/.test(s))
        categories.aws_credentials.push({ type: 'AccessKeyId', value: s });
    else if (/accessKey|secretKey|stsToken|bucketName|oss-cn|amazonaws\.com/.test(s))
        categories.aws_credentials.push({ type: 'aws_related', value: s });

    // Crypto wallets
    else if (/^0x[a-fA-F0-9]{40}$/.test(s) && s !== '0x0000000000000000000000000000000000000000')
        categories.crypto_wallets.push({ type: 'ETH', value: s });
    else if (/^T[A-Za-z1-9]{33}$/.test(s))
        categories.crypto_wallets.push({ type: 'TRC20', value: s });
    else if (/^[13][a-km-zA-HJ-NP-Z1-9]{25,34}$/.test(s))
        categories.crypto_wallets.push({ type: 'BTC', value: s });
    else if (/^bc1[a-zA-HJ-NP-Z0-9]{6,87}$/.test(s))
        categories.crypto_wallets.push({ type: 'BTC_bech32', value: s });

    // Secrets / tokens
    else if (/^(eyJ[a-zA-Z0-9_-]{10,})/.test(s))
        categories.secrets_tokens.push({ type: 'JWT', value: s });
    else if (/^[a-f0-9]{32}$/.test(s))
        categories.secrets_tokens.push({ type: 'MD5/token', value: s });
    else if (/secret|apikey|api_key|accesssecret/i.test(s) && s.length < 80)
        categories.secrets_tokens.push({ type: 'secret_ref', value: s });

    // Emails
    else if (/^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$/.test(s) &&
             !s.includes('ethdisco') && !s.includes('feross'))
        categories.emails.push(s);

    // IPs
    else if (/^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}(:\d+)?$/.test(s))
        categories.ip_addresses.push(s);

    // Domains (not the scam site itself)
    else if (/^[a-zA-Z0-9\-\.]+\.(com|io|net|org|tw|cc|top|xyz|vip|club)$/.test(s) &&
             !s.includes('dspp') && !s.includes('ddjea'))
        categories.domains.push(s);

    // Error messages (reveals backend logic)
    else if (s.length > 10 && s.length < 100 &&
             /error|fail|invalid|expired|forbidden|unauthorized|insufficient|wrong|incorrect/i.test(s))
        categories.error_messages.push(s);

    // JSON field names (camelCase 3-40 chars)
    else if (/^[a-z][a-zA-Z0-9]{2,39}$/.test(s) && !/^(true|false|null|undefined|function|return)$/.test(s))
        categories.json_field_names.push(s);
}

// ── Print results ─────────────────────────────────────────────
function section(title, items, limit = 100) {
    console.log(`\n${'─'.repeat(60)}`);
    console.log(`  ${title}  (${items.length})`);
    console.log('─'.repeat(60));
    const show = items.slice(0, limit);
    show.forEach(item => {
        if (typeof item === 'object')
            console.log(`  [${item.type}]  ${item.value}`);
        else
            console.log(`  ${item}`);
    });
    if (items.length > limit) console.log(`  ... and ${items.length - limit} more`);
}

section('🌐 API ENDPOINTS', categories.api_endpoints, 200);
section('🔗 URLS', categories.urls, 50);
section('🔑 AWS CREDENTIALS / S3', categories.aws_credentials, 50);
section('💰 CRYPTO WALLET ADDRESSES', categories.crypto_wallets, 50);
section('🔐 SECRETS / TOKENS / JWTs', categories.secrets_tokens, 30);
section('📧 EMAILS', categories.emails, 30);
section('🖥  IP ADDRESSES', categories.ip_addresses, 30);
section('🌍 DOMAINS', categories.domains, 50);
section('⚠️  ERROR MESSAGES', categories.error_messages, 50);

// ── Build API endpoint → field name map ───────────────────────
console.log(`\n${'─'.repeat(60)}`);
console.log(`  📋 API ENDPOINT CONTEXT MAP`);
console.log('─'.repeat(60));
console.log('  (endpoints found near request field names in string order)');

// Group field names that appear close to API paths in the original arrays
const apiPaths   = categories.api_endpoints;
const fieldNames = new Set(categories.json_field_names);

// Find field names that are likely request params based on known patterns
const requestFields = categories.json_field_names.filter(f =>
    /^(page|coin|symbol|amount|address|type|status|id|from|to|start|end|time|limit|size|num|code|phone|email|name|period|order|sort|key|token|sign|level|lang|currency|wallet)/i.test(f)
);

console.log(`\n  Likely request body field names (${requestFields.length}):`);
requestFields.sort().forEach(f => console.log(`  * ${f}`));

// ── Save everything ───────────────────────────────────────────
const report = {
    summary: {
        total_input:     strings.length,
        unique_strings:  seen.size,
        api_endpoints:   categories.api_endpoints.length,
        crypto_wallets:  categories.crypto_wallets.length,
        aws_items:       categories.aws_credentials.length,
        emails:          categories.emails.length,
        ip_addresses:    categories.ip_addresses.length,
    },
    categories,
    request_field_names: requestFields.sort(),
};

const outFile = path.join(outDir, 'decoded_analysis.json');
fs.writeFileSync(outFile, JSON.stringify(report, null, 2), 'utf8');

// Also save flat files for easy grepping
fs.writeFileSync(path.join(outDir, 'decoded_api_endpoints.txt'),
    categories.api_endpoints.join('\n'), 'utf8');
fs.writeFileSync(path.join(outDir, 'decoded_wallets.txt'),
    categories.crypto_wallets.map(w => `[${w.type}] ${w.value}`).join('\n'), 'utf8');
fs.writeFileSync(path.join(outDir, 'decoded_urls.txt'),
    categories.urls.join('\n'), 'utf8');

console.log(`\n${'='.repeat(60)}`);
console.log(`  Saved full report  -> ${outFile}`);
console.log(`  Saved API paths    -> ${outDir}/decoded_api_endpoints.txt`);
console.log(`  Saved wallets      -> ${outDir}/decoded_wallets.txt`);
console.log(`  Saved URLs         -> ${outDir}/decoded_urls.txt`);
console.log(`${'='.repeat(60)}\n`);
