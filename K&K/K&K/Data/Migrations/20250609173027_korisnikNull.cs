using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K_K.Data.Migrations
{
    /// <inheritdoc />
    public partial class korisnikNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Korpa_AspNetUsers_KorisnikId",
                table: "Korpa");

            migrationBuilder.AlterColumn<string>(
                name: "KorisnikId",
                table: "Korpa",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Korpa_AspNetUsers_KorisnikId",
                table: "Korpa",
                column: "KorisnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Korpa_AspNetUsers_KorisnikId",
                table: "Korpa");

            migrationBuilder.AlterColumn<string>(
                name: "KorisnikId",
                table: "Korpa",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Korpa_AspNetUsers_KorisnikId",
                table: "Korpa",
                column: "KorisnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
