# Tôi Đã Phân Tích Kỹ Thuật BG Wealth Sharing: Bên Trong Hạ Tầng Của Một Mạng Lừa Đảo "Giết Lợn" Tỷ Đô

*Phân tích kỹ thuật chuyên sâu của một kỹ sư phần mềm về BG Wealth Sharing, DSJEX, và hạ tầng đằng sau một trong những mạng lừa đảo tinh vi nhất đang hoạt động*

> **Cập nhật — Tháng 5/2026:** Nền tảng đã đóng băng tài khoản của tất cả nạn nhân và đang yêu cầu nộp thêm "phí mở khóa" để khôi phục quyền truy cập. Đây là giai đoạn cuối của trò lừa đảo trước khi kẻ vận hành biến mất. Toàn bộ bằng chứng đã được gửi đến 9 sàn giao dịch, Alibaba Cloud, FBI IC3, Cục An toàn Thông tin Việt Nam (NCSC), và Chainabuse.

---

## Ai Nên Đọc Bài Này

- Bất kỳ ai đã được tiếp cận về "BG Wealth Sharing" hoặc "DSJEX"
- Các nhà nghiên cứu bảo mật tiền mã hóa và điều tra blockchain
- Nhà báo đưa tin về tội phạm mạng có tổ chức tại Đông Nam Á
- Cơ quan thực thi pháp luật theo dõi mạng lưới gian lận tài chính
- Bất kỳ ai tò mò về cách các hoạt động lừa đảo hiện đại thực sự hoạt động

---

## Bối Cảnh — Tại Sao Tôi Điều Tra

Tôi đã bị nhắm mục tiêu bởi chiến dịch lừa đảo này. Sau khi bị lừa và xác minh trên Tronscan rằng tiền đã nộp bị quét đi trong vòng vài phút, tôi quyết định không bỏ cuộc. Tôi là kỹ sư phần mềm có kinh nghiệm về phát triển web, API và mạng máy tính. Thay vì nộp một báo cáo có thể chẳng đi đến đâu, tôi dành thời gian phân tích có hệ thống toàn bộ hạ tầng kỹ thuật của chiến dịch này.

Bài viết này ghi lại những gì tôi tìm thấy. Tất cả đều dựa trên điều tra kỹ thuật trực tiếp — phân tích lưu lượng mạng, giải mã JavaScript, ánh xạ điểm cuối API và truy vết giao dịch on-chain. Không có suy đoán nào ngoài những gì bằng chứng cho thấy.

---

## BG Wealth Sharing / DSJEX Là Gì?

Bề ngoài, BG Wealth Sharing tự giới thiệu là một nhóm đầu tư cộng đồng cung cấp "tín hiệu giao dịch" cho một sàn giao dịch tiền mã hóa có tên DSJEX (DSJ Exchange). Các thành viên được thông báo rằng họ có thể kiếm được lợi nhuận hàng ngày ổn định bằng cách làm theo các tín hiệu này trên nền tảng.

Bề ngoài được đánh bóng và chuyên nghiệp:

- Một nhân vật đầu mối — "Giáo sư Stephen Beard" — tự giới thiệu là chuyên gia tài chính hợp pháp
- Cuộc họp Zoom hàng tuần với hàng trăm người tham dự trên toàn cầu
- Sự kiện ngoại tuyến tổ chức ở nhiều quốc gia, mỗi sự kiện có hàng chục đến hàng trăm người tham gia
- Hệ thống đại lý 12 cấp với mức lương và yêu cầu thăng chức được xác định rõ ràng
- Tài liệu tiếp thị bóng bẩy bằng 14 ngôn ngữ: tiếng Anh, tiếng Việt, tiếng Trung, tiếng Hàn, tiếng Đức, tiếng Pháp, tiếng Ba Lan, tiếng Litva, tiếng Indonesia, tiếng Hungary, tiếng Bồ Đào Nha, tiếng Tây Ban Nha, tiếng Ý
- Nền tảng liên lạc mã hóa riêng tư (BonChat) được sử dụng riêng cho các thông tin liên lạc chính thức
- Bảng lợi nhuận giả mạo cho thấy mức tăng hàng ngày hoàn toàn mượt mà trong hơn 25 ngày liên tiếp không có phiên lỗ nào

