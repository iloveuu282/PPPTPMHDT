# Từ thao tác trên web đến database

Tài liệu này giải thích các thao tác chính đi qua lớp nào và đọc, ghi bảng nào. Đường dẫn mã nguồn tính từ `src/TroLyQuyChe.Web/`.

## 1. Người dùng bấm Hỏi quy chế

Ví dụ: "Nghỉ 3 ngày làm việc thì báo trước bao lâu?"

1. `Pages/Index.cshtml.cs`, hàm `OnPostAsync`, kiểm tra câu hỏi và ngày người dùng nhập.
2. `QuestionAnswerService.AskAsync` gọi `ApplicableDateResolver` để lấy thời điểm tra.
3. `QuestionRecognizer` nhận ra ý hỏi về báo trước; `LeaveDurationParser` đọc được thời lượng 3 ngày.
4. `PolicyLookupService` gọi `RegulationRepository` để đọc quy chế, phiên bản và điều khoản phù hợp.
5. Chương trình lấy nguyên văn điều khoản SQL làm câu trả lời, rồi lưu lượt hỏi và nguồn trong cùng giao dịch.
6. Trang chuyển sang địa chỉ kết quả để hiển thị câu trả lời và liên kết Xem nguồn.

| Bảng | Thao tác |
|---|---|
| `Documents`, `DocumentVersions`, `Clauses` | Đọc để tìm nguồn |
| `QaEntries` | Thêm câu hỏi, câu trả lời, thời điểm và trạng thái |
| `Citations` | Thêm nguồn nếu câu trả lời có căn cứ |

Với bộ quy chế mẫu, ví dụ này trả mốc ít nhất 5 ngày làm việc. Con số được lấy từ điều khoản, không nằm trong mã nhận diện.
Câu cần làm rõ hoặc thiếu căn cứ vẫn lưu một lượt hỏi, nhưng không có trích dẫn. Câu trống hoặc quá 2000 ký tự bị chặn trước khi lưu. Lỗi SQL được báo là lỗi kỹ thuật.

## 2. Mở lịch sử và nguồn

| Thao tác | Mã xử lý | Dữ liệu đọc |
|---|---|---|
| Chọn một câu trong lịch sử | `IndexModel.OnGetAsync` → `HistoryService` → `FindEntryAsync` | `QaEntries`, `Citations` và nguồn liên quan |
| Bấm Xem nguồn | `SourceModel.OnGetAsync` → `CitationSourceService` → `FindCitationAsync` | `Citations` → `Clauses` → `DocumentVersions` → `Documents` |

Lịch sử gắn với mã nhận diện được lưu trong cookie của trình duyệt. Trình duyệt khác không có quyền mở lại lượt hỏi riêng đó. Quản trị viên có quyền xem nguồn theo chức năng được cấp.
Nguồn của lịch sử vẫn mở được nếu phiên bản đã bị thay thế hoặc thu hồi. Yêu cầu không có quyền hoặc không tồn tại trả HTTP 404.

## 3. Đọc bộ quy chế và đăng nhập

Trang `Pages/Policies.cshtml.cs` đọc các phiên bản đã duyệt, kiểm tra thời gian hiệu lực và bản thay thế rồi hiển thị bộ đang áp dụng. Thao tác này không ghi database.

Trang `Pages/Account/Login.cshtml.cs` gọi `AdminQueries.UserAsync` để đọc tài khoản còn hoạt động trong `AppUsers`. C# dùng `PasswordHasher` kiểm tra mật khẩu với giá trị băm. Đúng mật khẩu thì cấp cookie có vai trò quản trị. Không lưu mật khẩu dạng rõ vào SQL.

## 4. Quản trị quy chế

Các thao tác ghi bên dưới đi qua `VersionAdministrationService`. Dịch vụ kiểm tra quyền quản trị phía máy chủ.

| Thao tác | Trang và hàm | Thay đổi trong SQL |
|---|---|---|
| Thêm văn bản | `Admin/Index:OnPostAsync` → `CreateDocumentAsync` | Thêm `Documents` |
| Tạo bản nháp | `Admin/Create:OnPostAsync` → `CreateDraftAsync` | Thêm `DocumentVersions`, sao chép `Clauses` nếu được chọn và ghi `VersionActions` |
| Lưu nháp | `Admin/Edit:OnPostSaveAsync` → `SaveDraftAsync` | Cập nhật bản nháp, thay danh sách điều khoản của bản nháp và ghi nhật ký |
| Duyệt hoặc thay thế | `Admin/Edit:OnPostApproveAsync` → `ApproveAsync` | Cập nhật trạng thái, ngày duyệt, liên kết bản trước và ghi nhật ký |
| Thu hồi | `Admin/Edit:OnPostRevokeAsync` → `RevokeAsync` | Chuyển trạng thái sang thu hồi và ghi lý do vào nhật ký |

Chương trình chặn bản nháp rỗng, khoảng báo trước bị giao nhau hoặc bỏ sót và hiệu lực chồng nhau khi không chọn thay thế. Nếu bản mới bỏ một chủ đề đã có, quản trị viên phải xác nhận rõ. Dữ liệu đã duyệt không sửa trực tiếp.
`RowVersion` phát hiện biểu mẫu đang dùng dữ liệu cũ. Giao dịch và khóa theo văn bản giúp hai thao tác duyệt đồng thời không tạo ra hai bản cùng áp dụng.

Trang dòng thời gian và nhật ký chỉ đọc `DocumentVersions`, `VersionActions`, `AppUsers` để hiển thị trạng thái, người thao tác và lý do.

## 5. C# và SQL Server làm gì

**C#** hiểu ý hỏi theo mẫu, xác định thời điểm, áp dụng quy tắc chọn nguồn và điều phối các bước xử lý.

**SQL Server** lưu dữ liệu, thực hiện truy vấn và bảo vệ dữ liệu bằng khóa ngoại, điều kiện kiểm tra, ràng buộc duy nhất, `RowVersion`, giao dịch và trigger. SQL Server không tự hiểu câu hỏi tiếng Việt.

Để đối chiếu, mở [DoiChieu_BanChot.sql](DoiChieu_BanChot.sql) trong SSMS. Chọn đúng database, tìm Id của lượt hỏi trên web rồi đối chiếu `QaEntries`, `Citations` và điều khoản liên quan. Script này chỉ đọc dữ liệu.
