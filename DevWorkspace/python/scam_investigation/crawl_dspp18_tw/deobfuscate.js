#!/usr/bin/env node
/**
 * deobfuscate.js
 * Executes the obfuscated JS in a sandboxed Node.js VM,
 * intercepts all string array accesses, and dumps decoded strings.
 *
 * Usage:
 *   node deobfuscate.js <file.js>
 *   node deobfuscate.js js/pc__pcstatic__js__app~a6f663ec.js
 */

const fs   = require('fs');
const path = require('path');
const vm   = require('vm');

const filePath = process.argv[2];
if (!filePath) {
    console.error('Usage: node deobfuscate.js <file.js>');
    process.exit(1);
}

const outDir = 'deob_output';
if (!fs.existsSync(outDir)) fs.mkdirSync(outDir);

const baseName   = path.basename(filePath, '.js');
const outStrings = path.join(outDir, baseName + '_strings.txt');
const outApi     = path.join(outDir, baseName + '_api.txt');
const outReport  = path.join(outDir, baseName + '_report.json');

console.log(`\n${'='.repeat(60)}`);
console.log(`  FILE   : ${filePath}`);
console.log(`  OUTPUT : ${outDir}/`);
console.log(`${'='.repeat(60)}\n`);

// ── Read file ─────────────────────────────────────────────────
let code = fs.readFileSync(filePath, 'utf8');
console.log(`  . Read ${(code.length / 1024).toFixed(0)} KB`);

// ── Step 1: Extract string arrays directly via regex ──────────
// javascript-obfuscator stores strings like: ['str1','str2',...]
// Sometimes base64 or RC4 encoded, sometimes plain

const collected = {
    rawArrayStrings:  new Set(),
    decodedStrings:   new Set(),
    apiEndpoints:     new Set(),
    urls:             new Set(),
    jsonKeys:         new Set(),
    vmStrings:        new Set(),
};

// Extract all single-quoted strings from the file
const singleQuoted = code.matchAll(/'([^'\\]{3,100})'/g);
for (const m of singleQuoted) {
    const s = m[1];
    collected.rawArrayStrings.add(s);

    // Try base64 decode
    try {
        const decoded = Buffer.from(s, 'base64').toString('utf8');
        if (/^[\x20-\x7E]{4,}$/.test(decoded)) {
            collected.decodedStrings.add(decoded);
        }
    } catch {}

    // Collect /api/ paths
    if (s.startsWith('/api/')) collected.apiEndpoints.add(s);

    // Collect URLs
    if (s.startsWith('http')) collected.urls.add(s);

    // Collect camelCase/snake_case field names
    if (/^[a-zA-Z][a-zA-Z0-9_]{2,40}$/.test(s) && !/^[A-Z0-9_]{5,}$/.test(s)) {
        collected.jsonKeys.add(s);
    }
}

// Also double-quoted
const doubleQuoted = code.matchAll(/"([^"\\]{3,100})"/g);
for (const m of doubleQuoted) {
    const s = m[1];
    if (s.startsWith('/api/')) collected.apiEndpoints.add(s);
    if (s.startsWith('http')) collected.urls.add(s);
    if (/^[a-zA-Z][a-zA-Z0-9_]{2,40}$/.test(s)) collected.jsonKeys.add(s);
}

console.log(`  . Static extraction: ${collected.rawArrayStrings.size} raw strings, ${collected.apiEndpoints.size} API paths`);

// ── Step 2: VM execution to decode string arrays ──────────────
console.log(`  . Running VM sandbox to decode string arrays...`);

// Build a fake browser-like sandbox
const intercepted = new Set();
const sandbox = {
    window:    {},
    document:  { createElement: () => ({}), getElementById: () => null },
    navigator: { userAgent: 'Mozilla/5.0' },
    location:  { href: 'https://dspp18.tw/', hostname: 'dspp18.tw' },
    console:   { log: () => {}, warn: () => {}, error: () => {} },
    setTimeout:  (fn) => { try { fn(); } catch {} },
    setInterval: () => {},
    clearTimeout: () => {},
    self: {},
    // Intercept any string that gets pushed to an array or returned
    __intercepted: intercepted,
};
sandbox.window   = sandbox;
sandbox.self     = sandbox;
sandbox.globalThis = sandbox;

// We only execute the FIRST ~50KB of each file — that's where
// the string array and decoder functions always live in javascript-obfuscator output
const bootCode = code.substring(0, Math.min(80000, code.length));

// Wrap in try/catch so errors don't stop us
const wrapped = `
(function() {
  try {
    ${bootCode}
  } catch(e) {}
})();
`;

