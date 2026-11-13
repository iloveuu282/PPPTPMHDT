using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TroLyQuyChe.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class VersionSuccession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovedAt",
                table: "DocumentVersions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReplacesVersionId",
                table: "DocumentVersions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_ReplacesVersionId",
                table: "DocumentVersions",
                column: "ReplacesVersionId",
                unique: true,
                filter: "[ReplacesVersionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentVersions_DocumentVersions_ReplacesVersionId",
                table: "DocumentVersions",
                column: "ReplacesVersionId",
                principalTable: "DocumentVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.Sql("""
                CREATE TRIGGER dbo.TR_VersionSuccession_Immutable ON dbo.DocumentVersions AFTER INSERT, UPDATE AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM deleted d JOIN inserted i ON i.Id=d.Id
                        WHERE d.Status<>N'Draft' AND
                        (ISNULL(i.ReplacesVersionId,-1)<>ISNULL(d.ReplacesVersionId,-1)
                        OR (i.ApprovedAt<>d.ApprovedAt) OR (i.ApprovedAt IS NULL AND d.ApprovedAt IS NOT NULL)
                        OR (i.ApprovedAt IS NOT NULL AND d.ApprovedAt IS NULL)))
                        THROW 51004, N'Không được sửa quan hệ thay thế hoặc ngày duyệt của nguồn đã duyệt.', 1;
                    IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.DocumentVersions p ON p.Id=i.ReplacesVersionId
                        WHERE i.DocumentId<>p.DocumentId OR i.EffectiveFrom<=p.EffectiveFrom
                        OR i.Status=N'Draft' OR p.Status=N'Draft')
                        THROW 51005, N'Bản thay thế phải cùng văn bản, đã duyệt và có ngày hiệu lực sau bản trước.', 1;
                    IF EXISTS (SELECT 1 FROM inserted i LEFT JOIN deleted d ON d.Id=i.Id
                        WHERE i.Status=N'Approved' AND (d.Id IS NULL OR d.Status=N'Draft') AND i.ApprovedAt IS NULL)
                        THROW 51006, N'Duyệt mới phải lưu thời điểm duyệt.', 1;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.TR_VersionSuccession_Immutable;");
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentVersions_DocumentVersions_ReplacesVersionId",
                table: "DocumentVersions");

            migrationBuilder.DropIndex(
                name: "IX_DocumentVersions_ReplacesVersionId",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "ReplacesVersionId",
                table: "DocumentVersions");
        }
    }
}
