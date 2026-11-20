# ─────────────────────────────────────────────────────────────
#  deobfuscate_hints.ps1
#  Extracts readable strings and API clues from obfuscated JS
#  without needing a full deobfuscator.
#
#  Run from inside crawl_dspp18_tw folder:
#      .\deobfuscate_hints.ps1
# ─────────────────────────────────────────────────────────────

param(
    [string]$JsDir = "js"
)

$outDir = "deob_output"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

function Green($m)   { Write-Host "  +  $m" -ForegroundColor Green }
function Cyan($m)    { Write-Host "  .  $m" -ForegroundColor Cyan }
function Yellow($m)  { Write-Host "  !  $m" -ForegroundColor Yellow }
function Magenta($m) { Write-Host "  *  $m" -ForegroundColor Magenta }
function Red($m)     { Write-Host "  x  $m" -ForegroundColor Red }

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  DEOBFUSCATION HINTS EXTRACTOR" -ForegroundColor Cyan
Write-Host "  JS folder : $JsDir" -ForegroundColor Cyan
Write-Host "  Output    : $outDir\" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

$files = Get-ChildItem -Path $JsDir -Filter "*__app~*.js" -File |
         Sort-Object Length -Descending

Cyan "Processing $($files.Count) js files (largest first)..."

$allStrings   = [System.Collections.Generic.List[string]]::new()
$allEndpoints = [System.Collections.Generic.List[string]]::new()
$allUrls      = [System.Collections.Generic.List[string]]::new()
$allKeys      = [System.Collections.Generic.List[string]]::new()

foreach ($file in $files) {
    Cyan "Reading $($file.Name)  ($([math]::Round($file.Length/1KB)) KB)"
    $content = Get-Content $file.FullName -Raw

    # ── Extract all quoted strings longer than 4 chars ────────
    $strings = [regex]::Matches($content, """([^""\\]{4,80})""") |
               ForEach-Object { $_.Groups[1].Value } |
               Where-Object {
                   # Filter out hex garbage and pure numbers
                   $_ -notmatch '^[0-9a-f]+$' -and
                   $_ -notmatch '^0x[0-9a-fA-F]+$' -and
                   $_ -notmatch '^[\d\.\-]+$' -and
                   $_ -notmatch '^[A-Z0-9]{20,}$'  # encoded garbage
               } |
               Sort-Object -Unique

    foreach ($s in $strings) { $allStrings.Add($s) }

    # ── Extract /api/ paths ───────────────────────────────────
    $eps = [regex]::Matches($content, '"/api/[^"]{3,80}"') |
           ForEach-Object { $_.Value.Trim('"') } |
           Sort-Object -Unique
    foreach ($e in $eps) { $allEndpoints.Add($e) }

    # ── Extract http URLs ─────────────────────────────────────
    $urls = [regex]::Matches($content, '"(https?://[^"]{8,120})"') |
            ForEach-Object { $_.Groups[1].Value } |
            Sort-Object -Unique
    foreach ($u in $urls) { $allUrls.Add($u) }

    # ── Extract JSON key names (request body field names) ─────
    $keys = [regex]::Matches($content, '"([a-zA-Z][a-zA-Z0-9_]{2,30})":\s*["{0-9\[\{]') |
            ForEach-Object { $_.Groups[1].Value } |
            Where-Object { $_ -notmatch '^(true|false|null|undefined)$' } |
            Sort-Object -Unique
    foreach ($k in $keys) { $allKeys.Add($k) }
}

# ── Deduplicate everything ────────────────────────────────────
$allStrings   = $allStrings   | Sort-Object -Unique
$allEndpoints = $allEndpoints | Sort-Object -Unique
$allUrls      = $allUrls      | Sort-Object -Unique
$allKeys      = $allKeys      | Sort-Object -Unique

# ── Save raw string dump ──────────────────────────────────────
$allStrings | Out-File "$outDir\all_strings.txt" -Encoding UTF8
Green "Saved $($allStrings.Count) unique strings -> $outDir\all_strings.txt"

# ── Save endpoints ────────────────────────────────────────────
$allEndpoints | Out-File "$outDir\api_endpoints.txt" -Encoding UTF8
Green "Saved $($allEndpoints.Count) API endpoints -> $outDir\api_endpoints.txt"

# ── Save URLs ─────────────────────────────────────────────────
$allUrls | Out-File "$outDir\urls.txt" -Encoding UTF8
Green "Saved $($allUrls.Count) URLs -> $outDir\urls.txt"

# ── Save JSON keys (request body field names) ─────────────────
$allKeys | Out-File "$outDir\json_keys.txt" -Encoding UTF8
Green "Saved $($allKeys.Count) JSON field names -> $outDir\json_keys.txt"

# ── Now do targeted searches for specific data ────────────────
Write-Host ""
Write-Host "[ Targeted Extraction ]" -ForegroundColor Yellow

# Find strings that look like request body schemas
# by looking for clusters of JSON keys near API paths
$apiFiles = Get-ChildItem -Path $JsDir -Filter "*__app~*.js" -File |
            Sort-Object Length -Descending |
            Select-Object -First 5

$schemaResults = [System.Collections.Generic.List[string]]::new()

foreach ($file in $apiFiles) {
    $content = Get-Content $file.FullName -Raw

    # Find every /api/ path and grab 300 chars around it
    $matches = [regex]::Matches($content, '"/api/[^"]{3,80}"')
    foreach ($m in $matches) {
        $start  = [Math]::Max(0, $m.Index - 150)
        $length = [Math]::Min(500, $content.Length - $start)
        $ctx    = $content.Substring($start, $length)

        # Extract any plain english strings from the context window
        $nearby = [regex]::Matches($ctx, '"([a-zA-Z][a-zA-Z0-9_]{2,40})"') |
                  ForEach-Object { $_.Groups[1].Value } |
                  Where-Object { $_ -notmatch '^(function|return|var|let|const|true|false|GET|POST)$' }

        if ($nearby.Count -gt 2) {
            $line = "$($m.Value)  =>  params: [$($nearby -join ', ')]"
            $schemaResults.Add($line)
        }
    }
}

$schemaResults | Sort-Object -Unique | Out-File "$outDir\endpoint_params.txt" -Encoding UTF8
Green "Saved endpoint+param hints -> $outDir\endpoint_params.txt"

# ── Print the most interesting strings to screen ──────────────
Write-Host ""
Write-Host "[ API Endpoints found in JS ]" -ForegroundColor Magenta
$allEndpoints | ForEach-Object { Magenta $_ }

Write-Host ""
Write-Host "[ URLs found in JS ]" -ForegroundColor Magenta
$allUrls | Where-Object { $_ -notmatch 'ethereum|github|eips' } |
           ForEach-Object { Magenta $_ }

# ── Find strings that look like field names for missing params ─
Write-Host ""
Write-Host "[ Likely request body field names ]" -ForegroundColor Yellow
$interestingKeys = $allKeys | Where-Object {
    $_ -match 'Id$|Num$|Size$|Type$|symbol|page|coin|amount|address|period|code|phone|email|name|level|sort|status|time|start|end|from|to|limit'
}
$interestingKeys | ForEach-Object { Yellow $_ }

Write-Host ""
Write-Host "============================================================" -ForegroundColor Green
Write-Host "  All output saved to: $outDir\" -ForegroundColor Green
Write-Host "  Key files to review:" -ForegroundColor Green
Write-Host "    endpoint_params.txt  <- API paths + nearby param names" -ForegroundColor Green
Write-Host "    json_keys.txt        <- all JSON field names found" -ForegroundColor Green
Write-Host "    all_strings.txt      <- every readable string" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host ""
