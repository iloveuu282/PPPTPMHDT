# Kiểm thử phần mềm

Thư mục này có bộ câu hỏi, kết quả đã chạy và các tệp minh chứng. Khi sửa code hoặc quy chế cần chạy lại phần kiểm thử liên quan; các kết quả cũ không tự cập nhật.

## Cách chạy bộ C#

Build Release tại thư mục gốc dự án trước. Sau đó tạo database thử mới và chạy kiểm thử:

```powershell
$credential = Get-Credential -UserName admin -Message 'Đặt mật khẩu cho database thử'
$env:DemoAdmin__Password = $credential.GetNetworkCredential().Password
try {
    dotnet run --project tests/TroLyQuyChe.Checks -c Release --no-build -- --prepare-demo
} finally {
    Remove-Item Env:DemoAdmin__Password -ErrorAction SilentlyContinue
    Remove-Variable credential -ErrorAction SilentlyContinue
}
dotnet run --project tests/TroLyQuyChe.Checks -c Release --no-build
```

Lệnh tạo một database `TroLyQuyCheDemo_Simple_...` riêng. Bài kiểm thử có thay và thu hồi quy chế, nên mỗi lần chạy toàn bộ cần tạo database thử mới. Không dùng database này làm bản trình chiếu cuối hoặc đổi kết nối sang database chính.

## Kết quả C# đã lưu ngày 11/11/2026

| Nhóm | Kết quả | Tệp kết quả |
|---|---|---|
| Hỏi đáp chuẩn | 67/67 | [simple-results.json](simple-results.json) |
| Hỏi đáp hồi quy | 44/44 | [final-regression-results.json](final-regression-results.json) |
| Kiểm tra dữ liệu | 2/2 | DB01–DB02 trong simple-results.json |
| Quản trị chuẩn | 12/12 | AD01–AD12 trong simple-results.json |
| Quản trị bổ sung | 13/13 | [final-admin-results.json](final-admin-results.json) |

Các câu và nội dung cần đối chiếu nằm trong [simple-cases.json](simple-cases.json) và [final-regression-cases.json](final-regression-cases.json).

Tổng cộng có 111 ca hỏi đáp, gồm câu biến thể và câu tra thời điểm. Kết quả này không có nghĩa chương trình hiểu đúng mọi câu tiếng Việt.
Đồng hồ trong bộ C# cố định ngày 11/11/2026. Dữ liệu ban đầu là giờ 8–16, phiên bản 1.0; các bài quản trị tạo thêm thay đổi để kiểm tra vòng đời phiên bản.

## Kiểm thử giao diện

Phần này cần Node.js, Playwright và web trình chiếu đang chạy ở cổng 5081. Đây là công cụ kiểm thử, không phải điều kiện để dùng ứng dụng.

```powershell
npm install
npx playwright install chromium
$credential = Get-Credential -UserName admin -Message 'Mật khẩu quản trị của web đang kiểm tra'
$env:UI_ADMIN_PASSWORD = $credential.GetNetworkCredential().Password
try {
    npm run check:ui
} finally {
    Remove-Item Env:UI_ADMIN_PASSWORD -ErrorAction SilentlyContinue
    Remove-Variable credential -ErrorAction SilentlyContinue
}
```

`UI_BASE_URL` dùng để chọn địa chỉ web; mặc định là `http://localhost:5081`. `UI_BROWSER` cho phép chọn tệp thực thi của trình duyệt đã cài.
Chỉ đặt `UI_ADMIN_MUTATIONS=1` khi chấp nhận thử thay phiên bản trên database diễn tập riêng. Tệp cookie do bài thử tạo nằm trong `output/` và không đưa lên Git.

## Kết quả giao diện và lưu dữ liệu ngày 11/11/2026

| Nội dung | Kết quả | Tệp minh chứng |
|---|---|---|
| Giao diện bản trình chiếu | 33 kiểm tra đạt | [giao-dien-dien-tap.json](bang-chung/giao-dien-dien-tap.json) |
| Giao diện bản chính | 28 kiểm tra đạt | [giao-dien-ban-chinh.json](bang-chung/giao-dien-ban-chinh.json) |
| Bảo toàn các dòng cũ | 7 bảng giữ nguyên | [final-main-applied.json](bang-chung/final-main-applied.json) |
| Tắt và chạy lại | Số lượng bản ghi trước/sau bằng nhau | [final-main-restart.json](bang-chung/final-main-restart.json) |
| Mở lại lịch sử và nguồn | Đạt | [final-history-after-restart.json](bang-chung/final-history-after-restart.json) |

Bản trình chiếu dùng giờ 9–17, thời gian 4.0, nghỉ/công ty 1.0. Khác biệt với giờ 8–16 trong bộ C# là do dùng hai bộ dữ liệu có mục đích khác nhau.
Các bài thử giao diện kiểm tra tải CSS/JavaScript, nguồn, quyền xem lịch sử, chống giả mạo yêu cầu, mã HTML trong câu hỏi và nhiều độ rộng màn hình.

## Kiểm tra khởi động ngày 12/11/2026

Build Release đã ghi nhận 0 lỗi, 0 cảnh báo. Có [13 kiểm tra đạt](bang-chung/kiem-tra-web-20261112.json): khởi động, các trang và tệp giao diện, đọc ba quy chế từ SQL, quyền quản trị, các trang đã bỏ và chặn khởi tạo khi thiếu mật khẩu.

Lần này chỉ đọc dữ liệu và không chạy lại toàn bộ 111 ca. Không reset database hoặc đổi mật khẩu của tài khoản đang có.

## Giới hạn của kết quả

Các tệp trên ghi lại những lần chạy cụ thể. Chương trình vẫn nhận diện theo mẫu, chưa tự học chủ đề hoặc tính số dư phép. Cần phân biệt câu trả lời có nguồn với việc đã hiểu đúng ý hỏi; cả hai đều phải được kiểm tra.
