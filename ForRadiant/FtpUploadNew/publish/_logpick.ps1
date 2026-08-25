# =============================================================================
#  _logpick.ps1 - calendar picker for the HTML reports. Mirrors the in-app view:
#  a month calendar where the days that HAVE a log are shown BOLD + RED and are
#  clickable; clicking one builds + opens that day's report. Non-log days are
#  greyed and inert. Kind = day (upload report) | ng (NG report).
#
#     _htmllog.bat   -> _logpick.ps1 -Kind day
#     _nghtmllog.bat -> _logpick.ps1 -Kind ng
# =============================================================================
param([ValidateSet('day','ng')][string]$Kind = 'day')

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$root = $PSScriptRoot
if (-not $root) { $root = Split-Path -Parent $MyInvocation.MyCommand.Path }

# ---- resolve the log folder from config.json (same rule the app/report scripts use) ----
$LogFolder = ''
$cfgPath = Join-Path $root 'config.json'
if (Test-Path $cfgPath) {
    try { $cfg = Get-Content $cfgPath -Raw | ConvertFrom-Json; $LogFolder = [string]$cfg.LogFolder } catch { }
}
if ([string]::IsNullOrEmpty($LogFolder)) { $LogFolder = Join-Path $root 'logs' }
if (-not [System.IO.Path]::IsPathRooted($LogFolder)) { $LogFolder = Join-Path $root $LogFolder }

if ($Kind -eq 'ng') {
    $pattern = '^(\d{8})_ngretrylog\.txt$'
    $script:gen = Join-Path $root '_nghtmllog.ps1'
    $title = 'NG Retry Report - pick a day'
} else {
    $pattern = '^(\d{8})_rawlog\.txt$'
    $script:gen = Join-Path $root '_htmllog.ps1'
    $title = 'Upload Day Report - pick a day'
}

# ---- which days have a log ----
$script:logDays = @{}
if (Test-Path $LogFolder) {
    Get-ChildItem $LogFolder -File -ErrorAction SilentlyContinue | ForEach-Object {
        if ($_.Name -match $pattern) { $script:logDays[$matches[1]] = $true }
    }
}
$allDates = @($script:logDays.Keys | ForEach-Object { [datetime]::ParseExact($_, 'yyyyMMdd', $null) })
$startView = if ($allDates.Count -gt 0) { ($allDates | Sort-Object)[-1] } else { Get-Date }
$script:view = (Get-Date -Year $startView.Year -Month $startView.Month -Day 1)

# ---- palette / fonts ----
$fRed  = New-Object System.Drawing.Font('Segoe UI', 9, [System.Drawing.FontStyle]::Bold)
$fGrey = New-Object System.Drawing.Font('Segoe UI', 9, [System.Drawing.FontStyle]::Regular)
$cRed  = [System.Drawing.Color]::FromArgb(211, 47, 47)     # log day
$cGrey = [System.Drawing.Color]::FromArgb(150, 150, 150)   # no log
$cHdr  = [System.Drawing.Color]::FromArgb(90, 100, 125)

# ---- form ----
$form = New-Object System.Windows.Forms.Form
$form.Text = $title
$form.StartPosition = 'CenterScreen'
$form.FormBorderStyle = 'FixedDialog'
$form.MaximizeBox = $false
$form.MinimizeBox = $false
$form.ClientSize = New-Object System.Drawing.Size(300, 356)
$form.Font = New-Object System.Drawing.Font('Segoe UI', 9)
$form.BackColor = [System.Drawing.Color]::White

# header: < month >
$btnPrev = New-Object System.Windows.Forms.Button
$btnPrev.Text = [char]0x25C0; $btnPrev.Location = New-Object System.Drawing.Point(10, 10)
$btnPrev.Size = New-Object System.Drawing.Size(30, 26); $btnPrev.FlatStyle = 'Flat'
$form.Controls.Add($btnPrev)

$lblMonth = New-Object System.Windows.Forms.Label
$lblMonth.Location = New-Object System.Drawing.Point(44, 12); $lblMonth.Size = New-Object System.Drawing.Size(212, 24)
$lblMonth.TextAlign = 'MiddleCenter'
$lblMonth.Font = New-Object System.Drawing.Font('Segoe UI', 11, [System.Drawing.FontStyle]::Bold)
$form.Controls.Add($lblMonth)

