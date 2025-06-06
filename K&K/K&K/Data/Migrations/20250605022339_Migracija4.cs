using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K_K.Data.Migrations
{
    /// <inheritdoc />
    public partial class Migracija4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Korpa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KorisnikId = table.Column<int>(type: "int", nullable: false),
                    brojProizvoda = table.Column<int>(type: "int", nullable: false),
                    ukupnaCijena = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Korpa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Korpa_Osoba_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Osoba",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StavkaKorpe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KorpaId = table.Column<int>(type: "int", nullable: false),
                    ProizvodId = table.Column<int>(type: "int", nullable: false),
                    Kolicina = table.Column<int>(type: "int", nullable: false),
                    Cijena = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StavkaKorpe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StavkaKorpe_Korpa_KorpaId",
                        column: x => x.KorpaId,
                        principalTable: "Korpa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StavkaKorpe_Proizvod_ProizvodId",
                        column: x => x.ProizvodId,
                        principalTable: "Proizvod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Korpa_KorisnikId",
                table: "Korpa",
                column: "KorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_StavkaKorpe_KorpaId",
                table: "StavkaKorpe",
                column: "KorpaId");

            migrationBuilder.CreateIndex(
                name: "IX_StavkaKorpe_ProizvodId",
                table: "StavkaKorpe",
                column: "ProizvodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StavkaKorpe");

            migrationBuilder.DropTable(
                name: "Korpa");
        }
    }
}
