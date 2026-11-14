using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TroLyQuyChe.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialQcLv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QaEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Question = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ApplicableFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ApplicableTo = table.Column<DateOnly>(type: "date", nullable: true),
                    AskedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QaEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    VersionLabel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentVersions", x => x.Id);
                    table.CheckConstraint("CK_Version_Dates", "[EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom]");
                    table.ForeignKey(
                        name: "FK_DocumentVersions_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clauses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentVersionId = table.Column<int>(type: "int", nullable: false),
                    ArticleNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceLocation = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clauses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clauses_DocumentVersions_DocumentVersionId",
                        column: x => x.DocumentVersionId,
                        principalTable: "DocumentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Citations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QaEntryId = table.Column<long>(type: "bigint", nullable: false),
                    ClauseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Citations_Clauses_ClauseId",
                        column: x => x.ClauseId,
                        principalTable: "Clauses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Citations_QaEntries_QaEntryId",
                        column: x => x.QaEntryId,
                        principalTable: "QaEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Documents",
                columns: new[] { "Id", "Code", "Title" },
                values: new object[] { 1, "QC-LV", "Giờ làm việc và làm việc từ xa" });

            migrationBuilder.InsertData(
                table: "DocumentVersions",
                columns: new[] { "Id", "DocumentId", "EffectiveFrom", "EffectiveTo", "Status", "VersionLabel" },
                values: new object[,]
                {
                    { 1, 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), "Approved", "1.0" },
                    { 2, 1, new DateOnly(2026, 7, 1), null, "Approved", "2.0" },
                    { 3, 1, new DateOnly(2026, 7, 1), null, "Draft", "3.0" }
                });

            migrationBuilder.InsertData(
                table: "Clauses",
                columns: new[] { "Id", "ArticleNumber", "Body", "DocumentVersionId", "SourceLocation", "Topic" },
                values: new object[,]
                {
                    { 11, "Điều 1", "Giờ làm việc từ 08:30 đến 17:30, thứ Hai đến thứ Sáu. Thời gian nghỉ trưa từ 12:00 đến 13:00.", 1, "Nguồn quy chế giả lập Mai Việt Hoàng (bộ cũ) • QC-LV v1.0", "WorkingHours" },
                    { 12, "Điều 2", "Nhân viên được làm việc từ xa tối đa 1 ngày mỗi tuần. Yêu cầu làm việc từ xa phải được quản lý trực tiếp duyệt.", 1, "Nguồn quy chế giả lập Mai Việt Hoàng (bộ cũ) • QC-LV v1.0", "RemoteWork" },
                    { 21, "Điều 1", "Giờ làm việc từ 08:30 đến 17:30, thứ Hai đến thứ Sáu. Thời gian nghỉ trưa từ 12:00 đến 13:00.", 2, "Nguồn quy chế giả lập Mai Việt Hoàng (bộ cũ) • QC-LV v2.0", "WorkingHours" },
                    { 22, "Điều 2", "Nhân viên được làm việc từ xa tối đa 2 ngày mỗi tuần. Yêu cầu làm việc từ xa phải được quản lý trực tiếp duyệt.", 2, "Nguồn quy chế giả lập Mai Việt Hoàng (bộ cũ) • QC-LV v2.0", "RemoteWork" },
                    { 31, "Điều 1", "Giờ làm việc từ 08:30 đến 17:30, thứ Hai đến thứ Sáu. Thời gian nghỉ trưa từ 12:00 đến 13:00.", 3, "Nguồn quy chế giả lập Mai Việt Hoàng (bộ cũ) • QC-LV v3.0", "WorkingHours" },
                    { 32, "Điều 2", "Nhân viên được làm việc từ xa tối đa 3 ngày mỗi tuần. Yêu cầu làm việc từ xa phải được quản lý trực tiếp duyệt.", 3, "Nguồn quy chế giả lập Mai Việt Hoàng (bộ cũ) • QC-LV v3.0", "RemoteWork" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citations_ClauseId",
                table: "Citations",
                column: "ClauseId");

            migrationBuilder.CreateIndex(
                name: "IX_Citations_QaEntryId_ClauseId",
                table: "Citations",
                columns: new[] { "QaEntryId", "ClauseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clauses_DocumentVersionId_ArticleNumber",
                table: "Clauses",
                columns: new[] { "DocumentVersionId", "ArticleNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Code",
                table: "Documents",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_DocumentId_VersionLabel",
                table: "DocumentVersions",
                columns: new[] { "DocumentId", "VersionLabel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QaEntries_OwnerId_AskedAt",
                table: "QaEntries",
                columns: new[] { "OwnerId", "AskedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Citations");

            migrationBuilder.DropTable(
                name: "Clauses");

            migrationBuilder.DropTable(
                name: "QaEntries");

            migrationBuilder.DropTable(
                name: "DocumentVersions");

            migrationBuilder.DropTable(
                name: "Documents");
        }
    }
}
