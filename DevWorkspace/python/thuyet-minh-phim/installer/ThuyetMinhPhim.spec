# PyInstaller: đóng gói app thành thư mục dist/ThuyetMinhPhim (chạy không cần cài Python).
# Chạy qua installer/build.ps1.
from pathlib import Path

from PyInstaller.utils.hooks import collect_all, collect_submodules

ROOT = Path(SPECPATH).parent
# Tesseract KHÔNG đưa vào đây (PyInstaller sẽ chép trùng DLL của nó ra _internal); build.ps1 chép sau vào dist/ThuyetMinhPhim/tesseract
datas = [(str(ROOT / "tessdata"), "tessdata")]
binaries = []
hiddenimports = collect_submodules("winrt") + collect_submodules("longtieng")
hiddenimports += ["_cffi_backend", "_miniaudio"]  # miniaudio nạp phần C qua cffi, PyInstaller không tự thấy
for pkg in ("imageio_ffmpeg", "edge_tts", "gtts", "miniaudio", "certifi"):
    d, b, h = collect_all(pkg)
    datas += d
    binaries += b
    hiddenimports += h
# VietOCR: chỉ phần mô hình (vietocr.tool.utils/predictor kéo theo gdown, tqdm -> không dùng).
# Mô hình .pth không đưa vào đây; build.ps1 chép vào dist/ThuyetMinhPhim/models.
hiddenimports += collect_submodules("vietocr.model") + ["vietocr.tool.translate", "einops", "yaml"]
# torchvision nạp phần C (_C_stable.pyd, image_stable.pyd + DLL) theo đường dẫn -> PyInstaller không tự thấy;
# thiếu thì import báo "operator torchvision::nms does not exist"
import torchvision

_tv = Path(torchvision.__file__).parent
binaries += [(str(f), "torchvision") for f in list(_tv.glob("*.pyd")) + list(_tv.glob("*.dll"))]
# VieNeu-TTS: chỉ đường ONNX/CPU (không torch). Mô hình (~520 MB) app tự tải lần đầu, không đưa vào đây.
# Không collect_submodules("vieneu") vì các chế độ GPU/web của nó kéo theo gradio, lmdeploy...
import vieneu

_vn = Path(vieneu.__file__).parent
datas += [(str(_vn / "assets" / "voices_v3_turbo.json"), "vieneu/assets")]
hiddenimports += ["vieneu", "vieneu.factory", "vieneu.base", "vieneu.v3turbo",
                  "vieneu._v3_turbo_engine", "vieneu._v3_turbo_engine.onnx_runtime_lite",
                  "vieneu._v3_turbo_engine.onnx_denoiser", "vieneu._v3_turbo_engine.rep_history",
                  "vieneu._v3_turbo_engine.capture_lock", "vieneu_utils.core_utils", "vieneu_utils.phonemize_text",
                  "soxr", "soundfile", "tokenizers"]
for pkg in ("sea_g2p", "onnxruntime"):
    d, b, h = collect_all(pkg)
    datas += d
    binaries += b
    hiddenimports += h

a = Analysis(
    [str(ROOT / "main.py")],
    pathex=[str(ROOT)],
    binaries=binaries,
    datas=datas,
    hiddenimports=hiddenimports,
    excludes=["matplotlib", "pytest", "IPython", "scipy", "pandas", "tensorboard", "torch.utils.tensorboard",
              "gdown", "torchaudio", "gradio", "librosa", "trafilatura", "lmdeploy"],  # sympy, tqdm: torch cần khi import -> không loại
    noarchive=False,
)
pyz = PYZ(a.pure)
exe = EXE(
    pyz,
    a.scripts,
    [],
    exclude_binaries=True,
    name="ThuyetMinhPhim",
    icon=str(ROOT / "installer" / "icon.ico"),
    console=False,
    upx=False,
)
coll = COLLECT(exe, a.binaries, a.datas, name="ThuyetMinhPhim", upx=False)
