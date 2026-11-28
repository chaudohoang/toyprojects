# Camera Viewer

Xem tất cả camera trong mạng LAN trên một cửa sổ, mỗi camera một ô; bấm vào ô để xem lớn.
Thay cho việc cài EZStation / EZVIZ Studio trên từng máy.

- WinForms, .NET Framework 4.8 (có sẵn trên Windows 10/11), x64. Phát video bằng **LibVLC**
  (đi kèm trong `bin\Release\net48\libvlc\win-x64`, ~100 MB) — không cần cài VLC.
- Build: `build.bat` (hoặc `dotnet build -c Release`). Chạy: `bin\Release\net48\CameraViewer.exe`.
- **Bản đang dùng trên máy này:** `D:\PortableApps\CameraViewer`, có `cameras.ini` riêng. Cách cập nhật xem
  `CLAUDE.md` ở gốc repo; không chép đè `cameras.ini`.
- **Mang sang máy khác:** chép nguyên thư mục `bin\Release\net48` (kể cả `cameras.ini` nếu muốn
  giữ cấu hình + mật khẩu), chạy `CameraViewer.exe`. Không cần cài gì.

## Camera

Mặc định là 11 kênh của đầu ghi Uniview **NVR301-16S3 ở 192.168.100.100**, tài khoản `viewer`
(người dùng loại Người vận hành tạo riêng để xem; `admin` không ai biết mật khẩu).

| Dùng ở đâu | Địa chỉ |
|---|---|
| Ô trong lưới (luồng phụ, nhẹ) | `rtsp://192.168.100.100:554/unicast/c<kênh>/s1/live` |
| Xem lớn (luồng chính, nét, có tiếng) | `rtsp://192.168.100.100:554/unicast/c<kênh>/s0/live` |

Tài khoản dùng cho mỗi camera, theo thứ tự ưu tiên: `user:pass` ghi sẵn trong địa chỉ, rồi cột
**Tài khoản riêng / Mật khẩu riêng** của dòng đó, rồi tài khoản chung của đầu ghi trong Cài đặt.
Tài khoản được truyền cho libvlc qua `:rtsp-user` / `:rtsp-pwd`.

**Kiểm tra đăng nhập trước khi mở:** camera được nhóm theo thiết bị + tài khoản. Mỗi nhóm chỉ thử
đăng nhập **1 lần** (`RtspProbe.Check`); đúng thì mới mở các kênh, sai thì dừng hẳn đến khi bấm
Lưu trong Cài đặt hoặc F5. Đầu ghi Uniview từng khóa `viewer` vì 11 kênh cùng đăng nhập sai liên tục.

**EZVIZ** (Hikvision): C6N ở .9, Mini O ở .8, bóng đèn LB1 ở cửa ở .10. Trên firmware 2023 của C6N và
Mini O, cổng 554 ban đầu đóng. Trong EZVIZ Studio → Remote Configuration → Network → General, đổi
RTSP Port rồi để camera reboot thì cổng 554 mở. Đường dẫn: `/h264/ch1/main/av_stream` (luồng
phụ: `sub`). Tài khoản riêng `admin`, mật khẩu riêng là mã xác minh 6 chữ in hoa trên nhãn camera.

## Dùng

| Thao tác | Kết quả |
|---|---|
| Bấm vào ô / phím 1–9 | Xem lớn camera đó (luồng chính, có tiếng) |
| Bấm vào hình lớn / Esc | Quay lại lưới |
| ← / → | Camera trước / sau (khi xem lớn) |
| M | Bật/tắt tiếng (khi xem lớn) |
| F11 | Toàn màn hình (Esc để thoát) |
| F5 | Kết nối lại tất cả |
| Chuột phải | Menu: Cài đặt, toàn màn hình, thoát |

Cấu hình lưu ở `cameras.ini` cạnh exe (git bỏ qua file này vì có mật khẩu — chỉ mã hóa base64).

## Ghi chú kỹ thuật

- Mỗi ô một `MediaPlayer`; lưới thêm `:no-audio`. RTSP qua TCP (`:rtsp-tcp`), `:network-caching=500`.
- Tự kết nối lại: cứ 3 giây kiểm tra mỗi ô. Ended/Error/Stopped → mở lại sau 6 giây; kẹt
  Opening/Buffering quá 25 giây → mở lại; đang Playing nhưng 20 giây không có khung hình mới
  (`Media.Statistics.DecodedVideo` và `Time` đều đứng yên) → mở lại.
- Stop/Play chạy ngoài luồng UI: `MediaPlayer.Stop()` của libvlc có thể chặn trong lúc chờ cửa sổ video.
- Click dùng `MouseUp`, không dùng `MouseClick`: cửa sổ con của libvlc (đã disabled, nên click rơi
  xuống VideoView) nằm trên, mà WinForms chỉ bắn Click khi `WindowFromPoint` đúng là control đó.
- Lần chạy đầu Windows Firewall có thể hỏi quyền (live555 mở cổng UDP cho RTP). Chọn Cancel vẫn
  xem được vì app dùng RTSP qua TCP.
