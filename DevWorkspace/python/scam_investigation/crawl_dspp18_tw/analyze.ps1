# ─────────────────────────────────────────────────────────────
#  analyze.ps1  -  Static analysis on downloaded JS files
#  Run from inside the crawl_dspp18_tw folder:
#      .\analyze.ps1
#  Or point it at another folder:
#      .\analyze.ps1 -JsDir "C:\path\to\js"
# ─────────────────────────────────────────────────────────────

param(
    [string]$JsDir = "js"
)

# ── Colors ────────────────────────────────────────────────────
function Green($m)  { Write-Host "  +  $m" -ForegroundColor Green }
function Cyan($m)   { Write-Host "  .  $m" -ForegroundColor Cyan }
function Yellow($m) { Write-Host "  !  $m" -ForegroundColor Yellow }
function Magenta($m){ Write-Host "  *  $m" -ForegroundColor Magenta }
function Red($m)    { Write-Host "  x  $m" -ForegroundColor Red }
function Header($m) { Write-Host $m -ForegroundColor Cyan }

# ── Verify folder exists ──────────────────────────────────────
if (-not (Test-Path $JsDir)) {
    Red "Folder not found: $JsDir"
    Red "Run this script from inside your crawl_xxxxx folder."
    exit 1
}

$files = Get-ChildItem -Path $JsDir -Filter "*.js" -File
if ($files.Count -eq 0) {
    Red "No .js files found in: $JsDir"
    exit 1
}

Header ""
Header "============================================================"
Header "  JS folder : $JsDir"
Header "  Files     : $($files.Count)"
Header "============================================================"

# ── Search helper ─────────────────────────────────────────────
function Search {
    param(
        [string]$Label,
        [string]$Pattern,
        [string]$Color = "Magenta"
    )

    Write-Host ""
    Write-Host "[ $Label ]" -ForegroundColor Yellow

    $results = $files | Select-String -Pattern $Pattern -CaseSensitive:$false

    if ($results.Count -eq 0) {
        Write-Host "  -- none found" -ForegroundColor DarkGray
        return
    }

    # Deduplicate by the matched line content
    $seen = @{}
    foreach ($r in $results) {
        $line = $r.Line.Trim()
        # truncate very long lines
        if ($line.Length -gt 200) { $line = $line.Substring(0, 200) + "..." }
        $key = "$($r.Filename):$($r.LineNumber)"
        if (-not $seen.ContainsKey($key)) {
            $seen[$key] = $true
            Write-Host ("  [{0}:{1}]  {2}" -f $r.Filename, $r.LineNumber, $line) -ForegroundColor $Color
        }
    }
    Green "$($seen.Count) match(es) found"
}

# ── Run searches ──────────────────────────────────────────────

Search -Label "Base URL / axios.create"    -Pattern "baseURL|baseUrl|BASE_URL|axios\.create"
Search -Label "API Hosts / Domain config"  -Pattern "https?://[a-zA-Z0-9\-\.]+\.(com|tw|io|net|cc|top|xyz|vip)/api"
Search -Label "Login / Auth endpoints"     -Pattern "login|sign.?in|logout|register|sign.?up" -Color "Yellow"
Search -Label "Token / Authorization"      -Pattern "Authorization|Bearer|access.?token|refresh.?token" -Color "Yellow"
Search -Label "AWS S3 / File upload"       -Pattern "s3\.|amazonaws|bucket|pre.?signed|upload"
Search -Label "WebSocket connections"      -Pattern "new WebSocket|wss?://"
Search -Label "Crypto wallet methods"      -Pattern "getAddress|sendTransaction|signTransaction|privateKey|mnemonic|eth_sendRawTransaction"
Search -Label "Hardcoded secrets"          -Pattern "secret|api.?key|password|passwd|private.?key" -Color "Red"
Search -Label "Admin / internal routes"    -Pattern "/admin|/manage|/dashboard|/internal|/backend" -Color "Red"
Search -Label "Environment / config vars"  -Pattern "process\.env|VUE_APP_|VITE_|NODE_ENV"

# ── Save results to file ──────────────────────────────────────
Write-Host ""
Header "============================================================"
$outFile = "analysis_results.txt"
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

$allResults = @()
$allResults += "Analysis run: $timestamp"
$allResults += "JS folder: $JsDir"
$allResults += ""

$searches = @(
    @{ Label="Base URL / axios.create";   Pattern="baseURL|baseUrl|BASE_URL|axios\.create" },
    @{ Label="API Hosts";                 Pattern="https?://[a-zA-Z0-9\-\.]+\.(com|tw|io|net|cc|top|xyz|vip)/api" },
    @{ Label="Login / Auth";              Pattern="login|sign.?in|logout|register" },
    @{ Label="Token / Authorization";     Pattern="Authorization|Bearer|access.?token|refresh.?token" },
    @{ Label="AWS S3 / Upload";           Pattern="s3\.|amazonaws|bucket|pre.?signed|upload" },
    @{ Label="WebSocket";                 Pattern="new WebSocket|wss?://" },
    @{ Label="Crypto wallet methods";     Pattern="getAddress|sendTransaction|signTransaction|privateKey|mnemonic" },
    @{ Label="Hardcoded secrets";         Pattern="secret|api.?key|password|passwd|private.?key" },
    @{ Label="Admin routes";              Pattern="/admin|/manage|/dashboard|/internal|/backend" },
    @{ Label="Env / config vars";         Pattern="process\.env|VUE_APP_|VITE_|NODE_ENV" }
)

foreach ($s in $searches) {
    $allResults += "=== $($s.Label) ==="
    $matches = $files | Select-String -Pattern $s.Pattern -CaseSensitive:$false
    if ($matches.Count -eq 0) {
        $allResults += "  -- none found"
    } else {
        $seen = @{}
        foreach ($r in $matches) {
            $key = "$($r.Filename):$($r.LineNumber)"
            if (-not $seen.ContainsKey($key)) {
                $seen[$key] = $true
                $line = $r.Line.Trim()
                if ($line.Length -gt 300) { $line = $line.Substring(0, 300) + "..." }
                $allResults += "  [$($r.Filename):$($r.LineNumber)]  $line"
            }
        }
    }
    $allResults += ""
}

$allResults | Out-File -FilePath $outFile -Encoding UTF8
Green "Results saved to: $outFile"
Header "============================================================"
Write-Host ""