Trên thực tế, DSJEX là một sàn giao dịch giả mạo. Số dư hiển thị là những con số trong cơ sở dữ liệu. Các tín hiệu giao dịch dẫn đến các giao dịch mô phỏng. Việc rút tiền bị khóa theo mặc định. Tiền nộp vào sẽ trực tiếp chuyển đến ví tiền mã hóa do kẻ lừa đảo kiểm soát và được chuyển đi trong vòng vài phút.

Đây là trò lừa đảo "giết lợn" — tiếng Trung gọi là 杀猪盘 (shā zhū pán). Thuật ngữ này ám chỉ quá trình vỗ béo lợn trước khi giết mổ — xây dựng lòng tin và thể hiện lợi nhuận giả mạo trước khi thực hiện vụ trộm cuối cùng.

---

## Cuộc Điều Tra Kỹ Thuật

### Bước 1 — Giao Diện Frontend

Trò lừa đảo sử dụng vô số tên miền frontend thay đổi xoay vòng trên nhiều đuôi tên miền. Trong khi một số theo mẫu như `dspp[số].tw`, những cái khác hoàn toàn ngẫu nhiên — ví dụ được xác nhận bao gồm `32dde.com`, `wdw3.vip`, `bgbg66.net`, `rfrt5.vip` và hàng chục cái khác. Những tên miền này hoàn toàn dùng một lần và thay đổi mỗi 24–48 giờ hoặc nhanh hơn. Khi Cloudflare chặn một cái, một tên miền mới sẽ hoạt động trong vài giờ và nạn nhân được chuyển hướng lặng lẽ qua thông báo BonChat. Không có ích gì khi cố gắng theo dõi hoặc báo cáo các tên miền riêng lẻ — chiến dịch tạo ra cái mới nhanh hơn bất kỳ quy trình gỡ xuống nào. Tên miền chỉ là điểm vào dùng một lần. Hạ tầng cơ bản ổn định — `api.ddjea.com`, bucket Alibaba Cloud OSS, các ví master — là thứ thực sự xác định chiến dịch này và không bao giờ thay đổi.

Frontend là Ứng dụng Trang Đơn Vue.js, được làm mờ nặng nề bằng javascript-obfuscator với sơ đồ mã hóa base64 tùy chỉnh. Mã nguồn chứa 32 chunk JavaScript tổng cộng khoảng 18MB mã bị làm mờ. Các thư viện chính được nhúng bao gồm:

- `npm.web3` và `npm.ethereumjs` — để kết nối ví tiền mã hóa
- `npm.klinecharts` — để hiển thị biểu đồ giao dịch giả mạo
- `npm.echarts` — để hiển thị biểu đồ lợi nhuận bịa đặt
- `npm.qrcode` — để tạo mã QR nạp tiền
- `ali-oss` — SDK Alibaba Cloud OSS để lưu trữ tài liệu KYC của nạn nhân

Chuỗi phiên bản `P2.9.3` xuất hiện nhất quán trong mọi header yêu cầu (`app-version: P2.9.3`), gợi ý đây là một bộ lừa đảo được phiên bản hóa được bán hoặc phân phối cho nhiều nhà vận hành.

### Bước 2 — Giải Mã JavaScript

Sau khi tải xuống tất cả 18 file JavaScript ứng dụng, tôi đã đảo ngược kỹ thuật mã hóa base64 tùy chỉnh được sử dụng bởi bộ làm mờ. Mỗi file chứa một mảng chuỗi gồm hàng nghìn giá trị được mã hóa. Bằng cách trích xuất hàm giải mã và chạy nó đối với mảng, tôi đã khôi phục **68.098 chuỗi được giải mã** trên tất cả các file.

Những phát hiện chính từ việc giải mã:

**141 điểm cuối API** — nhiều hơn gấp ba lần so với 42 điểm cuối ban đầu tìm thấy qua phân tích lưu lượng. Danh sách điểm cuối đầy đủ tiết lộ một hệ sinh thái sàn giao dịch giả mạo hoàn chỉnh bao gồm hợp đồng tương lai vĩnh cửu, giao dịch quyền chọn, tính năng casino/cờ bạc, giao dịch C2C, sản phẩm vay, bảng xếp hạng nhóm, hệ thống tài khoản demo, và nhiều hơn nữa.

**Ba tên miền backend** (không phải một):
- `api.ddjea.com` — backend chính
- `api.ddjeb.com` — backend phụ/dự phòng
- `api.dsjhout.com` — tên miền backend thứ ba
- `ds.dsjhout.com` — máy chủ phương tiện và file

