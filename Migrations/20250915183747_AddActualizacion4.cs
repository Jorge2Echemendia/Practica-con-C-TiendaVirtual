using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaVirtual.Migrations
{
    /// <inheritdoc />
    public partial class AddActualizacion4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TiempoPromocion",
                table: "Productos",
                type: "TIMESTAMP(7)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TiempoPromocion",
                table: "Productos");
        }
    }
}
