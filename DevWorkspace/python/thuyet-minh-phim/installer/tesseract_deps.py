"""In ra các DLL mà tesseract.exe thực sự cần (đi theo bảng import PE), để không phải chép cả 160 MB."""
import sys
from pathlib import Path

import pefile

root = Path(sys.argv[1])
local = {p.name.lower(): p for p in root.glob("*.dll")}
todo, seen = ["tesseract.exe"], set()
while todo:
    name = todo.pop()
    if name in seen:
        continue
    seen.add(name)
    pe = pefile.PE(str(root / (name if name != "tesseract.exe" else "tesseract.exe")), fast_load=True)
    pe.parse_data_directories(directories=[pefile.DIRECTORY_ENTRY["IMAGE_DIRECTORY_ENTRY_IMPORT"],
                                           pefile.DIRECTORY_ENTRY["IMAGE_DIRECTORY_ENTRY_DELAY_IMPORT"]])
    for attr in ("DIRECTORY_ENTRY_IMPORT", "DIRECTORY_ENTRY_DELAY_IMPORT"):
        for entry in getattr(pe, attr, []):
            dep = entry.dll.decode().lower()
            if dep in local:
                todo.append(dep)
for n in sorted(seen):
    print(n)