**Định danh nội bộ duy nhất cho codebase này:**
- `yongxuDemo` — tên tài khoản thử nghiệm được hardcode, có mặt nguyên vẹn trong binary sản xuất
- `xchain.wxtome.link` — một tên miền liên kết chưa xác định, có thể là một chiến dịch khác trong cùng mạng lưới
- `pcchain.wxpass.net` — tên miền thử nghiệm nội bộ được tìm thấy dưới dạng văn bản thuần túy

**Điểm cuối WebSocket cho dữ liệu thời gian thực:**
- `/ws/app/perpetual` — nguồn cấp giá hợp đồng tương lai vĩnh cửu trực tiếp
- `/ws/app/chat/` — hệ thống chat trực tiếp
- `/ws/sub-quotes` — nguồn cấp dữ liệu thị trường

**Chuỗi Alibaba Cloud OSS** — xem Bước 4.

Định danh `yongxuDemo` đặc biệt hữu ích để lấy dấu vân tay: bất kỳ trang web nào trả về chuỗi này trong JavaScript của nó đều đang chạy cùng bộ lừa đảo, bất kể tên miền hay thương hiệu.

### Bước 3 — API Backend

Bằng cách chặn lưu lượng mạng khi duyệt trang web trực tiếp, tôi đã xác định API backend. API chạy trên Java Spring Boot, có thể nhận biết từ cấu trúc phản hồi phân trang và quy ước mã lỗi số.

Tất cả các yêu cầu đều bao gồm các header tùy chỉnh:
```
app-login-token: [session token]
app-version: P2.9.3
app-client-timezone: +8
app-analog: false
aws-check: true
```

Việc hardcode múi giờ (`+8`) kết hợp với hạ tầng Alibaba Cloud (xem Bước 4) gợi ý mạnh mẽ rằng người vận hành có trụ sở tại Trung Quốc đại lục.

Tôi đã ánh xạ **141 điểm cuối API riêng biệt** bao gồm toàn bộ chức năng nền tảng:

**Người dùng, xác thực và KYC:**
- `/api/app/user/login` và `/api/app/user/register`
- `/api/app/user/certification/ordinary` — nộp tài liệu KYC
- `/api/app/user/certification/advanced` — xác minh nâng cao
- `/api/app/user/google/auth` — Google 2FA
- `/api/app/user/withdrawal/password/status` — trạng thái khóa rút tiền

**Quỹ và ví:**
- `/api/app/funds/overview` — hiển thị số dư giả mạo
- `/api/app/funds/wallet/money` — chi tiết ví theo coin
- `/api/app/upload/s3/pre-signed` — cấp thông tin xác thực Alibaba Cloud OSS để tải lên tài liệu KYC

**Nạp tiền:**
- `/api/app/recharge/payment/submit`
- `/api/app/prompt/recharge` — thông báo nạp tiền

**Rút tiền (bị khóa theo thiết kế):**
- `/api/app/withdraw/submit`
- `/api/app/withdraw/address/coin`

**Giao dịch (tất cả đều mô phỏng):**
- `/api/app/second/order/submit/by/time` — lệnh quyền chọn
- `/api/app/perpetual/order/submit` — hợp đồng tương lai vĩnh cửu
- `/api/app/spot/submit` — giao dịch giao ngay
- `/api/app/game/turn` — casino/cờ bạc
- `/api/app/quotes/trade` — dữ liệu thị trường giả mạo

**Hạ tầng MLM/giới thiệu:**
- `/api/app/share/team` — quản lý downline
- `/api/app/rank/team` — bảng xếp hạng nhóm
- `/api/app/second/share/user/add` — đăng ký giới thiệu

**Hệ thống tài khoản demo:**
- `/api/app/demo/reset/assets` — xác nhận hệ thống tài khoản demo dùng để hiển thị lợi nhuận giả mạo cho khách hàng tiềm năng trước khi yêu cầu nộp tiền thật

### Bước 4 — Lấy Dấu Vân Tay Hạ Tầng

