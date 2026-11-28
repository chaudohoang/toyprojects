# Thuyết minh phim

Thuyết minh tiếng Việt (một giọng đọc cho mọi nhân vật) cho phim có phụ đề in cứng (*hardsub*), trên Windows. Hai chế độ:

- **File phim (thuyết minh sẵn)** — khuyên dùng. Quét cả file phim lấy phụ đề → file `.srt` → tạo giọng cho từng câu vừa khít thời gian → xuất video mới có tiếng thuyết minh. Xem bằng trình phát bất kỳ, tua thoải mái, giọng khớp từng câu.
- **Trực tiếp (màn hình)**: chụp vùng phụ đề trên màn hình khi xem phim online → OCR → đọc ngay. Tiện nhưng phụ đề đổi quá nhanh thì khó theo kịp.

```
màn hình ──(mss, ~6 lần/giây)──► tách chữ trắng viền đen ──► có câu mới? ──► Tesseract (vie) ──► lọc rác/trùng ──► Edge TTS ──► loa
```

## Cài trên máy khác (bộ cài)

Gửi file `dist\setup\ThuyetMinhPhim-Setup-<phiên bản>.exe` sang máy kia và chạy. **Không cần cài Python, Tesseract hay
PyTorch**: bộ cài đã kèm sẵn Python, Tesseract + dữ liệu tiếng Việt, VietOCR (PyTorch + mô hình), thư viện giọng
VieNeu, ffmpeg. Cần Windows 10 (1809) / 11, 64-bit.

- Cài cho mọi người dùng (cần quyền admin) hoặc chọn *chỉ cho tôi* nếu không có quyền admin.
- Máy chưa có giọng **Microsoft An** thì bộ cài có ô *Cài giọng đọc offline Microsoft An* (chỉ hiện khi cài bằng
  quyền admin, cần Internet, vài phút). Hoặc tự cài sau: Settings → Time & language → Speech → Add voices → Tiếng Việt.
  Không có giọng An thì chọn giọng HoaiMy / NamMinh / chị Google (cần mạng) hoặc một giọng VieNeu.
- Giọng **VieNeu** (offline): lần đầu chọn giọng VieNeu, app hỏi tải mô hình ~520 MB từ HuggingFace về
  `%APPDATA%\ThuyetMinhPhim\models\vieneu` (một lần, cần Internet lúc tải). Máy không có mạng: tạo bộ cài bằng
  `installer\build.bat -WithVieNeuModel` để kèm sẵn mô hình (bộ cài nặng thêm ~450 MB).
- Cấu hình, cache giọng đọc, mô hình VieNeu nằm ở `%APPDATA%\ThuyetMinhPhim` (gỡ cài đặt sẽ xoá cache và mô hình, giữ `config.json`).
- Kiểm tra máy mới: chạy `"C:\Program Files\ThuyetMinhPhim\ThuyetMinhPhim.exe" --selftest` rồi mở
  `%APPDATA%\ThuyetMinhPhim\selftest.txt` (OCR Tesseract, VietOCR, giọng Windows, giọng VieNeu, giọng Google, ffmpeg, màn hình đều phải `[OK]`).
  Thêm đường dẫn một file phim sau `--selftest` để thử luôn quét + thuyết minh.
  App gặp lỗi khi chạy sẽ ghi chi tiết ra `%APPDATA%\ThuyetMinhPhim\error.log`.

