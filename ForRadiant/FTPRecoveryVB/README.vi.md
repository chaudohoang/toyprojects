# FTP Recovery — Hướng dẫn sử dụng

Công cụ này hoàn tất những lần upload bị kẹt.

Đôi khi một panel dừng giữa chừng khi đang upload lên server của khách hàng. Ảnh vẫn
nằm trên máy, một số file có thể đã lên server, nhưng panel không bao giờ hoàn tất —
nên khách hàng không nhận được danh sách file cuối cùng, và với họ thì coi như không
có gì được gửi lên cả.

Công cụ này tìm ra những panel đó, upload phần còn thiếu, rồi gửi danh sách cuối cùng
để panel được hoàn tất đúng cách.

---

## Khi nào cần dùng

Khi gặp một trong các trường hợp sau:

- Panel nằm trong thư mục upload hàng giờ hoặc hàng ngày mà không có gì xảy ra
- Khách hàng báo thiếu file của một panel
- Thư mục upload cứ đầy lên mà không bao giờ vơi
- Sau khi mất mạng hoặc server gặp sự cố, upload dừng giữa chừng

---

## Trước khi bắt đầu

**1. Tắt FTPUploader.**

Điều này rất quan trọng. Hai chương trình cùng ghi vào một file, nếu chạy đồng thời
sẽ xung đột và có thể làm hỏng bản ghi những gì đã gửi. Hãy tắt và giữ nguyên như vậy
cho đến khi làm xong.

**2. Kiểm tra server của khách hàng có kết nối được không.**

Nếu không kết nối được, công cụ sẽ tự dừng sau vài lần thất bại thay vì mất thời gian
— nhưng kiểm tra trước vẫn nhanh hơn.

**3. Không cần chuẩn bị gì thêm.**

Bạn có thể chạy bao nhiêu lần cũng được. Công cụ không bao giờ upload lại file đã
upload, nên nếu chưa chắc chắn thì cứ chạy và xem kết quả trước khi làm gì tiếp.

---

## Cách sử dụng

Nhấp đúp vào **FTPRecoveryGUI.exe**. Cửa sổ mở ở chế độ toàn màn hình.

Thư mục upload đã được điền sẵn. Nếu sai, nhấn **Browse**.

### Bước 1 — Xem trước

Nhấn **Start Scan**.

Bước này chỉ để xem. Không có gì được upload. Sau vài giây bạn sẽ thấy bảng liệt kê
mọi panel đang kẹt và điều gì sẽ xảy ra với từng panel.

### Bước 2 — Đọc bảng

Mỗi dòng là một panel:

| Cột | Ý nghĩa |
|---|---|
| **PID** | mã của panel |
| **Total** | panel này phải có bao nhiêu file |
| **Host now** | đã ghi nhận gửi được bao nhiêu file |
| **Done** | đã gửi rồi, không cần làm gì thêm |
| **Retry** | trước đó thất bại, sẽ thử lại |
| **New** | chưa gửi |
| **Rebuilt** | file tìm thấy trên máy mà trước đó bị bỏ sót |
| **Projected** | số file cuối cùng sẽ đạt được |
| **Verdict** | điều gì sẽ xảy ra |

Màu sắc cho biết tình trạng ngay lập tức:

| Màu | Ý nghĩa |
|---|---|
| Trắng | sẽ hoàn tất bình thường |
| Xanh dương | file đã gửi hết; chỉ còn gửi danh sách cuối |
| **Tím nhạt** | sẽ hoàn tất, nhưng **chỉ nhờ tìm lại được file bị bỏ sót trên máy** |
| **Hồng** | **không thể hoàn tất** — xem cột Verdict để biết lý do |
| Xanh lá | đã hoàn tất thành công |
| Cam | đã hoàn tất nhưng bị thiếu — nên kiểm tra lại |

Dòng màu tím nhạt đáng xem kỹ hơn. Chúng sẵn sàng, nhưng đường dẫn đích của những
file được tìm lại là do suy ra chứ không phải đọc từ file lệnh. Cột Rebuilt và cột
Verdict đều cho biết số lượng.

Panel chưa xử lý nằm ở trên. Sau khi upload xong, chúng chuyển xuống dưới kèm kết
quả, nên bảng trở thành bản ghi lại những gì bạn đã làm.

