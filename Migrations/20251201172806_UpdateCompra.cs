using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaVirtual.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adreess",
                table: "Compra");

            migrationBuilder.AddColumn<decimal>(
                name: "Lat",
                table: "Compra",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Lon",
                table: "Compra",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lat",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "Lon",
                table: "Compra");

            migrationBuilder.AddColumn<string>(
                name: "Adreess",
                table: "Compra",
                type: "NVARCHAR2(2000)",
                nullable: true);
        }
    }
}
