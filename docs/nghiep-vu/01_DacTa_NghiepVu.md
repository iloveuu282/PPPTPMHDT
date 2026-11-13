# Đặc tả chức năng

Ứng dụng có hai vai trò: người tra cứu quy chế và quản trị viên. Người tra cứu đặt câu hỏi, đọc lịch sử và xem nguồn. Quản trị viên quản lý văn bản cùng các phiên bản của văn bản.

Phạm vi gồm thời gian làm việc, đăng ký nghỉ và làm việc tại công ty. Xem nội dung mẫu tại [bộ quy chế](06_BoQuyChe_ChotDemo.md).

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|---|---|
| BR-01 | Chỉ lấy bản đã duyệt, chưa thu hồi và có hiệu lực trong ngày hoặc kỳ được hỏi. Không chọn chỉ vì số phiên bản lớn nhất. |
| BR-02 | Không nhập thời điểm thì dùng hôm nay theo giờ Việt Nam. Hỏi quá khứ mà thiếu mốc, ngày nhập mâu thuẫn hoặc kỳ tra có đổi phiên bản thì hỏi lại. |
| BR-03 | Phân biệt hạn mức được nghỉ, thời hạn báo trước và người duyệt. Hỏi báo trước chung thì trả các trường hợp có căn cứ; có số ngày nghỉ thì chọn trường hợp phù hợp. |
| BR-04 | Câu trả lời có căn cứ phải có đoạn trích, mã văn bản, điều khoản, phiên bản, thời điểm tra và Xem nguồn. Lịch sử giữ câu trả lời và nguồn đã dùng. |
| BR-05 | Chỉ quản trị viên được ghi quy chế. Chỉ sửa nội dung bản nháp. Bản thiếu chủ đề phải được xác nhận; khoảng báo trước bị giao nhau hoặc bỏ sót thì chặn duyệt. |
| BR-06 | Bản mới thay thế phải cùng văn bản, không hồi tố trước hôm nay và không bắt đầu trước bản cũ. Mỗi bản có tối đa một bản kế tiếp. Thu hồi bản sau không tự khôi phục bản trước. |
| BR-07 | Lưu câu hỏi hợp lệ và câu trả lời vào lịch sử; chỉ câu có căn cứ mới có trích dẫn. Câu trống hoặc quá 2000 ký tự không được lưu. Lỗi SQL là lỗi kỹ thuật. |
| BR-08 | Không tự suy phép năm hoặc số dư phép cá nhân từ hạn mức tuần/tháng. Chủ đề mới cần bổ sung mẫu nhận diện trong C#. |

Quy chế không có ngày hết hạn tiếp tục áp dụng cho đến khi bị thay thế hoặc thu hồi. Nếu thay thế cùng ngày, câu hỏi mới dùng bản sau; nguồn trước vẫn giữ cho lịch sử. Hệ thống chưa phân biệt phiên bản theo giờ và phút.

## UC-01 — Hỏi đáp quy chế

**Người thực hiện:** người tra cứu. Database cần có dữ liệu quy chế và kết nối được.

1. Người dùng nhập câu hỏi, có thể chọn thêm ngày.
2. Chương trình xác định thời điểm và ý hỏi. Nếu thiếu thông tin cần thiết thì yêu cầu làm rõ.
3. Chương trình tìm phiên bản hợp lệ, rồi lấy điều khoản đúng chủ đề.
4. Nếu có căn cứ, hiển thị nội dung điều khoản và nguồn. Nếu chưa có quy định phù hợp, nói rõ nội dung còn thiếu.
5. Lưu lượt hỏi và nguồn, sau đó hiển thị trang kết quả. Người dùng có thể mở lại từ lịch sử của trình duyệt.

Ví dụ, câu "Xin nghỉ báo trước bao lâu?" trả hai trường hợp của bộ mẫu. Câu "Nghỉ 3 ngày làm việc thì báo trước bao lâu?" chỉ trả trường hợp tương ứng. Câu "Nghỉ bao lâu?" cần hỏi lại vì chưa rõ đang hỏi hạn mức hay thời hạn báo trước.

