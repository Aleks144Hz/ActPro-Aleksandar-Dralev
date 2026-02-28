using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActPro.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RevertAuditLogBilingual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetailsBg",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "DetailsEn",
                table: "AuditLogs",
                newName: "Details");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Details",
                table: "AuditLogs",
                newName: "DetailsEn");

            migrationBuilder.AddColumn<string>(
                name: "DetailsBg",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
