"""Tải mô hình VietOCR vào thư mục models/ (một lần, cần Internet).

Lưu cả file cấu hình .yml cạnh trọng số để lúc chạy app không phải tải cấu hình từ mạng nữa.
    .venv\\Scripts\\python tools\\get_vietocr_model.py [vgg_transformer|vgg_seq2seq]
"""
import sys
import urllib.request
from pathlib import Path

import yaml

ROOT = Path(__file__).resolve().parent.parent
MODELS = ROOT / "models"
BASE_URL = "https://vocr.vn/data/vietocr"
CONFIG_FILES = {"vgg_transformer": "vgg-transformer.yml", "vgg_seq2seq": "vgg-seq2seq.yml"}


def _get_yaml(name: str) -> dict:
    with urllib.request.urlopen(f"{BASE_URL}/config/{name}", timeout=60) as r:
        return yaml.safe_load(r.read().decode("utf-8"))


def main(name: str = "vgg_transformer") -> None:
    MODELS.mkdir(exist_ok=True)
    yml, pth = MODELS / f"{name}.yml", MODELS / f"{name}.pth"
    if not yml.exists():
        cfg = _get_yaml("base.yml")
        cfg.update(_get_yaml(CONFIG_FILES[name]))
        cfg["source_url"] = cfg["weights"]
        cfg["weights"] = pth.name
        cfg["device"] = "cpu"
        cfg["cnn"]["pretrained"] = False  # không tải trọng số VGG ImageNet, đã có trong file .pth
        yml.write_text(yaml.safe_dump(cfg, allow_unicode=True, sort_keys=False), encoding="utf-8")
        print("Đã lưu cấu hình:", yml)
    if not pth.exists():
        url = yaml.safe_load(yml.read_text(encoding="utf-8"))["source_url"]
        print("Đang tải", url)
        tmp = pth.with_suffix(".part")
        urllib.request.urlretrieve(url, tmp)
        tmp.replace(pth)
    print(f"Xong: {pth} ({pth.stat().st_size / 1e6:.0f} MB)")


if __name__ == "__main__":
    main(*sys.argv[1:])
