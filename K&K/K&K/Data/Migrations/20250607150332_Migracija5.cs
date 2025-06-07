using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K_K.Data.Migrations
{
    /// <inheritdoc />
    public partial class Migracija5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Korpa_Osoba_KorisnikId",
                table: "Korpa");

            migrationBuilder.DropForeignKey(
                name: "FK_Narudzba_Osoba_KorisnikId",
                table: "Narudzba");

            migrationBuilder.DropForeignKey(
                name: "FK_Narudzba_Osoba_RadnikId",
                table: "Narudzba");

            migrationBuilder.DropForeignKey(
                name: "FK_Obavijest_Osoba_KorisnikId",
                table: "Obavijest");

            migrationBuilder.DropForeignKey(
                name: "FK_Recenzija_Osoba_KorisnikId",
                table: "Recenzija");

            migrationBuilder.DropTable(
                name: "Osoba");

            migrationBuilder.AlterColumn<string>(
                name: "KorisnikId",
                table: "Recenzija",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "DatumDodavanja",
                table: "Recenzija",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Tekst",
                table: "Recenzija",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "KorisnikId",
                table: "Obavijest",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "RadnikId",
                table: "Narudzba",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "KorisnikId",
                table: "Narudzba",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "KorisnikId",
                table: "Korpa",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Adresa",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ime",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prezime",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Uloga",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Korpa_AspNetUsers_KorisnikId",
                table: "Korpa",
                column: "KorisnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Narudzba_AspNetUsers_KorisnikId",
                table: "Narudzba",
                column: "KorisnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Narudzba_AspNetUsers_RadnikId",
                table: "Narudzba",
                column: "RadnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Obavijest_AspNetUsers_KorisnikId",
                table: "Obavijest",
                column: "KorisnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Recenzija_AspNetUsers_KorisnikId",
                table: "Recenzija",
                column: "KorisnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Korpa_AspNetUsers_KorisnikId",
                table: "Korpa");

            migrationBuilder.DropForeignKey(
                name: "FK_Narudzba_AspNetUsers_KorisnikId",
                table: "Narudzba");

            migrationBuilder.DropForeignKey(
                name: "FK_Narudzba_AspNetUsers_RadnikId",
                table: "Narudzba");

            migrationBuilder.DropForeignKey(
                name: "FK_Obavijest_AspNetUsers_KorisnikId",
                table: "Obavijest");

            migrationBuilder.DropForeignKey(
                name: "FK_Recenzija_AspNetUsers_KorisnikId",
                table: "Recenzija");

            migrationBuilder.DropColumn(
                name: "DatumDodavanja",
                table: "Recenzija");

            migrationBuilder.DropColumn(
                name: "Tekst",
                table: "Recenzija");

            migrationBuilder.DropColumn(
                name: "Adresa",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Prezime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Uloga",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "KorisnikId",
                table: "Recenzija",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "KorisnikId",
                table: "Obavijest",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "RadnikId",
                table: "Narudzba",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "KorisnikId",
                table: "Narudzba",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "KorisnikId",
                table: "Korpa",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateTable(
                name: "Osoba",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KorisnickoIme = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lozinka = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Uloga = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Osoba", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Korpa_Osoba_KorisnikId",
                table: "Korpa",
                column: "KorisnikId",
                principalTable: "Osoba",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Narudzba_Osoba_KorisnikId",
                table: "Narudzba",
                column: "KorisnikId",
                principalTable: "Osoba",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Narudzba_Osoba_RadnikId",
                table: "Narudzba",
                column: "RadnikId",
                principalTable: "Osoba",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Obavijest_Osoba_KorisnikId",
                table: "Obavijest",
                column: "KorisnikId",
                principalTable: "Osoba",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Recenzija_Osoba_KorisnikId",
                table: "Recenzija",
                column: "KorisnikId",
                principalTable: "Osoba",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
