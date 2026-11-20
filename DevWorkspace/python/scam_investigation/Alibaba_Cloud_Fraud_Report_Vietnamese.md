# Báo Cáo Lạm Dụng Alibaba Cloud — Lừa Đảo "Giết Lợn" (BG Wealth Sharing / DSJEX)

**Loại báo cáo:** Lừa đảo lấy danh nghĩa đầu tư / Gian lận tài chính
**Ngày:** Tháng 5/2026
**Trạng thái:** Chiến dịch vẫn đang hoạt động tính đến tháng 5/2026

---

## 1. Tóm Tắt

Báo cáo này ghi lại một chiến dịch lừa đảo tiền mã hóa quy mô lớn đang hoạt động (lừa đảo giết lợn / 杀猪盘) đang sử dụng hạ tầng Alibaba Cloud OSS để lưu trữ tài liệu KYC bị đánh cắp của nạn nhân. Nền tảng hoạt động như một sàn giao dịch tiền mã hóa giả mạo có tên DSJEX dưới thương hiệu "BG Wealth Sharing."

Chiến dịch đã lừa đảo khoảng **hơn 100.000 nạn nhân** số tiền ước tính **hơn 1.000.000.000 USDT** và vẫn đang hoạt động đầy đủ.

---

## 2. Hạ Tầng Các Tên Miền

| Tên miền | Vai trò |
|---|---|
| api.ddjea.com | API backend CHÍNH |
| api.ddjeb.com | API backend PHỤ — sao lưu |
| api.dsjhout.com | Tên miền backend thứ ba |
| ds.dsjhout.com | Máy chủ phương tiện/file |

Các tên miền frontend thay đổi hàng ngày (mẫu: dspp[n].tw) — các tên miền backend ở trên là neo hạ tầng ổn định.

---

## 3. Bằng Chứng Alibaba Cloud OSS

Các chuỗi sau được trích xuất trực tiếp từ JavaScript frontend của nền tảng thông qua giải mã 18 file JS (68.098 chuỗi được giải mã):

### Vùng và Điểm Cuối
- **Vùng OSS:** `oss-cn-hangzhou` — trung tâm dữ liệu Hàng Châu, Trung Quốc
- **Mẫu điểm cuối:** `vpc100-oss-cn-[region].aliyuncs.com`
- **Điểm cuối nội bộ:** `-internal.aliyuncs.com`

### Xác Thực
- **Phương thức:** Token tạm thời STS
- **Header:** `x-oss-security-token`
- **Làm mới token:** `refreshSTSTokenInterval` — tự làm mới phía client
- **Trường khóa truy cập:** `OSSAccessKeyId`
- **Header thông tin xác thực:** `x-oss-credential`

### Ký
- **Thuật toán:** `OSS4-HMAC-SHA256`
- **Phiên bản:** `aliyun_v4` / `aliyun_v4_request`

### SDK
- **Gói:** `ali-oss` npm package được nhúng trong JS frontend
- **Định dạng ARN:** `acs:oss:::` xác nhận việc sử dụng Alibaba Cloud thật sự

### Điểm Cuối Tải Lên
- `/api/app/upload/s3/pre-signed` — cấp thông tin xác thực OSS theo từng phiên
- Tên bucket được cấp động theo từng phiên (không được hardcode trong JS)

### Các Header OSS Được Tìm Thấy Trong JS
```
x-oss-security-token
x-oss-credential
x-oss-signature
x-oss-callback
x-oss-callback-var
x-oss-meta-
x-oss-object-acl
x-oss-copy-source
x-oss-date
x-oss-expires
x-oss-process
x-oss-request-id
x-oss-user-agent
x-oss-server-side-encryption
x-oss-storage-class
```

### Cảnh Báo SDK Alibaba (tìm thấy nguyên văn trong JS)
> "Please use STS Token for safety, see more details at https://help.aliyun.com/document_detail/32077.html"

Điều này xác nhận SDK Alibaba OSS thật sự đang được sử dụng, không phải triển khai giả mạo.

---

## 4. Tài Liệu Được Lưu Trong Bucket OSS

Điểm cuối `/api/app/upload/s3/pre-signed` được gọi trong quá trình KYC (xác minh danh tính) của nạn nhân. Các tài liệu được lưu trữ bao gồm:

- Ảnh chứng minh nhân dân/hộ chiếu (mặt trước và mặt sau)
- Ảnh selfie cầm tài liệu tùy thân
- Thông tin cá nhân được nộp trong quá trình xác minh giả mạo

Những tài liệu này thuộc về các nạn nhân lừa đảo và cấu thành dữ liệu cá nhân nhạy cảm cần được bảo vệ như bằng chứng pháp lý.

---

## 5. Bằng Chứng On-Chain Của Nạn Nhân

