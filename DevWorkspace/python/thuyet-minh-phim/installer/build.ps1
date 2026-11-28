# Kết quả:
#   dist\setup\ThuyetMinhPhim-Setup-<version>.exe       bộ cài
#   dist\portable\ThuyetMinhPhim\                        bản portable chạy được ngay (ThuyetMinhPhim.exe)
#   dist\portable\ThuyetMinhPhim-Portable-<version>.zip  bản portable nén để gửi đi
# Thư mục trung gian (PyInstaller) nằm trong build\.
# Cần trên máy build: Python 3.13 (py install 3.13; ONNX Runtime của VieNeu chưa có cho 3.14+), Tesseract (winget install UB-Mannheim.TesseractOCR), Inno Setup 6 (winget install JRSoftware.InnoSetup).
param([switch]$WithVieNeuModel)  # kèm luôn mô hình VieNeu (~520 MB) vào bộ cài, máy cài không cần Internet
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root

function Invoke-Checked([string]$what, [scriptblock]$cmd) {
    & $cmd
    if ($LASTEXITCODE -ne 0) { throw "$what thất bại (mã $LASTEXITCODE)" }
}

# 1. Môi trường Python
if (-not (Test-Path ".venv")) { Invoke-Checked "Tạo venv (Python 3.13)" { py -3.13 -m venv .venv } }
$py = Join-Path $root ".venv\Scripts\python.exe"
$pyver = (& $py -c "import sys; print('%d.%d' % sys.version_info[:2])").Trim()
if ($pyver -ne "3.13") { throw ".venv đang là Python $pyver, cần 3.13 (xoá .venv rồi chạy lại; cài: py install 3.13)" }
Invoke-Checked "Cài thư viện" { & $py -m pip install -q -r requirements.txt pyinstaller pefile }
# Giọng "chị Google": gTTS (--no-deps: nó đòi click cũ chỉ để chạy dòng lệnh, xung đột huggingface-hub)
Invoke-Checked "Cài gTTS" { & $py -m pip install -q --no-deps "gTTS==2.5.4" }
# VietOCR: PyTorch (CPU) + vietocr (--no-deps vì nó khai báo phụ thuộc cũ) + mô hình vgg_transformer
Invoke-Checked "Cài PyTorch" { & $py -m pip install -q -r requirements-vietocr.txt }
Invoke-Checked "Cài VietOCR" { & $py -m pip install -q --no-deps vietocr }
Invoke-Checked "Tải mô hình VietOCR" { & $py tools\get_vietocr_model.py }
# VieNeu-TTS (giọng offline): chỉ phần chạy ONNX trên CPU (--no-deps: bỏ gradio, librosa... của web demo)
Invoke-Checked "Cài thư viện VieNeu" { & $py -m pip install -q -r requirements-vieneu.txt }
Invoke-Checked "Cài VieNeu" { & $py -m pip install -q --no-deps "vieneu==3.8.5" }
$version = (& $py -c "import longtieng; print(longtieng.__version__)").Trim()
Write-Host "== Thuyết minh phim $version" -ForegroundColor Cyan

# 2. Tìm Tesseract (chỉ mang theo tesseract.exe + các DLL nó cần)
$tessDir = $null
$cmd = Get-Command tesseract -ErrorAction SilentlyContinue
if ($cmd) { $tessDir = Split-Path $cmd.Source }
foreach ($d in @("$env:ProgramFiles\Tesseract-OCR", "$env:LOCALAPPDATA\Programs\Tesseract-OCR")) {
    if (-not $tessDir -and (Test-Path "$d\tesseract.exe")) { $tessDir = $d }
}
if (-not $tessDir) { throw "Không tìm thấy Tesseract. Cài: winget install UB-Mannheim.TesseractOCR" }
$files = & $py installer\tesseract_deps.py $tessDir
Write-Host "Tesseract: $($files.Count) file từ $tessDir"

