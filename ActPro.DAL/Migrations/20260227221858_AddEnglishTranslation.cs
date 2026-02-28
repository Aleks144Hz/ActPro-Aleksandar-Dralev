using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActPro.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddEnglishTranslation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Places",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Places",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentEn",
                table: "News",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "News",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "ContentEn",
                table: "News");

            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "News");
        }
    }
}
