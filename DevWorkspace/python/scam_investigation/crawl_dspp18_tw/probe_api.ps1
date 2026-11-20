# ─────────────────────────────────────────────────────────────
#  probe_api.ps1  -  Probe scam site API with correct headers
#  Run from inside the crawl_dspp18_tw folder:
#      .\probe_api.ps1
#
#  To use your own login token (from DevTools Request Headers):
#      .\probe_api.ps1 -Token "your_token_here"
# ─────────────────────────────────────────────────────────────

param(
    [string]$BaseUrl = "https://api.ddjea.com",
    [string]$Token   = "c5ntqg7qv800_1jn2e191u",
    [string]$Origin  = "https://dspp17.tw"
)

# ── Headers extracted directly from browser DevTools ─────────
$headers = @{
    "Accept"               = "application/json, text/plain, */*"
    "Accept-Language"      = "en-US,en;q=0.9,vi;q=0.8"
    "Origin"               = $Origin
    "Referer"              = "$Origin/"
    "request-domain"       = "$Origin/pc/#/home"
    "app-login-token"      = $Token
    "app-version"          = "P2.9.3"
    "app-client-timezone"  = "+8"
    "app-analog"           = "false"
    "aws-check"            = "true"
    "set-aws"              = "true"
    "set-language"         = "ENGLISH"
    "x-requested-with"     = "XMLHttpRequest"
    "User-Agent"           = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36"
}

# ── Endpoints: [path, method, body] ──────────────────────────
$endpoints = @(
    # Public - likely no auth needed
    @{ path="/api/app/now";                             method="POST"; body="{}" },
    @{ path="/api/app/language";                        method="POST"; body="{}" },
    @{ path="/api/app/basis/notice";                    method="POST"; body='{"pageNum":1,"pageSize":10}' },
    @{ path="/api/app/quotes/sort";                     method="POST"; body="{}" },
    @{ path="/api/app/quotes/trade";                    method="POST"; body='{"symbol":"BTCUSDT"}' },
    @{ path="/api/app/second/product";                  method="POST"; body="{}" },
    @{ path="/api/app/second/now/period/profit";        method="POST"; body='{"periodId":1,"symbol":"BTCUSDT"}' },
    @{ path="/api/app/mining/product";                  method="POST"; body="{}" },
    @{ path="/api/app/basis/article/list";              method="POST"; body='{"pageNum":1,"pageSize":10}' },

    # Auth-required - token needed
    @{ path="/api/app/funds/overview";                  method="POST"; body="{}" },
    @{ path="/api/app/funds/wallet/money";              method="POST"; body="{}" },
    @{ path="/api/app/user/withdrawal/password/status"; method="POST"; body="{}" },
    @{ path="/api/app/user/credit/list";                method="POST"; body='{"pageNum":1,"pageSize":10}' },
    @{ path="/api/app/share/team";                      method="POST"; body="{}" },
    @{ path="/api/app/share/my/award";                  method="POST"; body="{}" },
    @{ path="/api/app/second/share/list";               method="POST"; body='{"pageNum":1,"pageSize":10}' },
    @{ path="/api/app/recharge/otc";                    method="POST"; body="{}" },
    @{ path="/api/app/financial/borrow";                method="POST"; body="{}" },
    @{ path="/api/app/c2c/order";                       method="POST"; body='{"pageNum":1,"pageSize":10}' },
    @{ path="/api/app/user/verify/now";                 method="POST"; body="{}" },
    @{ path="/api/app/second/share/user/info/code";     method="POST"; body="{}" }
)

$results  = @()
$logLines = @()

function Green($m)   { Write-Host "  +  $m" -ForegroundColor Green }
function Cyan($m)    { Write-Host "  .  $m" -ForegroundColor Cyan }
function Yellow($m)  { Write-Host "  !  $m" -ForegroundColor Yellow }
function Magenta($m) { Write-Host "  *  $m" -ForegroundColor Magenta }
function Red($m)     { Write-Host "  x  $m" -ForegroundColor Red }

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  API PROBE  (with correct headers)" -ForegroundColor Cyan
Write-Host "  Base URL : $BaseUrl" -ForegroundColor Cyan
Write-Host "  Token    : $Token" -ForegroundColor Cyan
Write-Host "  Origin   : $Origin" -ForegroundColor Cyan
Write-Host "  Endpoints: $($endpoints.Count)" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

