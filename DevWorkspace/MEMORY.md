# 🧠 Long-term Memory

## 📁 Cấu trúc Workspace
- Thư mục gốc: tùy máy (vd: `E:\Dev`, `D:\Projects`) — Môi trường test/sandbox đa ngôn ngữ. Không giả định đường dẫn tuyệt đối giống nhau giữa các máy.
- Chứa các thư mục ngôn ngữ (`python`, `c#`, `java`...) và cấu hình cho Agent (`AGENTS.md`, `MEMORY.md`...).

## 🤖 Dự án Telegram Bot AI (`python/telegram_bot`)
- **Tên Bot:** Trợ lý Pika
- **Tính cách (System Prompt):** Ngoan ngoãn, vâng lời, xưng "Em" và gọi người dùng là "Sếp", thường xuyên sử dụng emoji dễ thương.
- **Thư viện sử dụng:**
  - `python-telegram-bot` (Xử lý tin nhắn Telegram)
  - `google-generativeai` (Kết nối não Gemini)
  - `python-dotenv` (Đọc file cấu hình bảo mật)
- **Cấu hình bắt buộc (File `.env`):**
  - Cần cung cấp `TELEGRAM_BOT_TOKEN` (từ BotFather) và `GEMINI_API_KEY` (từ Google AI Studio).
- **Mô hình AI đang dùng:** `gemini-flash-latest` (Do tài khoản Free Tier hiện tại bị giới hạn Quota = 0 đối với bản Pro, dùng bản Flash để đảm bảo miễn phí và tốc độ siêu tốc).
- **Các Lưu ý Kỹ thuật quan trọng (Đã Fix):**
  1. **Lỗi hiển thị Terminal Windows:** Đã thêm `sys.stdout.reconfigure(encoding='utf-8')` để terminal không bị crash khi in các ký tự Emoji.
  2. **Lỗi Asyncio Python 3.14:** Thư viện Telegram v21 trên Python 3.14 hay bị lỗi "No current event loop". Đã fix bằng cách ép tạo loop mới ở đầu hàm `main()`.
  3. **Cập nhật Source:** Mỗi khi sửa code `bot.py` hoặc `.env`, MẶC ĐỊNH phải ngắt lệnh cũ (Ctrl+C) và gõ `python bot.py` chạy lại.

## 📝 Quy tắc làm việc của Agent
- Luôn cập nhật thông tin quan trọng vào `MEMORY.md`.
- Ghi log công việc hàng ngày vào thư mục `memory/YYYY-MM-DD.md`.
- Để duy trì "trí nhớ" khi đổi máy tính, Sếp cần đồng bộ cả file `MEMORY.md` và thư mục `memory/` sang máy mới.

## 🚀 Ghi chú cho Hệ thống Đa máy tính (Multi-Device)
Sếp muốn code và chạy Bot trên nhiều máy tính khác nhau (mỗi lúc ngồi 1 máy, chỉ chạy 1 máy tại một thời điểm).
- **Đồng bộ:** Sử dụng phần mềm **Syncthing** để đồng bộ tự động thư mục workspace (đường dẫn tùy máy: `E:\Dev`, `D:\Projects`...). Thư mục `.git` KHÔNG được đồng bộ — mỗi máy giữ repo riêng, dùng git push/pull qua remote.
- **Cấu hình Syncthing Ignore:** BẮT BUỘC phải bỏ qua thư mục môi trường ảo và rác của Python để tránh xung đột. Cấu hình file `.stignore`:
  ```text
  .venv
  __pycache__
  ```
- **Quy tắc chạy:** Tại một thời điểm, chỉ được phép chạy lệnh `python bot.py` trên MỘT MÁY TÍNH DUY NHẤT. Nếu quên tắt bot ở máy ở công ty mà về nhà bật lên chạy tiếp, Telegram sẽ văng lỗi Conflict 409 ngay lập tức.
- **Tiện ích quản lý Bot (nằm trong `python/telegram_bot/`):**
  - `resume_bot.bat`: Tự tạo `.venv` và cài đặt `requirements.txt` khi mới đồng bộ sang máy khác.
  - `start_bot_hidden.vbs`: Chạy bot ẩn dưới nền hệ thống (không mở cửa sổ).
  - `stop_bot.bat`: Tìm và tắt tiến trình bot đang chạy nền một cách an toàn.
