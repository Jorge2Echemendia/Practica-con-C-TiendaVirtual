using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaVirtual.Migrations
{
    /// <inheritdoc />
    public partial class AddPrecioUnitarioToItemCarrito3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Adreess",
                table: "Compra",
                type: "NVARCHAR2(2000)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adreess",
                table: "Compra");
        }
    }
}