foreach ($ep in $endpoints) {
    $url = "$BaseUrl$($ep.path)"
    Cyan "$($ep.method) $url"

    $entry = [PSCustomObject]@{
        endpoint    = $ep.path
        method      = $ep.method
        url         = $url
        status      = $null
        body_sent   = $ep.body
        response    = $null
        interesting = $false
        notes       = ""
    }

    try {
        $resp = Invoke-WebRequest `
            -Uri $url `
            -Method $ep.method `
            -Headers $headers `
            -Body $ep.body `
            -ContentType "application/json;charset=UTF-8" `
            -TimeoutSec 12 `
            -ErrorAction Stop

        $status  = $resp.StatusCode
        $rawBody = $resp.Content
        $entry.status = $status

        # Pretty-print JSON if possible
        try {
            $json   = $rawBody | ConvertFrom-Json
            $pretty = $json | ConvertTo-Json -Depth 8
            $entry.response = $pretty

            $code = $json.code
            if ($code -eq 0 -or $code -eq 200 -or $null -eq $code) {
                $entry.interesting = $true
                $entry.notes = "SUCCESS - real data returned"
            } else {
                $entry.notes = "code=$code  msg=$($json.msg)"
            }
        } catch {
            $entry.response    = $rawBody
            $entry.interesting = $true
        }

        if ($entry.interesting) {
            Green "HTTP $status  *** HAS DATA ***"
        } else {
            Yellow "HTTP $status  $($entry.notes)"
        }

        $preview = if ($entry.response.Length -gt 400) { $entry.response.Substring(0,400) + "..." } else { $entry.response }
        Write-Host "     $preview" -ForegroundColor DarkGray

    } catch [System.Net.WebException] {
        $status = [int]$_.Exception.Response.StatusCode
        $entry.status = $status
        try {
            $reader  = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $errBody = $reader.ReadToEnd()
            $entry.response = $errBody
            Yellow "HTTP $status  $errBody"
        } catch {
            Yellow "HTTP $status  (no body)"
        }
    } catch {
        $entry.status = "ERROR"
        $entry.notes  = $_.Exception.Message
        Red "Failed: $($_.Exception.Message)"
    }

    $results  += $entry
    $logLines += "[$($entry.status)] $($ep.method) $url"
    if ($entry.response) {
        $logLines += "  $($entry.response.Substring(0, [Math]::Min(600, $entry.response.Length)))"
    }
    $logLines += ""

    Start-Sleep -Milliseconds 400
}

# ── Summary ───────────────────────────────────────────────────
Write-Host ""
Write-Host "============================================================" -ForegroundColor Magenta
Write-Host "  SUMMARY" -ForegroundColor Magenta
Write-Host "============================================================" -ForegroundColor Magenta

$interesting = $results | Where-Object { $_.interesting }
if ($interesting) {
    Write-Host ""
    Write-Host "  *** Endpoints that returned real data ***" -ForegroundColor Green
    foreach ($r in $interesting) {
        Write-Host "  + [$($r.status)] $($r.endpoint)  — $($r.notes)" -ForegroundColor Green
    }
}

$authFail = $results | Where-Object { $_.status -in @(401, 403) }
if ($authFail) {
    Write-Host ""
    Write-Host "  Auth-protected (token may be expired or wrong):" -ForegroundColor Yellow
    foreach ($r in $authFail) {
        Write-Host "  ! [$($r.status)] $($r.endpoint)" -ForegroundColor Yellow
    }
}

# ── Save ──────────────────────────────────────────────────────
$results  | ConvertTo-Json -Depth 8 | Out-File "api_probe_results.json" -Encoding UTF8
$logLines | Out-File "api_probe_log.txt" -Encoding UTF8

Write-Host ""
Write-Host "============================================================" -ForegroundColor Green
Write-Host "  Saved -> api_probe_results.json" -ForegroundColor Green
Write-Host "  Saved -> api_probe_log.txt" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host ""