try {
    const ctx = vm.createContext(sandbox);
    vm.runInContext(wrapped, ctx, {
        timeout:     5000,
        filename:    baseName,
        displayErrors: false,
    });

    // After execution, scan sandbox for any arrays of strings that appeared
    function scanObj(obj, depth) {
        if (depth > 4) return;
        if (!obj || typeof obj !== 'object') return;
        for (const key of Object.keys(obj)) {
            try {
                const val = obj[key];
                if (Array.isArray(val)) {
                    val.forEach(item => {
                        if (typeof item === 'string' && item.length >= 3) {
                            collected.vmStrings.add(item);
                            if (item.startsWith('/api/')) collected.apiEndpoints.add(item);
                            if (item.startsWith('http'))  collected.urls.add(item);
                        }
                    });
                } else if (typeof val === 'object') {
                    scanObj(val, depth + 1);
                }
            } catch {}
        }
    }
    scanObj(sandbox, 0);
    console.log(`  . VM extracted ${collected.vmStrings.size} strings from sandbox`);

} catch (e) {
    console.log(`  ! VM execution failed (expected for heavily obfuscated files): ${e.message}`);
}

// ── Step 3: Pattern-based extraction for specific data ────────
console.log(`  . Running pattern extraction...`);

// Find axios/fetch request patterns and nearby field names
const apiContexts = [];
const apiPattern  = /['"]\/api\/([^'"]{3,80})['"]/g;
let   m;

while ((m = apiPattern.exec(code)) !== null) {
    const ep    = '/' + m[1].split('?')[0]; // strip query
    const start = Math.max(0, m.index - 300);
    const end   = Math.min(code.length, m.index + 300);
    const ctx   = code.substring(start, end);

    // Extract field names near this endpoint
    const fields = [];
    const fieldPat = /['"]([a-zA-Z][a-zA-Z0-9_]{2,30})['"]\s*:/g;
    let   fm;
    while ((fm = fieldPat.exec(ctx)) !== null) {
        const f = fm[1];
        if (!['true','false','null','use strict','function'].includes(f)) {
            fields.push(f);
        }
    }

    if (fields.length > 0) {
        apiContexts.push({ endpoint: ep, fields: [...new Set(fields)] });
    }
    collected.apiEndpoints.add(ep);
}

// ── Step 4: Filter meaningful strings ─────────────────────────
const allVmStrings = [...collected.vmStrings, ...collected.decodedStrings]
    .filter(s => /^[\x20-\x7E]{3,}$/.test(s))  // printable ASCII only
    .filter(s => s.length >= 3 && s.length <= 120)
    .filter(s => !/^[0-9a-f]{8,}$/i.test(s))   // no pure hex
    .filter(s => !/^[A-Z0-9+/]{20,}=*$/.test(s)) // no raw base64
    .sort();

// Categorize
const meaningful = {
    apiPaths:     [...collected.apiEndpoints].filter(s => s.startsWith('/api')).sort(),
    urls:         [...collected.urls].filter(s => s.startsWith('http') && !s.includes('ethereum') && !s.includes('github')).sort(),
    fieldNames:   [...collected.jsonKeys].filter(s => /^[a-z][a-zA-Z0-9_]{2,30}$/.test(s)).sort(),
    rawStrings:   allVmStrings,
    endpointMap:  apiContexts,
};

// ── Step 5: Print results ─────────────────────────────────────
console.log(`\n${'='.repeat(60)}`);
console.log(`  RESULTS`);
console.log(`${'='.repeat(60)}`);

console.log(`\n  [ API Paths ] (${meaningful.apiPaths.length} found)`);
meaningful.apiPaths.forEach(s => console.log(`    * ${s}`));

console.log(`\n  [ URLs ] (${meaningful.urls.length} found)`);
meaningful.urls.forEach(s => console.log(`    * ${s}`));

console.log(`\n  [ Endpoint + Fields ] (${meaningful.endpointMap.length} found)`);
meaningful.endpointMap.forEach(e => {
    console.log(`    * ${e.endpoint}`);
    console.log(`      fields: ${e.fields.join(', ')}`);
});

console.log(`\n  [ Field Names ] (${meaningful.fieldNames.length} found)`);
const interestingFields = meaningful.fieldNames.filter(f =>
    /id$|Id$|num$|Num$|size$|Size$|type$|Type$|symbol|page|coin|amount|address|period|code|phone|email|name|level|sort|status|time|start|end|from|to|limit|key|token|sign/i.test(f)
);
interestingFields.forEach(f => console.log(`    * ${f}`));

// ── Step 6: Save all output ───────────────────────────────────
fs.writeFileSync(outStrings, allVmStrings.join('\n'), 'utf8');
fs.writeFileSync(outApi,     meaningful.apiPaths.join('\n'), 'utf8');
fs.writeFileSync(outReport,  JSON.stringify(meaningful, null, 2), 'utf8');

console.log(`\n${'='.repeat(60)}`);
console.log(`  Saved strings   -> ${outStrings}`);
console.log(`  Saved API paths -> ${outApi}`);
console.log(`  Saved report    -> ${outReport}`);
console.log(`${'='.repeat(60)}\n`);
