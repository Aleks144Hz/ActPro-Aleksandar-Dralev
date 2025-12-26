using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActPro.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixNamingFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites1_Places",
                table: "Favorites");

            migrationBuilder.AlterColumn<int>(
                name: "PlaceId",
                table: "Favorites",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AspNetUserId",
                table: "Favorites",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_AspNetUserId",
                table: "Favorites",
                column: "AspNetUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites1_AspNetUsers",
                table: "Favorites",
                column: "AspNetUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites1_Places",
                table: "Favorites",
                column: "PlaceId",
                principalTable: "Places",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites1_AspNetUsers",
                table: "Favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_Favorites1_Places",
                table: "Favorites");

            migrationBuilder.DropIndex(
                name: "IX_Favorites_AspNetUserId",
                table: "Favorites");

            migrationBuilder.AlterColumn<int>(
                name: "PlaceId",
                table: "Favorites",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "AspNetUserId",
                table: "Favorites",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites1_Places",
                table: "Favorites",
                column: "PlaceId",
                principalTable: "Places",
                principalColumn: "Id");
        }
    }
}
