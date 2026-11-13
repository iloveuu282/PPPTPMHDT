using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TroLyQuyChe.Web.Data.Migrations;

[DbContext(typeof(RegulationDbContext))]
[Migration("20261111145000_AllowSameDayCorrection")]
public class AllowSameDayCorrection : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(Trigger(false));
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(Trigger(true));
    private static string Trigger(bool requireLaterDay) => $$"""
        CREATE OR ALTER TRIGGER dbo.TR_VersionSuccession_Immutable ON dbo.DocumentVersions AFTER INSERT, UPDATE AS
        BEGIN
            SET NOCOUNT ON;
            IF EXISTS (SELECT 1 FROM deleted d JOIN inserted i ON i.Id=d.Id
                WHERE d.Status<>N'Draft' AND
                (ISNULL(i.ReplacesVersionId,-1)<>ISNULL(d.ReplacesVersionId,-1)
                OR (i.ApprovedAt<>d.ApprovedAt) OR (i.ApprovedAt IS NULL AND d.ApprovedAt IS NOT NULL)
                OR (i.ApprovedAt IS NOT NULL AND d.ApprovedAt IS NULL)))
                THROW 51004, N'Không được sửa quan hệ thay thế hoặc ngày duyệt của nguồn đã duyệt.', 1;
            IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.DocumentVersions p ON p.Id=i.ReplacesVersionId
                WHERE i.DocumentId<>p.DocumentId OR i.EffectiveFrom{{(requireLaterDay ? "<=" : "<")}}p.EffectiveFrom
                OR i.Status=N'Draft' OR p.Status=N'Draft')
                THROW 51005, N'Bản thay thế phải cùng văn bản, đã duyệt và không có ngày hiệu lực sớm hơn bản trước.', 1;
            IF EXISTS (SELECT 1 FROM inserted i LEFT JOIN deleted d ON d.Id=i.Id
                WHERE i.Status=N'Approved' AND (d.Id IS NULL OR d.Status=N'Draft') AND i.ApprovedAt IS NULL)
                THROW 51006, N'Duyệt mới phải lưu thời điểm duyệt.', 1;
        END
        """;
}