| Thành phần | Chi tiết |
|---|---|
| CDN Frontend | Cloudflare (104.21.x.x) |
| API Backend | api.ddjea.com · api.ddjeb.com · api.dsjhout.com |
| Máy chủ phương tiện | ds.dsjhout.com |
| **Lưu trữ file** | **Alibaba Cloud OSS — oss-cn-hangzhou (Hàng Châu, Trung Quốc)** |
| Liên lạc | BonChat (nhắn tin mã hóa tự host) |
| Framework backend | Java Spring Boot |
| Framework frontend | Vue.js + Webpack |
| Làm mờ | javascript-obfuscator với mã hóa base64 tùy chỉnh |
| Phiên bản bộ kit | P2.9.3 |
| Điểm cuối API | 141 điểm cuối được ánh xạ đầy đủ |
| WebSocket | /ws/app/perpetual · /ws/app/chat/ · /ws/sub-quotes |

**Đính chính quan trọng:** lưu trữ file là **Alibaba Cloud OSS**, không phải AWS S3. Điểm cuối tải lên `/api/app/upload/s3/pre-signed` có tên gây hiểu nhầm, nhưng việc giải mã JavaScript tiết lộ cách triển khai thực tế:

```
oss-cn-hangzhou               ← Trung tâm dữ liệu Hàng Châu, Trung Quốc
vpc100-oss-cn-[region]        ← Truy cập OSS riêng tư dựa trên VPC
OSSAccessKeyId                ← Xác thực Alibaba
x-oss-security-token          ← Token tạm thời STS
OSS4-HMAC-SHA256              ← Thuật toán ký v4 của Alibaba
ali-oss                       ← SDK npm Alibaba OSS
refreshSTSTokenInterval       ← Token tự làm mới phía client
```

Nạn nhân tải lên chứng minh nhân dân và ảnh selfie trong quá trình xác minh giả mạo. Những tài liệu này được lưu trữ trong một bucket Alibaba Cloud OSS tại Hàng Châu, Trung Quốc — dưới một tài khoản được đăng ký bằng danh tính Trung Quốc thật. Tên bucket được cấp động theo từng phiên qua điểm cuối `/api/app/upload/s3/pre-signed`.

Báo cáo lạm dụng đã được nộp cho Alibaba Cloud: **Mã trường hợp #20260502710120867026**.

### Bước 5 — Xác Nhận Số Dư Giả Mạo

Sử dụng session token đã thu thập và các header yêu cầu đúng, tôi đã trực tiếp kiểm tra API trực tiếp.

Điểm cuối `/api/app/funds/overview` trả về:
```json
{
  "resultCode": true,
  "errCode": 0,
  "data": {
    "usdtTotal": 1129.5,
    "usdtAvailable": 1129.5,
    "usdtUnavailable": 0.0
  }
}
```

Điểm cuối `/api/app/user/withdrawal/password/status` trả về:
```json
{
  "resultCode": true,
  "errCode": 0,
  "data": false
}
```

Điều này xác nhận bẫy hai lớp: số dư được hiển thị là một con số bịa đặt được lưu trữ trong cơ sở dữ liệu của chúng, và việc rút tiền bị khóa theo quản trị. Nạn nhân thấy tiền mà họ không thể truy cập và sau đó được thông báo rằng họ phải trả "thuế," "phí xác minh" hoặc "phí mở khóa" để giải phóng — tất cả đều trực tiếp đến tay kẻ lừa đảo.

---

## Cơ Cấu Tổ Chức

Ngoài hạ tầng kỹ thuật, các tài liệu quảng cáo tôi thu được tiết lộ một cơ cấu tổ chức đa cấp tinh vi:

**Hệ thống phân cấp kim tự tháp** chạy 12 cấp, từ Cấp 1 (5 người tuyển trực tiếp, lương tối thiểu $30/tháng) đến Cấp 12 (300.000 thành viên nhóm, bảo hiểm $1.000.000/tháng). Cấu trúc này về mặt toán học giống hệt một sơ đồ kim tự tháp — chỉ có thể duy trì thông qua việc tuyển dụng liên tục thành viên mới.

**Cơ chế tuyển dụng** sử dụng đồng thời ba đòn bẩy tâm lý:
- Khuyến khích tài chính (hoa hồng giới thiệu lên đến 10% tiền nạp của thành viên mới)
- Áp lực xã hội (cuộc gọi Zoom hàng tuần, sự kiện ngoại tuyến, yêu cầu xây dựng nhóm)
- Sợ bỏ lỡ cơ hội (chiến dịch IPO có thời hạn, cấp tín hiệu độc quyền được mở khóa bằng tuyển dụng)