Số ngày âm, lẻ, quá lớn hoặc nhiều thời lượng không rõ được yêu cầu làm rõ. Lịch sử của trình duyệt khác không được tự ý mở. Nếu database không truy cập được thì báo lỗi kỹ thuật, không coi là kho thiếu quy định.

## UC-02 — Quản trị phiên bản

**Người thực hiện:** quản trị viên đã đăng nhập.

1. Chọn văn bản rồi tạo bản nháp. Có thể sao chép nội dung bản đã có để chỉnh sửa.
2. Lưu tên phiên bản, ngày hiệu lực và các điều khoản.
3. Chọn cách áp dụng: dùng ngày duyệt theo giờ Việt Nam hoặc dùng ngày đã lưu. Nếu tiếp quản bản đang áp dụng thì chọn thay thế.
4. Kiểm tra và xác nhận nội dung trước khi duyệt. Chương trình kiểm tra quyền, trạng thái, dữ liệu có bị thay đổi, hiệu lực và khoảng báo trước.
5. Nếu hợp lệ, lưu trạng thái đã duyệt và nhật ký trong cùng giao dịch. Bản trước vẫn được giữ làm nguồn lịch sử.
6. Khi cần thu hồi, nhập lý do rồi xác nhận. Bản thu hồi không được dùng để trả lời câu hỏi mới.

Không duyệt bản nháp rỗng. Hai lần duyệt cùng lúc phải được kiểm tra để tránh chồng hiệu lực. Nếu cố ý bỏ một chủ đề trong bản mới, quản trị viên phải xác nhận riêng.
Chương trình kiểm tra được các điều kiện đã lập trình, nhưng quản trị viên vẫn phải đọc nội dung để bảo đảm câu chữ và các mốc giờ thống nhất.

## UC-03 — Xem nguồn

**Người thực hiện:** chủ sở hữu lượt hỏi hoặc quản trị viên có quyền xem nguồn.

1. Bấm Xem nguồn dưới câu trả lời.
2. Chương trình kiểm tra quyền và tìm điều khoản qua trích dẫn đã lưu.
3. Hiển thị nguyên văn điều khoản, mã văn bản, phiên bản, ngày duyệt và thời gian áp dụng.

Nguồn vẫn mở được khi bản đó đã bị thay thế hoặc thu hồi. Nguồn bị thay ngay trong cùng ngày được ghi rõ là nguồn lịch sử. Không tìm thấy hoặc không có quyền thì trả HTTP 404.

## Tình huống cần kiểm tra

Kết quả giờ làm dưới đây dùng bộ trình chiếu thời gian 4.0, không phải bộ gốc khi vừa khởi tạo database.

| Mã | Tình huống | Kết quả mong đợi |
|---|---|---|
| AT-01 | Hỏi giờ vào làm, không chọn ngày | 9 giờ sáng, bản 4.0 |
| AT-02 | Hỏi giờ vào làm tháng 5 hoặc tháng 8/2026 | 8 giờ sáng, bản 1.0 |
| AT-03 | Xin nghỉ báo trước bao lâu? | Hai trường hợp, hai nguồn |
| AT-04 | Nghỉ 3 ngày làm việc thì báo trước bao lâu? | Ít nhất 5 ngày làm việc, một nguồn |
| AT-05 | Được nghỉ tối đa mấy ngày mỗi năm? | Thiếu căn cứ về hạn mức năm |
| AT-06 | Nghỉ bao lâu? | Hỏi lại ngắn gọn |
| AT-07 | Duyệt các khoảng báo trước bị giao nhau | Chặn duyệt, không ghi nhật ký duyệt thành công |
| AT-08 | Thay phiên bản cùng ngày rồi mở lịch sử cũ | Câu mới dùng bản sau, nguồn cũ giữ nguyên |
| AT-09 | Thu hồi bản thay thế | Không dùng bản đó cho câu mới, không tự khôi phục bản trước |
| AT-10 | Tắt và chạy lại web | Quy chế, lịch sử và trích dẫn vẫn còn |