$btnNext = New-Object System.Windows.Forms.Button
$btnNext.Text = [char]0x25B6; $btnNext.Location = New-Object System.Drawing.Point(260, 10)
$btnNext.Size = New-Object System.Drawing.Size(30, 26); $btnNext.FlatStyle = 'Flat'
$form.Controls.Add($btnNext)

# day-of-week header
$dow = @('Su','Mo','Tu','We','Th','Fr','Sa')
for ($i = 0; $i -lt 7; $i++) {
    $h = New-Object System.Windows.Forms.Label
    $h.Text = $dow[$i]; $h.TextAlign = 'MiddleCenter'; $h.ForeColor = $cHdr
    $h.Location = New-Object System.Drawing.Point((12 + $i * 40), 44)
    $h.Size = New-Object System.Drawing.Size(38, 20)
    $form.Controls.Add($h)
}

# 6 x 7 day cells
$script:cells = @()
$cellClick = {
    $ds = $this.Tag
    if ($ds -and $script:logDays.ContainsKey($ds)) {
        $btnClose.Enabled = $false
        try { & powershell -NoProfile -ExecutionPolicy Bypass -File $script:gen $ds | Out-Null }
        catch { [System.Windows.Forms.MessageBox]::Show($_.Exception.Message, 'Report error', 'OK', 'Error') | Out-Null }
        $btnClose.Enabled = $true
    }
}
for ($r = 0; $r -lt 6; $r++) {
    for ($c = 0; $c -lt 7; $c++) {
        $cell = New-Object System.Windows.Forms.Label
        $cell.TextAlign = 'MiddleCenter'
        $cell.Location = New-Object System.Drawing.Point((12 + $c * 40), (66 + $r * 40))
        $cell.Size = New-Object System.Drawing.Size(38, 36)
        $cell.Add_Click($cellClick)
        $form.Controls.Add($cell)
        $script:cells += $cell
    }
}

$lblInfo = New-Object System.Windows.Forms.Label
$lblInfo.Location = New-Object System.Drawing.Point(12, 306); $lblInfo.Size = New-Object System.Drawing.Size(276, 16)
$lblInfo.ForeColor = $cHdr
$form.Controls.Add($lblInfo)

$btnClose = New-Object System.Windows.Forms.Button
$btnClose.Text = 'Close'; $btnClose.Location = New-Object System.Drawing.Point(196, 324)
$btnClose.Size = New-Object System.Drawing.Size(92, 26)
$btnClose.Add_Click({ $form.Close() })
$form.Controls.Add($btnClose)

function Render-Month {
    $first = $script:view
    $lblMonth.Text = $first.ToString('MMMM yyyy')
    $startCol = [int]$first.DayOfWeek                      # 0 = Sunday
    $dim = [DateTime]::DaysInMonth($first.Year, $first.Month)
    $today = (Get-Date).ToString('yyyyMMdd')
    for ($i = 0; $i -lt 42; $i++) {
        $cell = $script:cells[$i]
        $cell.Text = ''; $cell.Tag = $null; $cell.BackColor = [System.Drawing.Color]::White
        $cell.Cursor = [System.Windows.Forms.Cursors]::Default
        $cell.BorderStyle = 'None'
    }
    for ($d = 1; $d -le $dim; $d++) {
        $idx = $startCol + $d - 1
        if ($idx -ge 42) { break }
        $cell = $script:cells[$idx]
        $ds = ('{0:0000}{1:00}{2:00}' -f $first.Year, $first.Month, $d)
        $cell.Text = "$d"; $cell.Tag = $ds
        if ($script:logDays.ContainsKey($ds)) {
            $cell.Font = $fRed; $cell.ForeColor = $cRed
            $cell.Cursor = [System.Windows.Forms.Cursors]::Hand
        } else {
            $cell.Font = $fGrey; $cell.ForeColor = $cGrey
        }
        if ($ds -eq $today) { $cell.BorderStyle = 'FixedSingle' }
    }
}

$btnPrev.Add_Click({ $script:view = $script:view.AddMonths(-1); Render-Month })
$btnNext.Add_Click({ $script:view = $script:view.AddMonths(1);  Render-Month })

if ($script:logDays.Count -gt 0) {
    $lblInfo.Text = "Red days have a log ($($script:logDays.Count)) - click one to open its report."
} else {
    $lblInfo.Text = "No logs found in $LogFolder"
    $lblInfo.ForeColor = [System.Drawing.Color]::Firebrick
}

Render-Month
[void]$form.ShowDialog()
$form.Dispose()