### Bước 3 — Thử một panel trước

Nhấn **Upload** ở một dòng. Theo dõi phần log bên phải.

Nếu thấy ổn thì làm tiếp. Cách này luôn an toàn hơn là chạy tất cả ngay từ đầu.

### Bước 4 — Làm phần còn lại

Nhấn **Upload ALL panels**.

Bạn có thể nhấn **Stop** bất cứ lúc nào. Chương trình sẽ hoàn tất file đang xử lý rồi
dừng lại sạch sẽ. Không mất gì cả — chạy lại sau đó sẽ tiếp tục từ chỗ đang dở.

---

## Ý nghĩa của cột Verdict

### Tốt

| Verdict | Ý nghĩa |
|---|---|
| `READY - 22 to upload` | còn 22 file cần gửi, sau đó panel hoàn tất |
| `READY - index/host only` | file đã gửi hết, chỉ còn danh sách cuối |
| `INDEX+HOST SENT` | **đã xong đúng cách** |
| `INDEX+HOST SENT (4 rebuilt)` | đã xong, và tìm lại được 4 file bị bỏ sót |

### Cần chú ý

| Verdict | Ý nghĩa | Nên làm gì |
|---|---|---|
| `SENT-SHORT (2 missing)` | đã hoàn tất nhưng 2 file không được gửi | xem bên dưới |
| `SENT-FORCED-SHORT (3 missing)` | bạn đã chọn hoàn tất dù biết thiếu 3 file | bình thường nếu bạn tick Force |

**Panel bị "short" nghĩa là khách hàng nhận được danh sách file không đầy đủ.** Thường
do ảnh không còn trên máy. Nên tìm hiểu nguyên nhân trước khi chấp nhận.

### Không thể hoàn tất

| Verdict | Ý nghĩa |
|---|---|
| `INCOMPLETE - 3 source file(s) missing` | 3 file ảnh đã mất khỏi máy |
| `INCOMPLETE - 4 queue file(s) missing` | 4 file lệnh upload đã mất |
| `INCOMPLETE - 1 source file(s) missing, 2 queue file(s) missing` | thiếu cả hai loại |

Có một khác biệt quan trọng:

- **Thiếu file ảnh** — bản thân file đã mất. Không cách nào lấy lại được. Cần tìm hiểu
  vì sao nó bị xóa.
- **Thiếu file lệnh** — file ảnh có thể vẫn còn trên máy, chỉ là bị bỏ sót.
  **Tick "Reconstruct from disk" rồi quét lại** — cách này thường khắc phục được.

---

## Bốn tùy chọn

### Reconstruct from disk — thường để BẬT

Tìm những file ảnh còn nằm trên máy nhưng bị bỏ sót, xác định nơi chúng thuộc về, và
gửi đi.

Đây chính là thứ cứu được hầu hết panel bị kẹt. Không có nó thì chúng không bao giờ
hoàn tất được.

Công cụ rất thận trọng khi chọn file để gửi. Một file chỉ được lấy nếu tên của nó
nằm trong danh sách mà công cụ chấp nhận — xem mục **Kiểm soát file nào được gửi**
bên dưới. Bất cứ thứ gì lạ — file tạm, file backup, file có con số bất thường trong
tên — đều bị bỏ qua và ghi vào log. Công cụ không bao giờ tự bịa ra file để gửi.

### Force incomplete — để TẮT trừ khi bạn thực sự muốn

Hoàn tất một panel ngay cả khi còn thiếu file, tức là gửi cho khách hàng một danh sách
không đầy đủ.

Chỉ dùng khi bạn đã xem kỹ panel đó, hiểu rõ đang thiếu gì, và quyết định rằng hoàn
tất vẫn tốt hơn là để kẹt.

### Skip missing source — thường để TẮT

Thay đổi cách xử lý khi một file ảnh đã mất khỏi máy.

| | Kết quả |
|---|---|
| **Tắt** (bình thường) | panel hoàn tất mà không có file đó — khách nhận danh sách thiếu |
| **Bật** | panel vẫn kẹt, không ghi gì cả |

Bật lên khi bạn muốn điều tra các file ảnh bị mất trước khi cho panel hoàn tất.

