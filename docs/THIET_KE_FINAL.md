# Thiết kế hệ thống

Ứng dụng được chia theo công việc của từng phần. Trang Razor nhận câu hỏi và hiển thị kết quả. Các lớp C# xử lý câu hỏi, chọn quy chế và gọi lớp truy cập dữ liệu. SQL Server lưu quy chế, tài khoản, lịch sử và nguồn đã trích.

## Các phần chính

| Phần | Nhiệm vụ | Lớp hoặc thư mục |
|---|---|---|
| Giao diện | Nhận dữ liệu, kiểm tra biểu mẫu, hiển thị kết quả | `Pages/` |
| Nhận diện | Tìm ý hỏi, từ đồng nghĩa và số ngày nghỉ | `Services/Understanding/` |
| Nghiệp vụ | Xác định thời điểm, chọn nguồn, quản lý phiên bản | `Services/` |
| Dữ liệu | Đọc và ghi SQL Server thông qua EF Core | `Data/` |

Ví dụ, khi người dùng hỏi "Nghỉ 3 ngày thì báo trước bao lâu?", trang hỏi đáp không chứa sẵn câu trả lời. Chương trình nhận ra ý hỏi về báo trước, đọc điều khoản phù hợp trong SQL rồi lưu câu trả lời cùng nguồn.

## Các lớp xử lý chính

| Lớp | Công việc |
|---|---|
| `QuestionAnswerService` | Gọi các bước xử lý và lưu một lượt hỏi |
| `ApplicableDateResolver` | Xác định ngày hoặc khoảng thời gian cần tra |
| `QuestionRecognizer` | Nhận diện người dùng đang hỏi hạn mức, báo trước, giờ làm hay nội dung khác |
| `LeaveDurationParser` | Đọc số ngày nghỉ, ví dụ 3 ngày hoặc từ 2 ngày trở lên |
| `PolicyCatalog` | Nối ý hỏi với mã văn bản và chủ đề điều khoản |
| `PolicyLookupService` | Chọn phiên bản và điều khoản đúng thời điểm |
| `ClauseValidation` | Kiểm tra khoảng số ngày nghỉ và chủ đề của bản nháp |
| `RegulationRepository` | Đọc quy chế, lưu lịch sử và lấy nguồn đã trích |
| `VersionAdministrationService` | Tạo nháp, lưu, duyệt, thay thế và thu hồi |
| `VersionPresentation` | Hiển thị trạng thái và thời gian áp dụng |

Chi tiết từng thao tác nằm trong [luồng code và SQL](demo/Luong_NguoiDung_Code_SQL.md).

## Dữ liệu lưu ở đâu

| Bảng | Dữ liệu |
|---|---|
| `Documents` | Mã và tên quy chế |
| `DocumentVersions` | Phiên bản, trạng thái, ngày hiệu lực và bản được thay thế |
| `Clauses` | Số điều, chủ đề, nội dung và khoảng số ngày nghỉ nếu có |
| `QaEntries` | Câu hỏi, câu trả lời, thời điểm tra và thông tin xử lý |
| `Citations` | Liên kết lượt hỏi với điều khoản đã dùng |
| `AppUsers` | Tài khoản quản trị và mật khẩu đã băm |
| `VersionActions` | Nhật ký thao tác trên phiên bản |

`__EFMigrationsHistory` là bảng EF Core dùng để theo dõi migration, không phải bảng nghiệp vụ.

## Chọn phiên bản

Một bản được dùng khi đã duyệt, chưa thu hồi và có hiệu lực tại ngày tra. Ngày kết thúc áp dụng là mốc sớm hơn giữa ngày hết hạn đã lưu và ngày trước khi bản thay thế bắt đầu.

Nếu bản mới thay thế ngay trong cùng ngày, câu hỏi mới dùng bản sau. Nguồn trước vẫn giữ để mở lại lịch sử. Nếu thu hồi bản sau, chương trình không tự cho bản trước có hiệu lực trở lại. Hiện chưa tra phiên bản theo giờ và phút.

## Giữ dữ liệu nhất quán

- Khi lưu câu trả lời, `QaEntries` và `Citations` được ghi trong cùng giao dịch. Nếu một bước lỗi thì giao dịch được hủy.
- Khi quản trị sửa hoặc duyệt, chương trình kiểm tra `RowVersion` để phát hiện dữ liệu đã bị người khác thay đổi.
- Các thao tác quản trị dùng giao dịch `Serializable` và khóa dòng văn bản bằng `UPDLOCK, HOLDLOCK` để tránh hai lần duyệt gây xung đột.
- Khóa ngoại, ràng buộc duy nhất và điều kiện kiểm tra trong SQL giúp ngăn dữ liệu không hợp lệ. Trigger bảo vệ nội dung nguồn đã duyệt.
- Lịch sử đọc câu trả lời và điều khoản đã lưu. Nó không chạy lại câu hỏi bằng phiên bản hiện tại.

Các migration được giữ để cài database từ đầu. Lệnh khởi động web thông thường không chạy migration hoặc tạo lại dữ liệu.

## Giới hạn

Hệ thống hiểu câu hỏi theo các mẫu đã viết. Nhập một chủ đề hoàn toàn mới vào trang quản trị chưa làm chương trình tự hiểu chủ đề đó. Quản trị viên vẫn phải kiểm tra ý nghĩa câu chữ, ví dụ giờ vào làm, giờ tan làm và tổng thời lượng có thống nhất hay không.
