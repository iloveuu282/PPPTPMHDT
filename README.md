# Trợ lý hỏi đáp quy chế có trích dẫn

Đồ án môn Phương pháp phát triển phần mềm hướng đối tượng, lớp OOSD_CNPM1.

Mã nguồn: [iloveuu282/PPPTPMHDT](https://github.com/iloveuu282/PPPTPMHDT).

- Sinh viên: Mai Việt Hoàng — 1250080058.
- Giảng viên: Phạm Trọng Huynh.
- Công nghệ: C#, .NET 10, Razor Pages, Entity Framework Core và SQL Server Express.

Ứng dụng giúp tra cứu quy chế về thời gian làm việc, đăng ký nghỉ và làm việc tại công ty. Mỗi câu trả lời có căn cứ sẽ kèm điều khoản, phiên bản và liên kết xem nguồn. Dữ liệu quy chế là dữ liệu giả lập dùng cho đồ án.

## Chức năng

- Đặt câu hỏi bằng tiếng Việt. Nếu không nhập thời điểm, ứng dụng dùng ngày hiện tại theo giờ Việt Nam.
- Tra quy chế theo ngày hoặc tháng/năm.
- Hỏi lại khi câu hỏi chưa rõ; báo thiếu căn cứ khi chưa có quy định phù hợp.
- Lưu lịch sử và mở lại đúng nguồn đã trích.
- Quản trị viên tạo bản nháp, lưu nội dung, duyệt, thay thế và thu hồi phiên bản.

Phần nhận diện dùng từ đồng nghĩa và mẫu câu viết bằng C#. Chủ đề mới cần bổ sung quy tắc. Ứng dụng chưa xử lý đơn nghỉ hoặc tính số ngày phép còn lại của từng người.

## Cài đặt và chạy

Cần .NET SDK 10 và SQL Server Express. Mở PowerShell tại thư mục chứa `TroLyQuyChe.slnx`:

```powershell
dotnet restore
dotnet build -c Release --no-restore
```

Sửa kết nối trong `src/TroLyQuyChe.Web/appsettings.json`, mục `ConnectionStrings:Regulations`, nếu instance trên máy khác với cấu hình mẫu. Cấu hình hiện tại là `MVKING\SQLEXPRESS`, database `TroLyQuyCheDb`, dùng xác thực Windows.

Nếu database đã có dữ liệu:

```powershell
.\scripts\run_main.ps1
```

Mở **http://localhost:5080**. Tên đăng nhập quản trị là `admin`, mật khẩu do người cài đặt thiết lập. Mỗi lần chạy web không tự tạo lại dữ liệu hoặc đổi mật khẩu.

### Máy mới chưa có database

Nhập mật khẩu quản trị trong hộp thoại rồi chạy lệnh khởi tạo:

```powershell
$credential = Get-Credential -UserName admin -Message 'Đặt mật khẩu quản trị'
$env:DemoAdmin__Password = $credential.GetNetworkCredential().Password
try {
    dotnet run --project src/TroLyQuyChe.Web -c Release --no-build -- --init-db
} finally {
    Remove-Item Env:DemoAdmin__Password -ErrorAction SilentlyContinue
    Remove-Variable credential -ErrorAction SilentlyContinue
}
.\scripts\run_main.ps1
```

Lệnh `--init-db` dành cho database trống. Nó tạo cấu trúc bảng và bộ dữ liệu ban đầu có ba quy chế, 20 điều khoản, giờ làm 8–16, phiên bản 1.0. Không dùng lệnh này để làm mới database đang sử dụng. Các migration cũ có một lần thay bộ quy chế và xóa dữ liệu cũ; chương trình có bước chặn nếu phát hiện lịch sử cần bảo vệ.

Khi cần chủ động đặt lại mật khẩu, nhập mật khẩu như trên và thay `--init-db` bằng `--set-demo-admin`. Không lưu mật khẩu thật vào repository.

### Tạo database riêng để trình chiếu

Bộ trình chiếu dùng giờ làm 9–17, phiên bản thời gian 4.0. Nghỉ và làm việc tại công ty dùng bản 1.0. Để tạo một database trình chiếu mới:

```powershell
$credential = Get-Credential -UserName admin -Message 'Đặt mật khẩu cho bản trình chiếu'
$env:DemoAdmin__Password = $credential.GetNetworkCredential().Password
try {
    dotnet run --project tests/TroLyQuyChe.Checks -c Release --no-build -- --prepare-final-demo
} finally {
    Remove-Item Env:DemoAdmin__Password -ErrorAction SilentlyContinue
    Remove-Variable credential -ErrorAction SilentlyContinue
}
.\scripts\run_demo.ps1
```

Mở **http://localhost:5081**. Lệnh tạo database mới có tên `TroLyQuyCheDemo_Final_...`, không thay dữ liệu trong `TroLyQuyCheDb`. Thông tin kết nối được lưu riêng ở `output/final-demo.json`. Những lần sau chỉ cần chạy `run_demo.ps1` để dùng lại database này.

## Câu hỏi để thử

Kết quả dưới đây dùng bộ trình chiếu 9–17:

| Câu hỏi | Kết quả mong đợi |
|---|---|
| Giờ bắt đầu vào làm? | 9 giờ sáng, nguồn thời gian 4.0 |
| Giờ tan làm? | 17 giờ, nguồn thời gian 4.0 |
| Giờ vào làm tháng 5/2026? | 8 giờ sáng, nguồn lịch sử 1.0 |
| Xin nghỉ báo trước bao lâu? | Hai trường hợp: nghỉ 1 ngày báo trước ít nhất 1 ngày; từ 2 ngày báo trước ít nhất 5 ngày làm việc |
| Nghỉ 3 ngày làm việc thì báo trước bao lâu? | Ít nhất 5 ngày làm việc |
| Nghỉ bao lâu? | Hỏi lại để biết người dùng muốn tra nội dung gì |
| Được nghỉ tối đa bao nhiêu ngày mỗi năm? | Báo thiếu căn cứ về hạn mức năm |

## Kiểm thử

Xem [cách chạy và kết quả kiểm thử](docs/kiem-thu/KetQua_KiemThu.md). Bộ đã lưu có 67 câu chuẩn và 44 câu hồi quy, kèm kiểm tra dữ liệu, quản trị và giao diện. Các kết quả ghi rõ ngày chạy; không phải cam kết hiểu đúng mọi câu tiếng Việt.

## Publish

```powershell
dotnet publish src/TroLyQuyChe.Web -c Release -o output/publish
dotnet output/publish/TroLyQuyChe.Web.dll --contentRoot "$PWD/output/publish" --urls http://localhost:5080
```

Máy chạy bản publish cần ASP.NET Core Runtime 10 và database đã chuẩn bị. Build, publish, backup và cookie nằm ngoài danh sách đưa lên Git.

## Các thư mục chính

| Thư mục | Nội dung |
|---|---|
| `src/TroLyQuyChe.Web/Pages/` | Giao diện Razor và xử lý nhập liệu |
| `src/TroLyQuyChe.Web/Services/Understanding/` | Nhận diện câu hỏi và số ngày nghỉ |
| `src/TroLyQuyChe.Web/Services/` | Xử lý nghiệp vụ hỏi đáp và quản trị |
| `src/TroLyQuyChe.Web/Data/` | Các lớp dữ liệu, kết nối EF Core và migration |
| `tests/` | Kiểm thử C# và tạo dữ liệu trình chiếu |
| `scripts/` | Lệnh chạy web và kiểm thử giao diện |
| `docs/` | Đặc tả, thiết kế, sơ đồ, ảnh và kết quả kiểm thử |

## Chỉnh giao diện và cách hiểu câu hỏi

Các đường dẫn dưới đây tính từ `src/TroLyQuyChe.Web/`:

| Cần sửa | Vị trí |
|---|---|
| Font, cỡ chữ | `wwwroot/css/site.css`: `--font`, `--text`, `--small`, `--tiny`, `--heading` |
| Màu | Cùng file CSS: `--accent`, `--accent-hover`, `--ink`, `--muted`, `--bg`, `--surface`, `--line` |
| Khoảng cách, độ rộng | `--space-1` đến `--space-10`, `--page-width`, `--sidebar-width`, `--panel-radius`, `--control-radius` |
| Ô hỏi và menu | `Pages/Index.cshtml`, `Pages/Shared/_Layout.cshtml` |
| Từ đồng nghĩa, mẫu câu | `Services/Understanding/QuestionVocabulary.cs`, `QuestionRecognizer.cs` |
| Chọn quy chế theo ý hỏi | `Services/PolicyCatalog.cs` |

Nội dung câu trả lời được sửa qua trang quản trị và lưu trong SQL Server.

## Tài liệu đi kèm

- [Đặc tả chức năng](docs/nghiep-vu/01_DacTa_NghiepVu.md).
- [Bộ quy chế trình chiếu](docs/nghiep-vu/06_BoQuyChe_ChotDemo.md).
- [Thiết kế hệ thống](docs/THIET_KE_FINAL.md).
- [Một thao tác đi qua code và database như thế nào](docs/demo/Luong_NguoiDung_Code_SQL.md).
- [SQL để kiểm tra trong SSMS](docs/demo/DoiChieu_BanChot.sql).
- [Sơ đồ draw.io](docs/uml).

Ảnh chạy thực tế ngày 11/11/2026: [bộ quy chế](docs/hinh-anh/01-quy-che.png), [câu trả lời có nguồn](docs/hinh-anh/02-tra-loi.png), [giao diện điện thoại](docs/hinh-anh/03-dien-thoai.png).
