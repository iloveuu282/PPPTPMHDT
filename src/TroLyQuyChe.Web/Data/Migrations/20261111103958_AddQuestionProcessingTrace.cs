using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TroLyQuyChe.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionProcessingTrace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessingTraceJson",
                table: "QaEntries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessingTraceJson",
                table: "QaEntries");
        }
    }
}