**Kỹ thuật tiêm chủng** đặc biệt tinh vi. BG phân phát "Mẹo Chống Lừa Đảo" cho tất cả thành viên cảnh báo họ bỏ qua bất kỳ ai ngoài kênh BonChat, không tin tưởng các cảnh báo khẩn cấp, và xác minh mọi thứ qua các kênh chính thức. Đây là một kỹ thuật tuyển dụng giáo phái đã được ghi lại — nó khiến nạn nhân bác bỏ các cảnh báo hợp pháp từ gia đình và bạn bè.

**Hạ tầng bot** duy trì ảo giác về một cộng đồng hoạt động 24/7 thông qua các tin nhắn tự động trong nhóm chat.

**Nhân vật "Giáo sư Stephen Beard"** gần như chắc chắn là một diễn viên được trả tiền hoặc danh tính được xây dựng. Những người vận hành thực sự vẫn ẩn danh sau nhân vật tiền tuyến này.

---

## Tại Sao Chiến Dịch Này Khác Biệt

**Bảo mật vận hành:** Không có danh tính thật nào bị lộ. Xoay vòng tên miền có nghĩa là không có bề mặt frontend ổn định. Cloudflare có nghĩa là không lộ máy chủ. BonChat có nghĩa là không rò rỉ tin nhắn Telegram. Phân lớp chuỗi có nghĩa là không có dấu vết on-chain sạch.

**Chiều sâu tổ chức:** Hệ thống phân cấp 12 cấp có nghĩa là hàng nghìn người trên toàn cầu có lợi ích tài chính trong việc tuyển dụng nạn nhân mới.

**Mô hình nhượng quyền:** Chuỗi phiên bản `P2.9.3` và chất lượng bộ kit được đánh bóng gợi ý hạ tầng này được bán hoặc cấp phép cho các chiến dịch khác.

**Độ tinh vi tâm lý:** Tiêm chủng chống lừa đảo, bằng chứng xã hội bot, nhân vật quyền lực giáo sư giả mạo, câu chuyện khẩn cấp "IPO" — đây không phải là ngẫu hứng. Chúng phản ánh kiến thức về phương pháp tuyển dụng giáo phái và tâm lý hành vi được áp dụng ở quy mô lớn.

**Nhắm mục tiêu toàn cầu:** Tiếp thị bằng 14 ngôn ngữ, sự kiện tuyển dụng ngoại tuyến trên nhiều châu lục và hardcode múi giờ +8 trong khi nhắm mục tiêu nạn nhân ở châu Âu, Đông Nam Á, Hàn Quốc và Bắc Mỹ.

---

## Những Gì Tôi Không Thể Xác Định Một Mình

- IP máy chủ thực sự đằng sau Cloudflare yêu cầu trát hầu tòa của Cloudflare
- Chủ tài khoản Alibaba Cloud OSS yêu cầu sự hợp tác của Alibaba với cơ quan thực thi pháp luật
- Tài khoản sàn giao dịch được liên kết với địa chỉ nạp tiền cuối cùng yêu cầu sự hợp tác của sàn
- Dòng tiền đầy đủ yêu cầu công cụ phân tích blockchain chuyên nghiệp (cấp Chainalysis, TRM Labs)
- Danh tính của "Giáo sư Stephen Beard" yêu cầu nguồn lực điều tra mà tôi không có

Tất cả đều khả thi — nhưng yêu cầu thẩm quyền pháp lý hoặc công cụ chuyên nghiệp.

---

## Các Báo Cáo Đã Nộp

Các báo cáo sau đây đã được nộp tính đến tháng 5/2026:

**Sàn giao dịch (tất cả 9 sàn với bằng chứng giao dịch cụ thể):**
Binance · Bybit · OKX · Gate.io · Bitget · MEXC · KuCoin · HTX · Bitfinex

**Hạ tầng:**
- Báo cáo lạm dụng Alibaba Cloud — Mã trường hợp **#20260502710120867026**

**Thực thi pháp luật và cơ sở dữ liệu:**
- FBI IC3: `https://ic3.gov`
- Cục An toàn Thông tin Việt Nam (NCSC): `https://canhbao.ncsc.gov.vn`
- Chainabuse: `https://www.chainabuse.com`
- Tổ chức Chống Lừa Đảo Toàn cầu: `https://www.globalantiscam.org`

---

## Bạn Có Thể Làm Gì

