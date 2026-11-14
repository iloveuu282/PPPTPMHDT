using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TroLyQuyChe.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveNoticeRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MaximumLeaveDaysInclusive",
                table: "Clauses",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumLeaveDaysExclusive",
                table: "Clauses",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "MaximumLeaveDaysInclusive", "MinimumLeaveDaysExclusive" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "MaximumLeaveDaysInclusive", "MinimumLeaveDaysExclusive" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "MaximumLeaveDaysInclusive", "MinimumLeaveDaysExclusive" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "MaximumLeaveDaysInclusive", "MinimumLeaveDaysExclusive" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "MaximumLeaveDaysInclusive", "MinimumLeaveDaysExclusive" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "MaximumLeaveDaysInclusive", "MinimumLeaveDaysExclusive" },
                values: new object[] { null, null });

            migrationBuilder.InsertData(
                table: "Documents",
                columns: new[] { "Id", "Code", "Title" },
                values: new object[] { 2, "QC-NP", "Đăng ký nghỉ phép" });

            migrationBuilder.InsertData(
                table: "DocumentVersions",
                columns: new[] { "Id", "DocumentId", "EffectiveFrom", "EffectiveTo", "Status", "VersionLabel" },
                values: new object[] { 4, 2, new DateOnly(2026, 1, 1), null, "Approved", "1.0" });

            migrationBuilder.InsertData(
                table: "Clauses",
                columns: new[] { "Id", "ArticleNumber", "Body", "DocumentVersionId", "MaximumLeaveDaysInclusive", "MinimumLeaveDaysExclusive", "SourceLocation", "Topic" },
                values: new object[,]
                {
                    { 41, "Điều 1", "Nghỉ từ 1 ngày làm việc trở xuống phải gửi yêu cầu trước ít nhất 2 ngày làm việc và được quản lý trực tiếp duyệt.", 4, 1m, 0m, "Nguồn quy chế giả lập Mai Việt Hoàng (bộ cũ) • QC-NP v1.0", "LeaveNotice" },
                    { 42, "Điều 2", "Nghỉ trên 1 ngày làm việc phải gửi yêu cầu trước ít nhất 5 ngày làm việc và được quản lý trực tiếp duyệt.", 4, null, 1m, "Nguồn quy chế giả lập Mai Việt Hoàng (bộ cũ) • QC-NP v1.0", "LeaveNotice" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Clauses",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "DocumentVersions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "MaximumLeaveDaysInclusive",
                table: "Clauses");

            migrationBuilder.DropColumn(
                name: "MinimumLeaveDaysExclusive",
                table: "Clauses");
        }
    }
}
