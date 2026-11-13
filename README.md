# Trợ lý hỏi đáp quy chế có trích dẫn

Đồ án môn Phương pháp phát triển phần mềm hướng đối tượng, lớp OOSD_CNPM1.

Mã nguồn: [iloveuu282/PPPTPMHDT](https://github.com/iloveuu282/PPPTPMHDT).

Sinh viên: Mai Việt Hoàng — 1250080058.
Giảng viên: Phạm Trọng Huynh.
Công nghệ: C#, .NET 10, Razor Pages, Entity Framework Core và SQL Server

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

67 câu chuẩn và 44 câu hồi quy, kèm kiểm tra dữ liệu, quản trị và giao diện. Các kết quả ghi rõ ngày chạy; không phải cam kết hiểu đúng mọi câu tiếng Việt.

## Publish

```powershell
dotnet publish src/TroLyQuyChe.Web -c Release -o output/publish
dotnet output/publish/TroLyQuyChe.Web.dll --contentRoot "$PWD/output/publish" --urls http://localhost:5080
```

Máy chạy bản publish cần ASP.NET Core Runtime 10 và database đã chuẩn bị. Build, publish, backup và cookie nằm ngoài danh sách đưa lên Git.

