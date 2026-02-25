using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActPro.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddEnglishNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Cities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Activities",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Activities");
        }
    }
}