- **Địa chỉ nạp tiền của nạn nhân:** `TMU5v2zLvYeouqyQKtG1AA3AFRSFUdxTLb`
- **TronScan:** https://tronscan.org/#/address/TMU5v2zLvYeouqyQKtG1AA3AFRSFUdxTLb
- **Ví tổng hợp master:** `TWF1bcuNLXtwQCRxb6Qn6uKqWCuXRatiei`
- **TronScan:** https://tronscan.org/#/address/TWF1bcuNLXtwQCRxb6Qn6uKqWCuXRatiei
- **Giao dịch đến:** 101.902 giao dịch
- **Tổng nhận được:** ~1.000.000.000+ USDT
- **Mẫu quét:** Tiền nạn nhân chuyển đến ví master trong vòng vài phút — tự động

---

## 6. Tóm Tắt Hạ Tầng Kỹ Thuật

| Thành phần | Chi tiết |
|---|---|
| Framework backend | Java Spring Boot |
| Framework frontend | Vue.js + Webpack |
| CDN / Proxy | Cloudflare |
| Lưu trữ file | Alibaba Cloud OSS (oss-cn-hangzhou) |
| Liên lạc | BonChat nhắn tin mã hóa tự host |
| Làm mờ | javascript-obfuscator với mã hóa base64 tùy chỉnh |
| Phiên bản bộ kit | P2.9.3 (trong tất cả các header yêu cầu) |
| Điểm cuối API | 141 điểm cuối được ánh xạ đầy đủ |
| WebSocket | wss://[domain]/ws/app/perpetual, /ws/app/chat/, /ws/sub-quotes |

---

## 7. Dấu Vân Tay Codebase Duy Nhất

Các định danh này là duy nhất cho bộ kit lừa đảo này và có thể được sử dụng để xác định các triển khai khác:

- **`yongxuDemo`** — tên tài khoản thử nghiệm được hardcode — duy nhất cho bộ kit này
- **`P2.9.3`** — phiên bản bộ kit trong tất cả các header HTTP
- **`pcchain.wxpass.net`** — tên miền thử nghiệm nội bộ trong văn bản thuần túy
- **`xchain.wxtome.link`** — tên miền liên kết, có thể là một chiến dịch khác
- **`app-version: P2.9.3`** — header HTTP tùy chỉnh gửi với mỗi yêu cầu API

---

## 8. Quy Mô Của Chiến Dịch

- Nhắm mục tiêu nạn nhân bằng 14+ ngôn ngữ: tiếng Anh, tiếng Việt truyền thống, tiếng Trung, tiếng Hàn, tiếng Đức, tiếng Pháp, tiếng Ba Lan, tiếng Litva, tiếng Indonesia, tiếng Hungary, tiếng Bồ Đào Nha, tiếng Tây Ban Nha, tiếng Ý và nhiều hơn nữa
- Cơ cấu kim tự tháp MLM 12 cấp với mức lương được xác định
- Cuộc họp Zoom hàng tuần với hàng trăm người tham dự toàn cầu
- Sự kiện tuyển dụng ngoại tuyến trên nhiều quốc gia
- Hệ thống bot tự động duy trì hoạt động cộng đồng giả mạo 24/7
- Hệ thống tài khoản demo để hiển thị lợi nhuận giả mạo cho khách hàng tiềm năng trước khi nạp tiền thật

---

## 9. Yêu Cầu Đến Alibaba Cloud

1. Xác định bucket OSS và chủ tài khoản đang nhận tải lên từ `api.ddjea.com`
2. Bảo tồn tất cả tài liệu KYC của nạn nhân được lưu trong bucket OSS như bằng chứng pháp lý
3. Đình chỉ/đóng băng tài khoản OSS và tất cả các dịch vụ liên quan
4. Chia sẻ chi tiết đăng ký tài khoản, hồ sơ thanh toán và nhật ký IP với cơ quan thực thi pháp luật có liên quan
5. Xác định và đình chỉ các dịch vụ cho các tên miền liên quan: `api.ddjeb.com`, `api.dsjhout.com`, `ds.dsjhout.com`
6. Cung cấp số mã trường hợp để theo dõi

---

## 10. Các Báo Cáo Khác Đã Nộp

Trường hợp này cũng đã được báo cáo cho:
- Binance, Bybit, OKX, Gate.io, Bitget, MEXC, KuCoin, HTX, Bitfinex
- Chainabuse: https://www.chainabuse.com
- Tổ chức Chống Lừa Đảo Toàn cầu: https://www.globalantiscam.org
- Cục An toàn Thông tin Việt Nam (NCSC): https://canhbao.ncsc.gov.vn
- FBI IC3: https://ic3.gov

---

## 11. File Bằng Chứng Đính Kèm

- `Alibaba_Cloud_Fraud_Report.xlsx` — bằng chứng có cấu trúc bao gồm tất cả chuỗi OSS, 141 điểm cuối API và chi tiết hạ tầng
- `MASTER_REPORT.json` — đầu ra thô từ giải mã JS (68.098 chuỗi được giải mã từ 18 file JavaScript)

---

*Báo cáo này được chuẩn bị bởi một nhà nghiên cứu bảo mật độc lập và nạn nhân lừa đảo. Tất cả dữ liệu blockchain có thể xác minh công khai trên TronScan. Tất cả các chuỗi JavaScript được trích xuất thông qua giải mã tiêu chuẩn của mã frontend được phục vụ công khai.*
