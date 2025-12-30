using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActPro.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingToComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AspNetUserId",
                table: "Comments",
                column: "AspNetUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_AspNetUserId",
                table: "Comments",
                column: "AspNetUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_AspNetUserId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_AspNetUserId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Comments");
        }
    }
}
