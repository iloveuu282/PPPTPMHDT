# Bộ quy chế mẫu

Bộ này dùng cho bản trình chiếu và các ví dụ hỏi đáp. Quy chế là dữ liệu giả lập của Công ty Phần mềm Mai Việt Hoàng. Nội dung dưới đây đối chiếu với kho chính ngày 11/11/2026.

| Nhóm | Phiên bản | Số điều |
|---|---|---|
| Thời gian làm việc | 4.0 | 5 |
| Đăng ký nghỉ | 1.0 | 8 |
| Làm việc tại công ty | 1.0 | 7 |

Khi trả lời, ứng dụng đọc SQL Server. Tệp JSON cùng tên được công cụ tạo database trình chiếu sử dụng; sửa JSON không tự đổi dữ liệu đang có trong SQL.

## QC-THOIGIAN-LAMVIEC — Thời gian làm việc — phiên bản 4.0

- **Điều 1**: Bắt đầu làm việc lúc 9 giờ sáng.
- **Điều 2**: Kết thúc làm việc lúc 17 giờ (5 giờ chiều).
- **Điều 3**: Làm việc 5 ngày mỗi tuần, từ thứ Hai đến thứ Sáu. Thứ Bảy và Chủ nhật nghỉ.
- **Điều 4**: Nghỉ trưa từ 12 giờ đến 13 giờ.
- **Điều 5**: Làm việc 7 giờ mỗi ngày, không tính 1 giờ nghỉ trưa.

## QC-DANGKY-NGHI — Đăng ký nghỉ — phiên bản 1.0

- **Điều 1**: Được đăng ký nghỉ tối đa 3 ngày làm việc mỗi tuần.
- **Điều 2**: Được đăng ký nghỉ tối đa 8 ngày làm việc mỗi tháng.
- **Điều 3**: Giới hạn tuần và tháng áp dụng đồng thời. Muốn nghỉ vượt một trong hai giới hạn phải được trưởng bộ phận xét duyệt.
- **Điều 4**: Nghỉ 1 ngày làm việc phải gửi yêu cầu trước ít nhất 1 ngày làm việc.
- **Điều 5**: Nghỉ từ 2 ngày làm việc trở lên phải gửi yêu cầu trước ít nhất 5 ngày làm việc.
- **Điều 6**: Yêu cầu nghỉ trong giới hạn phải được quản lý trực tiếp duyệt trước khi nghỉ.
- **Điều 7**: Gửi yêu cầu nghỉ cho quản lý trực tiếp, ghi rõ ngày bắt đầu, ngày kết thúc, số ngày làm việc dự định nghỉ và lý do.
- **Điều 8**: Tuần tính từ thứ Hai đến Chủ nhật; tháng tính theo tháng dương lịch. Số ngày nghỉ và thời hạn báo trước tính theo ngày làm việc.

## QC-LAMVIEC-CONGTY — Làm việc tại công ty — phiên bản 1.0

- **Điều 1**: Đi muộn phải báo cho quản lý trực tiếp trước giờ bắt đầu làm việc.
- **Điều 2**: Muốn về sớm phải được quản lý trực tiếp đồng ý trước khi rời công ty.
- **Điều 3**: Ra ngoài trong giờ làm phải được quản lý trực tiếp đồng ý.
- **Điều 4**: Khách phải đăng ký với lễ tân và được người phụ trách đồng ý trước khi vào khu vực làm việc.
- **Điều 5**: Đăng ký phòng họp với bộ phận hành chính trước khi sử dụng; kết thúc đúng giờ đã đăng ký.
- **Điều 6**: Giữ trật tự tại khu vực làm việc, không làm ảnh hưởng đến người khác.
- **Điều 7**: Giữ vệ sinh chỗ làm, bỏ rác đúng nơi quy định.

## Dữ liệu ban đầu và dữ liệu trình chiếu

Database mới tạo bằng `--init-db` có bộ gốc 1.0, giờ 8–16. Bộ trình chiếu ở trên dùng thời gian 4.0, giờ 9–17. Vì vậy, khi hỏi tháng 5 hoặc tháng 8/2026, chương trình vẫn trả 8 giờ theo bản lịch sử 1.0.

Trên kho chính được đối chiếu, bản 4.0 bắt đầu ngày 11/11/2026. Khi tạo kho trình chiếu mới bằng `--prepare-final-demo`, bản 4.0 bắt đầu vào ngày tạo kho đó.

## Cách áp dụng

- Hạn mức tuần và tháng áp dụng đồng thời. Chưa có hạn mức năm hoặc số dư phép riêng của từng người.
- Câu hỏi báo trước không nêu số ngày nghỉ sẽ nhận cả hai trường hợp. Có số ngày rõ thì lấy trường hợp tương ứng.
- Không có ngày kết thúc thì quy chế tiếp tục áp dụng đến khi bị thay thế hoặc thu hồi.
- Nếu kỳ tra có đổi phiên bản, chương trình yêu cầu chọn ngày cụ thể. Nếu thay thế cùng ngày, câu hỏi mới dùng bản sau; nguồn lịch sử vẫn được giữ.
- Số điều đang áp dụng là 20. Tổng bản ghi trong SQL có thể nhiều hơn vì còn bản nháp, bản cũ và bản thu hồi.
