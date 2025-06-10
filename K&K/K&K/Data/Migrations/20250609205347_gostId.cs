using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K_K.Data.Migrations
{
    /// <inheritdoc />
    public partial class gostId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GostId",
                table: "Korpa",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GostId",
                table: "Korpa");
        }
    }
}