### Retries — để mặc định là 3

Số lần thử lại một file trước khi bỏ cuộc. Mỗi lần thử sẽ tạo kết nối mới, nên trục
trặc mạng ngắn thường tự khắc phục được.

### Dùng chung cả ba

Có thể tick cả ba cùng lúc, nhưng thực tế chỉ có hai cách cài đặt đáng dùng. Đây là
kết quả thực đo trên một thư mục có 490 panel bị kẹt:

| Reconstruct | Skip missing | Force | Panel hoàn tất | trong đó danh sách thiếu | Còn kẹt |
|:---:|:---:|:---:|---:|---:|---:|
| – | – | – | 435 | 55 | 55 |
| **bật** | – | – | **490** | 58 | **0** |
| – | bật | – | 380 | **0** | 110 |
| – | – | bật | 490 | **110** | 0 |
| **bật** | **bật** | – | 432 | **0** | 58 |
| bật | – | bật | 490 | 58 | 0 |
| – | bật | bật | 490 | **110** | 0 |
| bật | bật | bật | 490 | 58 | 0 |

**Hai lựa chọn hợp lý:**

| Cài đặt | Dùng khi |
|---|---|
| **Chỉ Reconstruct** | bạn muốn mọi panel đều hoàn tất. 490 panel xong; 58 gửi danh sách thiếu vì ảnh đã thực sự mất. |
| **Reconstruct + Skip missing** | tuyệt đối không được gửi danh sách thiếu. 432 panel xong sạch sẽ; 58 panel được giữ lại để bạn kiểm tra. |

**Hai tổ hợp nên tránh:**

- **Bật Force khi Reconstruct đang bật** trong lần đo này không thay đổi gì, vì
  Reconstruct đã gỡ kẹt hết mọi panel. Nhưng nó không vô dụng nói chung — một file
  mất **cả** lệnh upload *lẫn* ảnh thì không thể khôi phục, và chỉ có Force mới hoàn
  tất được panel đó. Log sẽ cho biết bạn đang ở trường hợp nào: nếu Force được bật
  mà không cần dùng đến, phần tổng kết sẽ ghi rõ.
- **Skip + Force** cho kết quả y hệt chỉ bật Force — Force triệt tiêu hoàn toàn
  Skip, nên bạn nhận kết quả tệ nhất là 110 danh sách thiếu, đồng thời những panel
  đó vẫn hiện lại ở lần quét sau vì Skip đã giữ nguyên file lệnh của chúng.

**Chỉ bật Force là cài đặt tệ nhất** — 110 danh sách thiếu, gấp đôi mức cần thiết,
bởi vì những file đó vẫn còn nằm trên máy và Reconstruct đã có thể tìm ra chúng.

Chương trình sẽ cảnh báo nếu bạn chọn một trong hai tổ hợp vô nghĩa này.

---

## Kiểm soát file nào được gửi

**Phần này chỉ ảnh hưởng đến "Reconstruct from disk".** File nào đã có lệnh upload
riêng thì luôn được gửi — TrueTest đã quyết định nó thuộc về panel đó rồi. Các quy
tắc dưới đây chỉ áp dụng cho file nằm trên máy mà không có lệnh, tức là trường hợp
công cụ phải tự suy đoán.

Hai file text nằm cạnh `FTPRecoveryGUI.exe` và đi kèm theo nó:

| File | Tác dụng |
|---|---|
| `allowed_filenames.txt` | những tên được phép gửi |
| `denied_filenames.txt` | những tên tuyệt đối không được gửi — ưu tiên cao nhất |

Mở bằng Notepad. Mỗi dòng một tên file. Dòng bắt đầu bằng `#` là ghi chú, bị bỏ qua.

```
step01_0650NIT_B056_imgY_Crop.tif
step99_0650NIT_UDIRVibMap_imgY_Crop.tif
```

Có thể dùng `*` với nghĩa "bất kỳ ký tự nào":

```
*_gamma.hex               khớp d994_gamma.hex, d995_gamma.hex, ...
NyPucData_@PID@_*.hex     khớp _1st.hex, _2nd.hex, _3rd.hex, ...
```

`@PID@` đại diện cho phần tên thay đổi theo từng panel.

### Tự học

