using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K_K.Data.Migrations
{
    /// <inheritdoc />
    public partial class Migracija3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'Proizvod' AND COLUMN_NAME = 'Hrana_Velicina')
        BEGIN
            ALTER TABLE [Proizvod] DROP COLUMN [Hrana_Velicina];
        END");

            migrationBuilder.AlterColumn<int>(
                name: "Velicina",
                table: "Proizvod",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Velicina",
                table: "Proizvod",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Hrana_Velicina",
                table: "Proizvod",
                type: "int",
                nullable: true);
        }
    }
}
