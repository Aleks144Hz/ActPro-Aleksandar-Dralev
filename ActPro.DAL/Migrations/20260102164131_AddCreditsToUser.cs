using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActPro.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Credits",
                table: "AspNetUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Credits",
                table: "AspNetUsers");
        }
    }
}