Mặc định, công cụ còn **tự học** tên file từ các lệnh upload thật mà nó tìm thấy, và
ghi nhớ vào `known_filenames.txt`. File đó do công cụ ghi ra — **sửa nó không có tác
dụng gì**, vì mỗi lần quét lại là nó được tạo lại.

Tự học thường là điều bạn muốn: các lệnh upload do TrueTest tạo ra, nên tên trong đó
mặc nhiên là đúng; và việc ghi nhớ giúp một tên vẫn dùng được sau khi mọi panel chứa
nó đã hoàn tất.

Nếu bạn muốn **chỉ** chấp nhận các tên trong `allowed_filenames.txt` và không học gì
thêm, thêm dòng này vào file đó:

```
!strict
```

Dùng khi bạn đã có danh sách ảnh chính thức và không muốn gửi bất cứ thứ gì ngoài nó.

### Kiểm tra trước khi tin tưởng

Quét một lần với Reconstruct được tick, rồi tìm trong log các dòng
`not a known filename`. Mỗi dòng là một file công cụ đã từ chối gửi. Nếu không có
dòng nào, tức là quy tắc của bạn bao phủ hết. Nếu thấy một file hợp lệ bị từ chối,
hãy thêm tên nó vào `allowed_filenames.txt`.

---

## Các tình huống thường gặp

**"Rất nhiều dòng hồng ghi queue file(s) missing"**
Tick **Reconstruct from disk** rồi nhấn **Start Scan** lại. Phần lớn sẽ chuyển sang
màu trắng.

**"Vẫn hồng sau khi đã reconstruct"**
Những file ảnh đó thực sự đã mất khỏi máy. Xem log để biết là file nào, rồi tìm hiểu
vì sao chúng bị xóa. Chỉ dùng Force sau khi đã chấp nhận việc gửi danh sách thiếu.

**"Chương trình tự dừng"**
Server của khách hàng không phản hồi, nên nó dừng lại thay vì đánh dấu hàng trăm file
là thất bại một cách vô ích. Sửa kết nối rồi chạy lại — nó sẽ tiếp tục từ chỗ đang dở.

**"Nhấn Upload mà không có gì xảy ra"**
Kiểm tra cột Verdict của dòng đó. Dòng hồng không thể hoàn tất nên không có gì được
gửi.

**"Cửa sổ có vẻ bị treo"**
Nhìn log bên phải và bộ đếm ở dưới cùng. Nếu chúng đang chạy thì chương trình vẫn hoạt
động. Một file đang thất bại mất khoảng một phút mới chuyển tiếp, nên dừng một lúc là
bình thường.

**"Một file có trên máy nhưng không được gửi"**
Tìm trong log dòng `not a known filename`. Nếu có, thêm tên file đó vào
`allowed_filenames.txt` — xem mục **Kiểm soát file nào được gửi**.

**"Tôi lỡ tay đóng chương trình"**
Không sao cả. Mở lại, nhấn Scan, rồi làm tiếp. Không mất gì.

**"Tôi lỡ chạy hai lần"**
Không sao cả. Chương trình biết file nào đã gửi rồi và sẽ không gửi lại lần nữa.

---

## Bản ghi được lưu ở đâu

Mọi thứ chương trình ghi ra đều nằm trong thư mục **Log\Recovery** cạnh
`FTPRecoveryGUI.exe` — không phải trong thư mục upload. Thư mục upload chỉ là đầu vào.

| File | Dùng để làm gì |
|---|---|
| `..._recovery_report.csv` | **mỗi panel một dòng — mở file này trước**, bằng Excel |
| `..._recovery.log` | chi tiết đầy đủ mọi thứ đã xảy ra |
| `..._winscp.log` | log kết nối thô, dùng khi việc truyền file có vấn đề |

Khung bên phải cửa sổ hiển thị cùng nội dung đó theo thời gian thực, nhưng sẽ tự cắt
bớt khi chạy lâu. **File log mới là bản ghi đầy đủ.**

Mở file CSV bằng Excel và sắp xếp theo cột cuối cùng để thấy tổng quan lần chạy, và
biết panel nào bị thiếu file.

### Các file đi kèm chương trình

Nằm cạnh `FTPRecoveryGUI.exe`:

