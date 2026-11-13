using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TroLyQuyChe.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceWithSimpleOfficePolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // User explicitly requested a complete replacement. Restore a verified backup to undo.
            // Migration transactions make the wipe, seed and trigger restoration atomic.
            migrationBuilder.Sql("""
                DISABLE TRIGGER ALL ON dbo.Clauses;
                DISABLE TRIGGER ALL ON dbo.DocumentVersions;
                DISABLE TRIGGER ALL ON dbo.Documents;
                DELETE FROM dbo.Citations;
                DELETE FROM dbo.QaEntries;
                DELETE FROM dbo.VersionActions;
                DELETE FROM dbo.Clauses;
                UPDATE dbo.DocumentVersions SET ReplacesVersionId = NULL;
                DELETE FROM dbo.DocumentVersions;
                DELETE FROM dbo.Documents;
                """);
            migrationBuilder.InsertData(
                table: "Documents",
                columns: new[] { "Id", "Code", "Title" },
                values: new object[,]
                {
                    { 101, "QC-THOIGIAN-LAMVIEC", "Thời gian làm việc" },
                    { 102, "QC-DANGKY-NGHI", "Đăng ký nghỉ" },
                    { 103, "QC-LAMVIEC-CONGTY", "Làm việc tại công ty" }
                });

            migrationBuilder.InsertData(
                table: "DocumentVersions",
                columns: new[] { "Id", "ApprovedAt", "DocumentId", "EffectiveFrom", "EffectiveTo", "ReplacesVersionId", "Status", "VersionLabel" },
                values: new object[,]
                {
                    { 101, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 101, new DateOnly(2026, 1, 1), null, null, "Approved", "1.0" },
                    { 102, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 102, new DateOnly(2026, 1, 1), null, null, "Approved", "1.0" },
                    { 103, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 103, new DateOnly(2026, 1, 1), null, null, "Approved", "1.0" }
                });

            migrationBuilder.InsertData(
                table: "Clauses",
                columns: new[] { "Id", "ArticleNumber", "Body", "DocumentVersionId", "MaximumLeaveDaysInclusive", "MinimumLeaveDaysExclusive", "SourceLocation", "Topic" },
                values: new object[,]
                {
                    { 1001, "Điều 1", "Bắt đầu làm việc lúc 8 giờ sáng.", 101, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Thời gian làm việc • 1.0", "WorkStart" },
                    { 1002, "Điều 2", "Kết thúc làm việc lúc 16 giờ (4 giờ chiều).", 101, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Thời gian làm việc • 1.0", "WorkEnd" },
                    { 1003, "Điều 3", "Làm việc 5 ngày mỗi tuần, từ thứ Hai đến thứ Sáu. Thứ Bảy và Chủ nhật nghỉ.", 101, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Thời gian làm việc • 1.0", "WorkSchedule" },
                    { 1004, "Điều 4", "Nghỉ trưa từ 12 giờ đến 13 giờ.", 101, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Thời gian làm việc • 1.0", "LunchBreak" },
                    { 1005, "Điều 5", "Làm việc 7 giờ mỗi ngày, không tính 1 giờ nghỉ trưa.", 101, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Thời gian làm việc • 1.0", "WorkDuration" },
                    { 2001, "Điều 1", "Được đăng ký nghỉ tối đa 3 ngày làm việc mỗi tuần.", 102, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Đăng ký nghỉ • 1.0", "LeaveWeekly" },
                    { 2002, "Điều 2", "Được đăng ký nghỉ tối đa 8 ngày làm việc mỗi tháng.", 102, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Đăng ký nghỉ • 1.0", "LeaveMonthly" },
                    { 2003, "Điều 3", "Giới hạn tuần và tháng áp dụng đồng thời. Muốn nghỉ vượt một trong hai giới hạn phải được trưởng bộ phận xét duyệt.", 102, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Đăng ký nghỉ • 1.0", "LeaveException" },
                    { 2004, "Điều 4", "Nghỉ 1 ngày làm việc phải gửi yêu cầu trước ít nhất 1 ngày làm việc.", 102, 1m, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Đăng ký nghỉ • 1.0", "LeaveNotice" },
                    { 2005, "Điều 5", "Nghỉ từ 2 ngày làm việc trở lên phải gửi yêu cầu trước ít nhất 5 ngày làm việc.", 102, null, 1m, "Bộ quy chế nội bộ Mai Việt Hoàng • Đăng ký nghỉ • 1.0", "LeaveNotice" },
                    { 2006, "Điều 6", "Yêu cầu nghỉ trong giới hạn phải được quản lý trực tiếp duyệt trước khi nghỉ.", 102, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Đăng ký nghỉ • 1.0", "LeaveApproval" },
                    { 2007, "Điều 7", "Gửi yêu cầu nghỉ cho quản lý trực tiếp, ghi rõ ngày bắt đầu, ngày kết thúc, số ngày làm việc dự định nghỉ và lý do.", 102, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Đăng ký nghỉ • 1.0", "LeaveRequest" },
                    { 2008, "Điều 8", "Tuần tính từ thứ Hai đến Chủ nhật; tháng tính theo tháng dương lịch. Số ngày nghỉ và thời hạn báo trước tính theo ngày làm việc.", 102, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Đăng ký nghỉ • 1.0", "LeaveCalendar" },
                    { 3001, "Điều 1", "Đi muộn phải báo cho quản lý trực tiếp trước giờ bắt đầu làm việc.", 103, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Làm việc tại công ty • 1.0", "LateArrival" },
                    { 3002, "Điều 2", "Muốn về sớm phải được quản lý trực tiếp đồng ý trước khi rời công ty.", 103, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Làm việc tại công ty • 1.0", "EarlyDeparture" },
                    { 3003, "Điều 3", "Ra ngoài trong giờ làm phải được quản lý trực tiếp đồng ý.", 103, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Làm việc tại công ty • 1.0", "LeavingOffice" },
                    { 3004, "Điều 4", "Khách phải đăng ký với lễ tân và được người phụ trách đồng ý trước khi vào khu vực làm việc.", 103, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Làm việc tại công ty • 1.0", "Visitors" },
                    { 3005, "Điều 5", "Đăng ký phòng họp với bộ phận hành chính trước khi sử dụng; kết thúc đúng giờ đã đăng ký.", 103, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Làm việc tại công ty • 1.0", "MeetingRoom" },
                    { 3006, "Điều 6", "Giữ trật tự tại khu vực làm việc, không làm ảnh hưởng đến người khác.", 103, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Làm việc tại công ty • 1.0", "WorkplaceOrder" },
                    { 3007, "Điều 7", "Giữ vệ sinh chỗ làm, bỏ rác đúng nơi quy định.", 103, null, null, "Bộ quy chế nội bộ Mai Việt Hoàng • Làm việc tại công ty • 1.0", "OfficeCleanliness" }
                });
            migrationBuilder.Sql("""
                ENABLE TRIGGER ALL ON dbo.Clauses;
                ENABLE TRIGGER ALL ON dbo.DocumentVersions;
                ENABLE TRIGGER ALL ON dbo.Documents;
                """);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Đã thay toàn bộ dữ liệu theo yêu cầu. Muốn khôi phục kho và lịch sử cũ phải phục hồi bản sao lưu trước thay đổi.");
        }
    }
}