**Bản portable** (không cần cài): `dist\portable\ThuyetMinhPhim-Portable-<phiên bản>.zip`; cạnh đó là thư mục
`dist\portable\ThuyetMinhPhim` đã giải nén sẵn, chạy thử ngay được. Giải nén cả thư mục ra chỗ ghi được
(ổ D:\, Tài liệu, USB; không để trong Program Files) rồi chạy `ThuyetMinhPhim.exe`. Có file `portable.txt` cạnh exe nên
cấu hình, cache, mô hình VieNeu nằm trong `data\` bên cạnh: chép cả thư mục là mang theo hết, xoá thư mục là gỡ xong.
Khác bộ cài: không có shortcut, không tự cài giọng Microsoft An (cần admin), và không thêm vào Add/Remove Programs.

**Tạo lại bộ cài** (trên máy có mã nguồn): chạy `installer\build.bat` (tự cài PyTorch, VietOCR, VieNeu và tải mô hình VietOCR nếu thiếu). Cần **Python 3.13**
(`py install 3.13`), Tesseract
(`winget install UB-Mannheim.TesseractOCR`) và Inno Setup 6 (`winget install JRSoftware.InnoSetup`). Đổi số phiên bản
trong `longtieng/__init__.py`. Script đóng gói app bằng PyInstaller (`installer/ThuyetMinhPhim.spec`), chép
`tesseract.exe` cùng đúng các DLL nó cần, rồi tạo bộ cài bằng `installer/ThuyetMinhPhim.iss` và file zip portable từ cùng thư mục đó.

## Cài đặt từ mã nguồn

1. **Python 3.13** (`py install 3.13`). Không dùng 3.14+ vì ONNX Runtime (cần cho giọng VieNeu) chưa có bản cho
   các phiên bản đó; máy có sẵn Python khác vẫn cài thêm 3.13 được, `run.bat` tự chọn 3.13.
2. **Cài Tesseract OCR** (Windows không có gói OCR tiếng Việt nên phải dùng Tesseract):
   ```powershell
   winget install UB-Mannheim.TesseractOCR
   ```
   Dữ liệu tiếng Việt đã có sẵn trong dự án: `tessdata/fast/vie.traineddata` (mặc định) và `tessdata/best/vie.traineddata`
   (tải từ `tesseract-ocr/tessdata_fast` và `tessdata_best` trên GitHub), không cần chép vào Program Files.

   Kiểm tra: `.venv\Scripts\python tools\test_image.py samples\motchill.webp --crop 300,720,1320,220` phải in ra `OCR: tesseract (vie)`.
3. Chạy `run.bat` (lần đầu sẽ tự tạo `.venv` và cài thư viện), hoặc:
   ```powershell
   py -3.13 -m venv .venv
   .venv\Scripts\python -m pip install -r requirements.txt
   .venv\Scripts\python main.py
   ```
4. **VietOCR** (tuỳ chọn nhưng nên có, ~1 GB): `run.bat` tự cài lần đầu. Cài tay:
   ```powershell
   .venv\Scripts\python -m pip install -r requirements-vietocr.txt
   .venv\Scripts\python -m pip install --no-deps vietocr
   .venv\Scripts\python tools\get_vietocr_model.py
   ```
   (`--no-deps` vì gói vietocr khai báo phụ thuộc cũ không có cho Python mới.) Thiếu VietOCR thì app chỉ dùng Tesseract.
5. **Giọng VieNeu** (tuỳ chọn, ~150 MB thư viện + ~520 MB mô hình tải khi chọn giọng): `run.bat` tự cài. Cài tay:
   ```powershell
   .venv\Scripts\python -m pip install -r requirements-vieneu.txt
   .venv\Scripts\python -m pip install --no-deps vieneu==3.8.5
   ```
   (`--no-deps` để bỏ gradio, librosa... chỉ cần cho web demo và nhân bản giọng.)
6. **Giọng chị Google** (gTTS): `run.bat` tự cài. Cài tay: `.venv\Scripts\python -m pip install --no-deps gTTS==2.5.4`
   (`--no-deps` vì gTTS đòi bản `click` cũ chỉ để chạy dòng lệnh, xung đột với huggingface-hub; nó chỉ cần `requests`.)

## Chế độ File phim

1. Tab **File phim (thuyết minh sẵn)** → **Chọn phim...** (`.mp4`, `.mkv`, ...).
2. **1. Quét phụ đề**: đọc **1/3 dưới** (thoại) và **40% trên** (phụ đề phụ: biển hiệu, địa danh, thời gian, lời qua
   điện thoại... thường trong `[ngoặc vuông]`) của khung hình, 5 lần/giây. Mỗi câu OCR 3 khung × 3 cách xử lý ảnh (model `best`,
   chạy song song theo số nhân CPU) rồi **bỏ phiếu từng chữ**. Câu mà các lần OCR không thống nhất (chữ Hán ở phần
   giới thiệu, logo, hiệu ứng) bị bỏ; chữ không ghép được thành âm tiết tiếng Việt bị lọc; sửa tự động vài lỗi mất dấu
   hay gặp (rôi→rồi, rât→rất, se→sẽ, vân→vẫn, cân→cần, theo đuối→theo đuổi, gì đầu→gì đâu...).

   **VietOCR**: mỗi câu đọc thêm 1 khung bằng mô hình VietOCR (`vgg_transformer`) rồi ghép với Tesseract
   (`textproc.merge_ocr`). Hai bên sai ở chữ khác nhau: Tesseract hay làm rơi dấu thanh trên chữ có mũ/móc
   (lần→lân, tiền→tiên, Hồng→Hông, đồ→đô), VietOCR đọc đúng các chữ đó nhưng hay nhầm loại dấu (sớm→sóm, cả→cá).
   Luật ghép: tin Tesseract, chỉ lấy dấu thanh của VietOCR trên nguyên âm có mũ/móc mà Tesseract để trống.

   Đo trên tập 1 (1280×534), so với đáp án chép tay:

   | | 100 câu (dùng để chỉnh luật) | 50 câu khác |
   |---|---|---|
   | Chỉ Tesseract | 77 câu đúng hết, 4,6% chữ sai | 46 câu, 1,4% chữ sai |
   | Tesseract + VietOCR | **91 câu, 2,0% chữ sai** | **50 câu, 0 chữ sai** |

   (Vài luật sửa theo ngữ cảnh được thêm sau khi đã thấy lỗi của bộ 50 câu, nên cột này hơi lạc quan.)
   Quét cả tập 35 phút: ~5,5 phút có VietOCR (~3 phút nếu chỉ Tesseract), 546 câu.
   Lỗi còn lại chủ yếu là chữ cả hai cùng sai (vả→và) → vẫn nên lướt file .srt trước khi xuất.
   Vùng trên: logo kênh đứng yên suốt phim (vd *Motchillv.so*) được tự tìm và xoá trước khi OCR; có ngoặc vuông thì
   chỉ lấy phần trong ngoặc; biển hiệu lặp lại (*[Công an]* mỗi cảnh xe cảnh sát) chỉ đọc 1 lần mỗi 60 giây.
   Tập 1 ở trên có thêm 42 câu phụ đề phụ, không lẫn rác. Khi đọc, ngoặc vuông được bỏ.
   Kết quả: `<tên phim>.thuyet-minh.srt` cạnh file phim (phụ đề phụ giữ ngoặc vuông để dễ nhận ra khi sửa).
3. **Sửa phụ đề (.srt)** (tuỳ chọn): mở file bằng Notepad, sửa chữ OCR sai rồi lưu. Bước 4 đọc lại file này.
4. **2. Thuyết minh & xuất**: tạo giọng từng câu (6 câu song song; Edge lỗi thì thử lại tới 6 lần rồi mới dùng giọng Windows).
   Câu nào dài hơn khoảng trống tới câu kế thì tạo lại với tốc độ nhanh hơn (tối đa `dub_max_rate` = +80%).
   Giọng được lưu trong `cache/tts/`, sửa vài câu rồi xuất lại chỉ tạo giọng cho câu đã sửa.
5. Kết quả: `<tên phim>.thuyet-minh.mp4` (hoặc `.mkv`), hình giữ nguyên (không mã hoá lại):
   - Track âm thanh 1 **Thuyết minh**: giọng đọc + tiếng gốc, tiếng gốc chỉ nhỏ xuống (`original_volume`, mặc định 30%) lúc có lời, nhạc/hiệu ứng giữ nguyên lúc không thoại.
   - Track 2 **Tiếng gốc**: nguyên bản, đổi track trong trình phát nếu muốn nghe lại.
   Bấm **▶ Xem** để mở bằng trình phát mặc định.

Ô **Giọng thuyết minh** của tab này độc lập với tab Trực tiếp (vd trực tiếp dùng Microsoft An cho nhanh, xuất file
dùng VieNeu cho hay). Tốc độ tối thiểu, âm lượng lấy theo tab Trực tiếp. Tạo video thử: `.venv\Scripts\python tools\make_test_video.py <thư mục>`.

**Cả thư mục...** (hàng chờ nhiều bộ phim): hộp chọn thư mục kiểu Explorer, **giữ Ctrl / Shift để chọn nhiều thư mục**
(`longtieng/folder_dialog.py`); chọn thư mục cha (vd `G:\E`) thì lấy cả mọi thư mục con. App quét + thuyết minh + xuất
từng tập, hết bộ này sang bộ kế, trong mỗi bộ theo số tập (Tập 2 trước Tập 10). File mới lưu cạnh từng tập như trên
(`longtieng/batch.py`).
- Đang chạy, nút đổi thành **Thêm thư mục...**: chọn thêm thư mục (vd vừa tải thêm phim) thì các tập nối vào cuối hàng
  chờ. Chọn lại thư mục cũ cũng được: chỉ thêm tập chưa có trong hàng chờ.
- Quét 2 tập cùng lúc (`batch_scan_workers`, mặc định 2), song song với tạo giọng + ghép tập đã quét xong. Đo trên CPU
  14 nhân: quét 1 tập ~20 giây/phút phim, 2 tập cùng lúc ~13 giây/phút phim (nhanh hơn ~1,5 lần); 3 tập không nhanh thêm.
  Vì vậy không cần mở nhiều app: một app tự làm song song (mở 2 app dùng chung cài đặt và dễ đè nhau).
- Tập đã có `.thuyet-minh.srt` thì dùng luôn (sửa tay trước cũng được), tập đã có video thuyết minh thì bỏ qua: bấm
  *Huỷ* rồi chạy lại sẽ làm tiếp. Một tập lỗi thì ghi vào nhật ký rồi làm tiếp tập sau.
- Muốn sửa phụ đề trước khi thuyết minh: chạy một lượt, xoá video thuyết minh của tập cần sửa, sửa `.srt`, chạy lại.

Ô **Xuất xong thì xoá file phim gốc và .srt** (`delete_source`, mặc định tắt; dùng cho cả tập lẻ và cả thư mục):
xuất xong tập nào thì chuyển phim gốc + `.thuyet-minh.srt` của tập đó vào **Thùng rác** (khôi phục được; muốn trống
ổ đĩa thì dọn Thùng rác). Chỉ xoá khi file thuyết minh có thật và dài bằng phim gốc (lệch < 3 giây); tập bị bỏ qua vì
đã xuất từ trước thì không đụng tới. Ổ không có Thùng rác (USB, ổ mạng) thì Windows hỏi trước khi xoá hẳn.

## Chế độ Trực tiếp


| Phím | Việc |
|---|---|
| **F4** | Đổi màn hình: Tự động → Màn hình 1 → 2 → 3 → Tự động. Hiện khung đỏ + số màn hình 1,5 giây |
| **F8** | Kéo tay vùng phụ đề (tắt chế độ tự động) |
| **F2** | Thử đọc câu phụ đề đang hiện |
| **F9** | Bắt đầu / dừng đọc tự động |

Phím tắt hoạt động ở mọi cửa sổ. Đổi phím trong `config.json` (`region_hotkey`, `test_hotkey`, `hotkey`).

1. Chạy `run.bat`, rồi mở phim và cho phát **toàn màn hình**.
2. Vùng phụ đề **tự động** là 1/3 dưới **cửa sổ đang chiếu phim**, tìm lại mỗi lần bấm F9/F2 (tên cửa sổ hiện cạnh ô *Tự động*):
   - Tìm cửa sổ phim theo thứ tự: cửa sổ đang **toàn màn hình** (trên màn hình bất kỳ) → cửa sổ đang active
     (bấm F9 lúc xem phim ở cửa sổ thường) → cửa sổ vừa dùng trước khi bấm vào phần mềm → màn hình có con trỏ chuột.
   - Chỉ đọc chữ trong cửa sổ phim: phần bị cửa sổ khác đè lên (chat, thông báo, taskbar, cửa sổ phần mềm này)
     được bôi đen trước khi OCR. Cửa sổ phim di chuyển/đổi cỡ thì vùng đọc đi theo; thu nhỏ thì tạm ngừng đọc.
   - Bấm **F4** để cố định một màn hình (đánh số từ trái sang phải). Khung đỏ hiện trên màn hình vừa chọn cho biết vùng sẽ đọc; khung không lấy focus nên phim không thoát toàn màn hình. Nếu đang đọc thì đổi luôn.
   - Phụ đề nằm chỗ khác thì bấm **F8** kéo tay (`Esc` để huỷ); vùng kéo tay vẫn bỏ phần bị cửa sổ khác đè lên. Bấm F4 hoặc tick lại *Tự động* để quay về.
3. Khi đang có phụ đề, bấm **F2** để thử. Ảnh gốc và ảnh đã xử lý lưu ở `debug/test_frame.png` và `debug/test_ocr.png`.
4. Bấm **F9** để đọc tự động, bấm **F9** lần nữa để dừng.
5. Nên giảm tiếng gốc của phim để nghe giọng đọc rõ hơn.

## Cách hoạt động

| Bước | File | Ghi chú |
|---|---|---|
| Chụp màn hình | `longtieng/engine.py` | `mss`, chỉ chụp vùng đã chọn nên rất nhẹ |
| Tách chữ | `longtieng/preprocess.py` | Giữ pixel **sáng + ít màu** và có **viền tối** bao quanh → loại mặt người, nền trắng, watermark xám mờ. Sau đó chỉ giữ các dòng có cỡ chữ ≥ 60% dòng to nhất → loại thời gian/nút của trình phát khi tạm dừng |
| Phát hiện câu mới | `engine.py` | So mặt nạ chữ giữa các khung hình; chỉ OCR khi phụ đề đổi và đứng yên ≥ 1 khung → OCR rất ít lần |
| OCR | `longtieng/ocr.py` | Tesseract `vie`, offline. Model `fast` ~125–210 ms/câu, `best` ~195–280 ms (đều đọc đúng 100% câu mẫu) |
| Lọc | `longtieng/textproc.py` | Bỏ dòng chứa từ trong `blacklist`, bỏ gạch đầu dòng hội thoại, chống đọc lặp bằng độ giống văn bản |
| Quét file | `longtieng/subscan.py`, `video.py`, `vietocr_ocr.py` | ffmpeg (gói `imageio-ffmpeg`) giải mã, quét riêng 1/3 dưới và 40% trên, Tesseract bỏ phiếu + VietOCR ghép dấu |
| Thuyết minh file | `longtieng/dubber.py` | Tạo giọng song song, khớp thời gian, ghép bằng ffmpeg (`amultiply` với đường âm lượng để hạ tiếng gốc lúc có lời) |
| Giọng VieNeu | `longtieng/vieneu_tts.py` | VieNeu-TTS v3 Turbo chạy ONNX trên CPU (không cần GPU/PyTorch), 25 giọng có sẵn, 48 kHz → 24 kHz. Không có chỉnh tốc độ nên co giãn âm thanh bằng ffmpeg `atempo` (giữ cao độ) |
| Giọng Google | `longtieng/google_tts.py` | gTTS (giọng đọc Google Dịch) → mp3 → PCM 24 kHz, chuẩn hoá âm lượng; tốc độ/cao độ bằng ffmpeg `asetrate` + `atempo` |
| Đọc | `longtieng/tts.py` | Edge TTS (Hoài My / Nam Minh, cần Internet). Edge lỗi hoặc chậm quá 2 giây thì đọc bằng giọng Windows *Microsoft An* (offline, ~0,1 s); lỗi 3 lần liền thì dùng giọng Windows 60 giây. Câu mới ngắt câu cũ để giữ khớp hình |

## Tuỳ chỉnh (`config.json`, tự tạo khi chạy)

- `auto_region` / `auto_region_fraction` (0.3334): bật vùng tự động và tỉ lệ chiều cao tính từ đáy màn hình.
- `monitor`: 0 = tự động, 1/2/3 = cố định màn hình (F4 sẽ tự đổi giá trị này).
- `blacklist`: thêm tên web/watermark hay xuất hiện, vd `"phimmoi"`.
- `min_brightness` (190): hạ xuống nếu phụ đề hơi xám; tăng nếu dính nền sáng.
- `require_outline` (true): đặt `false` nếu phụ đề không có viền đen (chỉ có bóng mờ).
- `tesseract_model`: `fast` (mặc định) hoặc `best` nếu gặp phụ đề khó đọc.
- `rate`: tốc độ đọc. Phim nói nhanh nên để +20 đến +40%.
- `fps`, `stable_frames`: tăng `stable_frames` lên 2 nếu phụ đề có hiệu ứng mờ dần gây đọc sai.
- `voice`: mặc định `windows` (Microsoft An, offline, miễn phí, cần cài *Text-to-speech* Tiếng Việt trong Settings → Language), hoặc `vi-VN-NamMinhNeural` / `vi-VN-HoaiMyNeural` (Edge, cần mạng, đôi khi lỗi), hoặc `google` (chị Google, cần mạng).
- `pitch` (−20): cao độ %, −20 cho giọng trầm kiểu thuyết minh. Thanh *Cao độ* trong giao diện (không áp dụng cho giọng VieNeu; với giọng Google, kéo xuống để có giọng trầm hơn).
- `voice`: `"windows"`, `"vi-VN-NamMinhNeural"`, `"vi-VN-HoaiMyNeural"`, `"google"` hoặc `"vieneu:<tên giọng>"` (vd `"vieneu:Thiện Minh"`).
  `dub_voice`: giọng khi xuất file ở tab File phim, cùng các giá trị như trên.
  `vieneu_threads` (0 = tự chọn, tối đa 8): số luồng CPU cho VieNeu.
  `google_concurrency` (3): số câu gửi Google cùng lúc khi xuất file.
- `edge_deadline` (2.0): số giây chờ Edge / Google trước khi chuyển sang giọng Windows. `edge_fallback: false` để bỏ qua câu thay vì đổi giọng.
- `scan_zones`: vùng quét file, tỉ lệ chiều cao từ trên xuống, mặc định `{"bottom": [0.6666, 1.0], "top": [0.0, 0.4]}`. Xoá `"top"` nếu không cần phụ đề phụ.
- `scan_vietocr` (true): dùng thêm VietOCR khi quét file. `vietocr_threads` (0 = tự chọn): số luồng CPU cho VietOCR.
- `scan_ignore_static` (true): bỏ chữ đứng yên suốt phim (logo). `side_repeat_seconds` (60): biển hiệu lặp lại trong khoảng này chỉ đọc 1 lần.
- `line_height_ratio` (0.6): đặt `0` nếu phụ đề có dòng chữ nhỏ hơn hẳn dòng kia mà bị mất.

## Thử trên ảnh

```powershell
.venv\Scripts\python tools\test_image.py samples\motchill.webp --crop 300,720,1320,220 --speak
```

## Lưu ý

- Edge TTS gửi nội dung phụ đề lên dịch vụ đọc của Microsoft, giọng Google gửi lên Google Dịch. Muốn hoàn toàn offline thì chọn giọng *Microsoft An* hoặc VieNeu.
- Giọng VieNeu tự nhiên hơn nhưng chậm: tạo giọng nhanh gấp ~1,6 lần thời gian thực trên CPU 14 nhân (i5-13500), máy
  yếu hơn sẽ chậm hơn. Hợp với chế độ File phim; ở chế độ Trực tiếp mỗi câu trễ thêm ~1–3 giây và câu nào bị câu mới
  vượt trước thì bỏ. Giấy phép: gói `vieneu` là Apache 2.0; điều khoản của mô hình trên HuggingFace
  (`pnnbao-ump/VieNeu-TTS-v3-Turbo`, `OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX`) hãy tự xem trước khi dùng thương mại.
- Giọng chị Google (gói `gTTS`, MIT) dùng dịch vụ đọc không chính thức của Google Dịch: nhanh (~0,35 s/câu ở chế độ
  Trực tiếp; xuất cả tập 546 câu ~50 giây), nhưng mất mạng hoặc bị Google tạm chặn thì app đọc bằng giọng Windows.
  Google chỉ có một giọng nữ, tốc độ thường; tốc độ và cao độ được chỉnh bằng ffmpeg. Giọng đọc chậm hơn Edge nên
  phim nói nhanh có thể vài câu dài hơn khoảng trống.
- Edge TTS là dịch vụ không chính thức, đôi khi trả lỗi `No audio was received` ngẫu nhiên. Khi đó phần mềm tự chuyển sang giọng Windows.
- Độ trễ đọc ≈ 0.6–1 giây sau khi phụ đề hiện (chủ yếu do mạng). Nếu Edge chậm quá `edge_deadline` thì câu đó được đọc bằng giọng Windows.