**Nếu bạn đã bị nhắm mục tiêu hoặc là nạn nhân:**
- Báo cáo lên Chainabuse: `https://www.chainabuse.com`
- Báo cáo lên Tổ chức Chống Lừa Đảo Toàn cầu: `https://www.globalantiscam.org`
- Báo cáo lên đơn vị tội phạm mạng quốc gia của bạn
- Nếu là người Việt Nam: Cục An toàn Thông tin — `https://canhbao.ncsc.gov.vn`
- Nếu ở Mỹ: FBI IC3 tại `https://ic3.gov`

**Nếu bạn là nhà nghiên cứu hoặc nhà báo:**
Tôi đã ghi lại bản đồ điểm cuối API, dấu vân tay hạ tầng, tài liệu quảng cáo, tài liệu cơ cấu tổ chức, đầu ra giải mã JavaScript (68.098 chuỗi được giải mã) và dấu vết giao dịch on-chain đầy đủ đến 7 sàn giao dịch. Tôi sẵn sàng chia sẻ tài liệu kỹ thuật đầy đủ với các nhà nghiên cứu, nhà báo hoặc cơ quan thực thi pháp luật có thông tin xác thực.

**Nếu bạn là cơ quan thực thi pháp luật hoặc công ty phân tích blockchain:**
Các tên miền backend `api.ddjea.com`, `api.ddjeb.com`, `api.dsjhout.com` là những neo hạ tầng ổn định. Bucket Alibaba Cloud OSS trong `oss-cn-hangzhou` chứa tài liệu KYC của nạn nhân. Các địa chỉ ví tổng hợp on-chain có thể xác định được qua các dấu vết Tronscan được ánh xạ bên dưới. Chuỗi phiên bản bộ kit `P2.9.3` và định danh nội bộ `yongxuDemo` có thể liên kết với các triển khai khác của cùng nền tảng.

---

## Ghi Chú Cuối

Bài viết này tồn tại vì tôi tin rằng ánh sáng mặt trời là loại thuốc khử trùng hiệu quả nhất cho các hoạt động như thế này. Các báo cáo chính thức biến mất vào hàng đợi. Tài liệu công khai thu hút các nhà nghiên cứu, nhà báo và điều tra viên có công cụ để đi xa hơn tôi có thể làm một mình.

Nếu bạn biết thêm về chiến dịch này, đã bị ảnh hưởng bởi nó, hoặc có nguồn lực điều tra có thể giúp ích — xin hãy liên hệ.

Những người điều hành BG Wealth Sharing đã lấy cắp tiền của hàng chục nghìn nạn nhân ở nhiều quốc gia, hoạt động nhiều năm mà không bị hậu quả, và xây dựng hạ tầng được thiết kế đặc biệt để khiến chúng không thể truy vết với cá nhân. Chúng không phải là không thể truy vết với một nỗ lực phối hợp.

---

*Cuộc điều tra này được thực hiện bằng các công cụ mã nguồn mở, dữ liệu blockchain công khai và các kỹ thuật phát triển web tiêu chuẩn. Không có hệ thống nào bị xâm phạm. Tất cả các thăm dò API được thực hiện bằng token phiên hợp lệ từ tài khoản của riêng tôi trên nền tảng.*

---

## Bằng Chứng On-Chain — Các Con Số

*Dữ liệu sau đây được thu thập thông qua truy vết blockchain trực tiếp bằng API công khai của TronScan.*

### Xác Minh On-Chain Dòng Tiền Nạp Của Nạn Nhân

Địa chỉ nạp tiền `TMU5v2zLvYeouqyQKtG1AA3AFRSFUdxTLb` cho thấy mẫu quét tiêu chuẩn của nền tảng — tiền được chuyển tự động đến ví tổng hợp master trong vòng vài phút sau khi nạp, không có bước thủ công nào.

### Ví Tổng Hợp Master Chính

`TWF1bcuNLXtwQCRxb6Qn6uKqWCuXRatiei`

- Số giao dịch đến: **101.902 giao dịch**
- Tổng nhận được: **~1.000.000.000+ USDT**
- Hoạt động: Tháng 10/2025 – Tháng 5/2026

### Kiến Trúc Dòng Tiền

Phân tích giao dịch được truy vết trên 15 ví master và 450+ giao dịch được xác nhận tiết lộ hai mẫu rửa tiền khác nhau:

**Mẫu A (đơn giản):**
```
Ví Master → [1–2 Ví Trung Gian] → Ví Nóng Sàn Giao Dịch
```

**Mẫu B (tổng hợp):**
```
Ví Master → [1–2 Ví Trung Gian] → Master Tổng Hợp
          → [1–2 Ví Trung Gian] → Ví Nóng Sàn Giao Dịch
```