# 3. Đóng gói app vào build\app\ThuyetMinhPhim, rồi đặt Tesseract cạnh ThuyetMinhPhim.exe
#    (không cho PyInstaller xử lý, vì nó sẽ chép trùng các DLL của Tesseract ra _internal)
Invoke-Checked "PyInstaller" {
    & $py -m PyInstaller --noconfirm --clean --log-level WARN --distpath build\app --workpath build\pyi installer\ThuyetMinhPhim.spec
}
$app = Join-Path $root "build\app\ThuyetMinhPhim"
$stage = Join-Path $app "tesseract"
New-Item -ItemType Directory -Force $stage | Out-Null
foreach ($f in $files) { Copy-Item (Join-Path $tessDir $f.Trim()) $stage }
if (Test-Path "$tessDir\doc\LICENSE") { Copy-Item "$tessDir\doc\LICENSE" "$stage\LICENSE-tesseract.txt" }
$models = Join-Path $app "models"
New-Item -ItemType Directory -Force $models | Out-Null
Copy-Item models\vgg_transformer.pth, models\vgg_transformer.yml $models
if ($WithVieNeuModel) {
    Invoke-Checked "Tải mô hình VieNeu" { & $py -c "from longtieng import vieneu_tts as v; v.ensure_models(print)" }
    Copy-Item -Recurse models\vieneu (Join-Path $models "vieneu")
}

# 4. Bộ cài
$iscc = @("$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe", "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
          "$env:ProgramFiles\Inno Setup 6\ISCC.exe") | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) { throw "Không tìm thấy Inno Setup 6. Cài: winget install JRSoftware.InnoSetup" }
Remove-Item (Join-Path $root "dist\setup\ThuyetMinhPhim-Setup-*.exe") -ErrorAction SilentlyContinue  # bỏ bộ cài bản cũ
Invoke-Checked "Inno Setup" { & $iscc /Q "/DAppVersion=$version" installer\ThuyetMinhPhim.iss }

$out = Join-Path $root "dist\setup\ThuyetMinhPhim-Setup-$version.exe"
Write-Host ("Xong: {0} ({1:N0} MB)" -f $out, ((Get-Item $out).Length / 1MB)) -ForegroundColor Green

# 5. Bản portable: chép thư mục app sang dist\portable\ThuyetMinhPhim + portable.txt
#    (app thấy file này thì ghi cấu hình/cache/mô hình vào data\ cạnh exe).
#    Giữ nguyên data\ nếu đã chạy thử bản portable trước đó (mô hình VieNeu đã tải...).
$portable = Join-Path $root "dist\portable\ThuyetMinhPhim"
New-Item -ItemType Directory -Force $portable | Out-Null
& robocopy $app $portable /MIR /XD (Join-Path $portable "data") /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -ge 8) { throw "Chép bản portable thất bại (robocopy mã $LASTEXITCODE)" }
$global:LASTEXITCODE = 0
$portableNote = Join-Path $portable "portable.txt"
@"
Thuyết minh phim $version - bản portable (không cần cài đặt)

- Giải nén cả thư mục ra chỗ ghi được: ổ D:\, thư mục Tài liệu, USB... (không để trong Program Files),
  rồi chạy ThuyetMinhPhim.exe. Không chạy thẳng từ trong file zip.
- Có thể đổi tên hoặc chuyển thư mục đi đâu cũng được, chỉ cần giữ nguyên các file bên trong.
- Cấu hình, cache giọng đọc, mô hình giọng VieNeu tải về đều nằm trong thư mục data\ bên cạnh.
  Chép cả thư mục sang máy khác là mang theo hết. Xoá thư mục là gỡ xong.
- Giọng Microsoft An (offline) phải tự cài trong Windows: Settings > Time & language > Speech > Add voices > Tiếng Việt.
  Không có thì dùng giọng VieNeu (offline, tải mô hình ~520 MB lần đầu) hoặc HoaiMy/NamMinh (cần mạng).
- Kiểm tra máy: ThuyetMinhPhim.exe --selftest, kết quả ở data\selftest.txt.
- Xoá file portable.txt này thì app ghi dữ liệu vào %APPDATA%\ThuyetMinhPhim như bản cài.
"@ | Set-Content -Encoding utf8 $portableNote
$zip = Join-Path $root "dist\portable\ThuyetMinhPhim-Portable-$version.zip"
Remove-Item (Join-Path $root "dist\portable\ThuyetMinhPhim-Portable-*.zip") -ErrorAction SilentlyContinue
# Không nén data\ (cấu hình, mô hình đã tải khi chạy thử trên máy build)
Invoke-Checked "Nén bản portable" {
    & "$env:SystemRoot\System32\tar.exe" -a -c -f $zip --exclude "ThuyetMinhPhim/data" -C (Join-Path $root "dist\portable") ThuyetMinhPhim
}
Write-Host ("Xong: {0} ({1:N0} MB)" -f $zip, ((Get-Item $zip).Length / 1MB)) -ForegroundColor Green
Write-Host "Bản portable chạy ngay: $portable\ThuyetMinhPhim.exe" -ForegroundColor Green
