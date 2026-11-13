using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TroLyQuyChe.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalPoliciesAndAdministration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "DocumentVersions",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VersionActions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentVersionId = table.Column<int>(type: "int", nullable: false),
                    ActorUserId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VersionActions_AppUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VersionActions_DocumentVersions_DocumentVersionId",
                        column: x => x.DocumentVersionId,
                        principalTable: "DocumentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Documents",
                columns: new[] { "Id", "Code", "Title" },
                values: new object[,]
                {
                    { 3, "QC-TB", "Nhận và bàn giao thiết bị" },
                    { 4, "QC-NP-HM", "Hạn mức và đăng ký nghỉ phép năm" },
                    { 5, "QC-LV-BS", "Hướng dẫn làm việc từ xa" }
                });

            migrationBuilder.InsertData(
                table: "DocumentVersions",
                columns: new[] { "Id", "DocumentId", "EffectiveFrom", "EffectiveTo", "Status", "VersionLabel" },
                values: new object[,]
                {
                    { 5, 3, new DateOnly(2026, 1, 1), null, "Approved", "1.0" },
                    { 6, 4, new DateOnly(2026, 1, 1), null, "Approved", "1.0" },
                    { 7, 5, new DateOnly(2026, 1, 1), null, "Approved", "1.0" }
                });

            migrationBuilder.InsertData(
                table: "Clauses",
                columns: new[] { "Id", "ArticleNumber", "Body", "DocumentVersionId", "MaximumLeaveDaysInclusive", "MinimumLeaveDaysExclusive", "SourceLocation", "Topic" },
                values: new object[,]
                {
                    { 51, "Điều 1", "Nhân viên ký biên bản khi nhận thiết bị.", 5, null, null, "docs/nghiep-vu/04_QuyChe_BoSung_Final.md • QC-TB v1.0", "EquipmentReceipt" },
                    { 52, "Điều 2", "Khi nghỉ việc, nhân viên phải bàn giao thiết bị cho bộ phận IT chậm nhất vào ngày làm việc cuối cùng.", 5, null, null, "docs/nghiep-vu/04_QuyChe_BoSung_Final.md • QC-TB v1.0", "EquipmentReturn" },
                    { 53, "Điều 3", "Khi thiết bị bị mất hoặc hư hỏng, nhân viên báo ngay cho bộ phận IT qua cổng hỗ trợ nội bộ và giữ nguyên hiện trạng để kiểm tra.", 5, null, null, "docs/nghiep-vu/04_QuyChe_BoSung_Final.md • QC-TB v1.0", "EquipmentDamage" },
                    { 54, "Điều 4", "Thiết bị công ty phải bật khóa màn hình khi rời chỗ, không chia sẻ mật khẩu và không cho người ngoài mượn sử dụng.", 5, null, null, "docs/nghiep-vu/04_QuyChe_BoSung_Final.md • QC-TB v1.0", "EquipmentSecurity" },
                    { 55, "Điều 5", "Khi bàn giao, nhân viên hoàn trả laptop, bộ sạc và phụ kiện đã ghi trong biên bản nhận; bộ phận IT kiểm tra và ký biên bản bàn giao.", 5, null, null, "docs/nghiep-vu/04_QuyChe_BoSung_Final.md • QC-TB v1.0", "EquipmentReturnItems" },
                    { 61, "Điều 1", "Nhân viên làm đủ 12 tháng được nghỉ phép năm tối đa 12 ngày làm việc trong mỗi năm dương lịch. Hạn mức này dành riêng cho nghỉ phép năm, không phải hạn mức cho mỗi lần nghỉ và không bao gồm các loại nghỉ khác. Kho mẫu chưa quy định cách tính cho người làm chưa đủ 12 tháng.", 6, null, null, "docs/nghiep-vu/04_QuyChe_BoSung_Final.md • QC-NP-HM v1.0", "LeaveAllowance" },
                    { 62, "Điều 2", "Phép năm chưa sử dụng được chuyển tối đa 5 ngày làm việc sang năm kế tiếp và phải dùng hết trước ngày 31/03 của năm đó; phần còn lại không được chuyển tiếp.", 6, null, null, "docs/nghiep-vu/04_QuyChe_BoSung_Final.md • QC-NP-HM v1.0", "LeaveCarryover" },
                    { 63, "Điều 3", "Nhân viên gửi yêu cầu nghỉ phép năm trên cổng nhân sự, ghi ngày bắt đầu, ngày kết thúc và số ngày làm việc dự định nghỉ; quản lý trực tiếp phê duyệt. Thời hạn báo trước thực hiện theo QC-NP.", 6, null, null, "docs/nghiep-vu/04_QuyChe_BoSung_Final.md • QC-NP-HM v1.0", "LeaveRequest" },
                    { 71, "Điều 1", "Yêu cầu làm việc từ xa phải gửi trên cổng nhân sự trước 16:00 của ngày làm việc liền trước và được quản lý trực tiếp duyệt trước khi thực hiện.", 7, null, null, "docs/nghiep-vu/04_QuyChe_BoSung_Final.md • QC-LV-BS v1.0", "RemoteNotice" },
                    { 72, "Điều 2", "Khi làm việc từ xa, nhân viên trực tuyến trong giờ làm việc 08:30–12:00 và 13:00–17:30, tham dự các cuộc họp đã lên lịch và cập nhật tiến độ cho quản lý trực tiếp.", 7, null, null, "docs/nghiep-vu/04_QuyChe_BoSung_Final.md • QC-LV-BS v1.0", "RemoteAvailability" },
                    { 73, "Điều 3", "Khi làm việc từ xa, nhân viên truy cập tài nguyên công ty qua VPN, dùng thiết bị được công ty quản lý và không lưu dữ liệu nội bộ trên máy tính công cộng.", 7, null, null, "docs/nghiep-vu/04_QuyChe_BoSung_Final.md • QC-LV-BS v1.0", "RemoteSecurity" }
                });

            migrationBuilder.Sql("""
                CREATE TRIGGER dbo.TR_Clauses_Immutable ON dbo.Clauses AFTER INSERT, UPDATE, DELETE AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM deleted d JOIN dbo.DocumentVersions v ON v.Id=d.DocumentVersionId WHERE v.Status<>N'Draft')
                       OR EXISTS (SELECT 1 FROM inserted i JOIN dbo.DocumentVersions v ON v.Id=i.DocumentVersionId WHERE v.Status<>N'Draft')
                        THROW 51001, N'Không được thay đổi điều khoản của phiên bản đã duyệt hoặc thu hồi.', 1;
                END
                """);
            migrationBuilder.Sql("""
                CREATE TRIGGER dbo.TR_Versions_Immutable ON dbo.DocumentVersions AFTER UPDATE, DELETE AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM deleted d LEFT JOIN inserted i ON i.Id=d.Id
                        WHERE d.Status<>N'Draft' AND (i.Id IS NULL OR i.DocumentId<>d.DocumentId OR i.VersionLabel<>d.VersionLabel
                        OR i.EffectiveFrom<>d.EffectiveFrom OR ISNULL(i.EffectiveTo,'9999-12-31')<>ISNULL(d.EffectiveTo,'9999-12-31')
                        OR NOT(i.Status=d.Status OR (d.Status=N'Approved' AND i.Status=N'Revoked'))))
                        THROW 51002, N'Phiên bản đã duyệt giữ nguyên nội dung và hiệu lực; chỉ cho phép thu hồi.', 1;
                END
                """);
            migrationBuilder.Sql("""
                CREATE TRIGGER dbo.TR_Documents_Immutable ON dbo.Documents AFTER UPDATE, DELETE AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM deleted d LEFT JOIN inserted i ON i.Id=d.Id
                        WHERE EXISTS(SELECT 1 FROM dbo.DocumentVersions v WHERE v.DocumentId=d.Id AND v.Status<>N'Draft')
                        AND (i.Id IS NULL OR i.Code<>d.Code OR i.Title<>d.Title))
                        THROW 51003, N'Không được đổi mã/tên văn bản đã có phiên bản được duyệt.', 1;
                END
                """);
            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_Username",
                table: "AppUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VersionActions_ActorUserId",
                table: "VersionActions",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VersionActions_DocumentVersionId",
                table: "VersionActions",
                column: "DocumentVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.TR_Clauses_Immutable; DROP TRIGGER IF EXISTS dbo.TR_Versions_Immutable; DROP TRIGGER IF EXISTS dbo.TR_Documents_Immutable;");
            migrationBuilder.DropTable(
                name: "VersionActions");

            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.DeleteData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "DocumentVersions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "DocumentVersions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "DocumentVersions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "DocumentVersions");
        }
    }
}