Ví trung gian được xác nhận: `TPcgRgCbc34BoFgLG4QSy4S9PYcFAe63W4` — 24 giao dịch xác nhận đến Binance Hot 7, tổng cộng $87.689 USDT.

### Rút Tiền Cuối Cùng — 9 Sàn Giao Dịch, 15 Ví

| Sàn | Nhãn | Địa chỉ | Số dư ước tính |
|---|---|---|---|
| Binance | Hot 7 | TDqSquXBgUCLYvYC4XZgrprLK589dkhSCf | $455.686.673 |
| Binance | Hot 6 | TJDENsfBJs4RFETt1X1W8wMDc8M5XnJhCe | $65.268.735 |
| Binance | Hot 5 | TAzsQ9Gx8eqFNFSKbeXrbi45CuVPHzA8wr | $64.459.689 |
| Binance | Hot 11 | TJqwA7SoZnERE4zW5uDEiPkbz4B66h9TFj | $46.506.000 |
| Binance | Hot 4 | TNXoiAJ3dct8Fjg4M9fkLFh9S2v9TXc32G | $45.189.581 |
| Binance | Hot 3 | TYASr5UV6HEcXatwdFQfmLVUqQQQMUxHLS | $65.160.131 |
| Bybit | Hot | TU4vEruvZwLLkSfV9bNw12EJTPvNr7Pvaa | $304.996.832 |
| Gate.io | Hot | TBA6CypYJizwA9XdC7Ubgc5F1bxrQ7SqPt | $235.865.678 |
| OKX | Hot 8 | TLaGjwhvA8XQYSxFAcAXy7Dvuue9eGYitv | $159.892.199 |
| OKX | Hot 1 | TM1zzNDZD2DPASbKcgdVoTYhfmYgtfwx9R | $234.505 |
| Bitget | Hot 9 | TJ7hhYhVhaxNx6BPyq7yFpqZrQULL3JSdb | $58.078.026 |
| MEXC | Hot | TEPSrSYPDSQ7yXpMFPq91Fb1QEWpMkRGfn | $77.353.677 |
| KuCoin | Hot 4 | TUpHuDkiCCmwaTZBHZvQdwWzGNm5t8J2b9 | $34.850.867 |
| HTX | Hot 4 | TFTWNgDBkQ5wQoP8RXpRznnHvAVV8x5jLu | $5.812.996 |
| Bitfinex | Hot | TXFBqBbqJommqZf7BV8NNYzePh97UmJodJ | $18.861.054 |

**Lưu ý:** Đây là ví nóng của sàn giao dịch — hạ tầng dùng chung phục vụ hàng triệu người dùng. Địa chỉ nạp tiền cá nhân của kẻ lừa đảo nằm một bước trước những ví này. Các sàn giao dịch có hồ sơ KYC cho những tài khoản cá nhân đó.

### 15 Ví Master Đã Xác Định

`TFBXCAz2uoFUchiYxDCWA8NmKxwCmMJNwg` · `TLHn4hvwcRcbT8T5zz4icZSYP9ycKQeMya` · `TX9MCjMWpJpm4qRCpbbbwWXYGwBab1Xp7a` · `TTDkfRZ83VfXW4M3ayf6QSrteH6EkG3ZVn` · `TPiDX9JrcHSxh9ekZCxEcAAR4MGMWYFoft` · `TBPQSHyoRprSADr9uDJzzYFUse6r5fUFvZ` · `TVu2uAeZfixfSQtKZeh4ztS8Y9zszHo8MZ` · `TRDjFwWXN9XjPxeBEwXjABncKzdqSuLxsa` · `TF1n6ykE17g8M9apkRgx3yJZpMwicPmPmk` · `TK7PTtpEaoXiHdX4qW7gHieKcEJdZNnaPK` · `THhqGS6izRWFWHNM3kaupCsNBDwEmbrUcq` · `TX3hfkdQF3gqr8Vz4DyVneWfBi7HFeGfPT` · `TXSVyoshhSD71vZEbJesK11VMRQN69UP1C` · `TAkZnpPoRX5ttbQbQJTrXNJUBci1XcgGub`

Tất cả địa chỉ ví và dòng giao dịch có thể xác minh công khai trên [TronScan](https://tronscan.org).

---

*Cập nhật lần cuối: Tháng 5/2026*
