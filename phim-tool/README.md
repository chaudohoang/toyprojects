# phim-tool

Lấy link các tập phim và tải về máy. Hỗ trợ phimvn2, tvhay/tvphim, motchill.

- Giao diện: `lay_link_tap_ui.pyw` (tab "Một phim" và tab "Hàng đợi" tải nhiều phim).
- Dòng lệnh: `python lay_link_tap.py <link phim>`.
- Cần Python 3 + `pip install -r requirements.txt` (httpx).
- ffmpeg: đặt `ffmpeg\ffmpeg.exe` cạnh tool (hoặc có trong PATH) để ghép .m3u8 thành .mp4.
  File này không có trong git vì quá 100 MB; lấy từ bản deploy `D:\PortableApps\phim-tool\ffmpeg`
  hoặc bản build "essentials" của gyan.dev.
- `hang_doi.json` (hàng đợi) do tool tự tạo cạnh file .pyw, mỗi máy một bản.

Ưu tiên phiên bản khi phim có nhiều bản: Lồng tiếng → Thuyết minh → Vietsub; bản kế tiếp làm
dự phòng nếu bản chính hỏng trên server (tra thêm kkphim/phimapi.com cho tvhay, tvphim, motchill).
