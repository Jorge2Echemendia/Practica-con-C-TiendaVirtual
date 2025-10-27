using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaVirtual.Migrations
{
    /// <inheritdoc />
    public partial class AddPrecioUnitarioToItemCarrito1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioUnitario",
                table: "ItemsCarrito",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioUnitario",
                table: "ItemsCarrito",
                type: "DECIMAL(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