| File | |
|---|---|
| `FTPRecoveryGUI.exe` | chương trình |
| `WinSCPnet.dll` | bắt buộc, đừng xóa |
| `allowed_filenames.txt` | **bạn sửa** — tên được phép gửi |
| `denied_filenames.txt` | **bạn sửa** — tên không bao giờ được gửi |
| `known_filenames.txt` | chương trình tự ghi, sửa không có tác dụng |
| `Log\Recovery\` | toàn bộ log và báo cáo |

Copy các file này sang máy nào cũng chạy được. Thư mục upload chỉ là đầu vào —
không cần đặt gì vào đó, và chương trình cũng không ghi gì vào đó.

---

## Nếu mất kết nối mạng

Chương trình được thiết kế để chạy không cần người trông, nên nó **không** dừng lại
khi server mất kết nối. Nó vẫn chạy tiếp và bảo vệ dữ liệu của bạn:

- **Không đánh dấu file nào là thất bại.** File không gửi được vì server sập sẽ được
  giữ nguyên, kèm file lệnh upload của nó.
- **Không gửi danh sách** cho những panel đó. Chúng được báo là `SERVER-OFFLINE`. Gửi
  danh sách lúc mất kết nối sẽ báo cho khách rằng những file đó không bao giờ đến,
  trong khi thực tế chúng còn chưa được thử gửi.
- **Không đứng chờ hàng giờ.** Khi server ngừng phản hồi, các file còn lại được bỏ
  qua chỉ trong tích tắc mỗi file, nên lần chạy kết thúc nhanh.
- **Tự phục hồi.** Cứ mỗi 30 giây nó kiểm tra lại server. Nếu server hoạt động trở
  lại giữa chừng, bạn sẽ thấy `server is reachable again - resuming normally` và nó
  chạy tiếp với tốc độ bình thường.

Trong log sẽ thấy như sau:

```
[conn] server unreachable, queue file kept: step01_0650NIT_B192_imgY_Crop.tif
```

và ở cuối:

```
Left for a later run : 4182  (server unreachable - queue files kept, nothing marked failed)
```

**Chỉ cần chạy lại khi có kết nối.** Nó sẽ tiếp tục từ chỗ đang dở, không mất gì và
không gửi trùng.

Có một khác biệt quan trọng giữa hai thứ trông có vẻ giống nhau:

| | Upload thất bại | Server không kết nối được |
|---|---|---|
| Chuyện gì xảy ra | kết nối vẫn tốt, nhưng **file này** không truyền được | không kết nối được tới server |
| Nguyên nhân thường gặp | không có quyền ghi, đĩa server đầy, file đang bị khóa | server tắt, mạng hỏng, sai mật khẩu |
| File lệnh | bị xóa | **được giữ lại** |
| Kết quả | panel hoàn tất, danh sách thiếu file đó | panel giữ nguyên, không gửi gì |

Nếu một lần truyền file mất nhiều thời gian, log sẽ báo thay vì im lặng:

```
... waiting 5s on step01_0650NIT_B048_imgY_Crop.tif  (attempt 1/3)
attempt 1/3 failed after 14s: <lý do>
```

---

## Một số thuật ngữ

| Từ | Ý nghĩa |
|---|---|
| **Panel** | một tấm màn hình và toàn bộ file thuộc về nó |
| **PID** | mã của panel theo hệ thống của khách hàng |
| **Queue file** | file lệnh nhỏ: "gửi ảnh này đến chỗ này" |
| **Host / index** | danh sách cuối cùng gửi đi, báo cho khách biết những gì đã nhận |
| **Short** | đã hoàn tất nhưng danh sách không đầy đủ |
| **Reconstruct** | dựng lại file lệnh bị mất, dựa vào file có sẵn trên đĩa |

---

## Nếu gặp vấn đề

Gửi cho người phụ trách công cụ này:

1. File `..._recovery_report.csv` của lần chạy đó
2. File `..._recovery.log` của lần chạy đó
3. Các tùy chọn nào đã được tick

Bấy nhiêu là đủ để biết chính xác chuyện gì đã xảy ra.

---

*Bản tiếng Anh: `README.md`. Về cài đặt, kiểm thử và cách hoạt động bên trong, xem
`DEVELOPER.md`.*
