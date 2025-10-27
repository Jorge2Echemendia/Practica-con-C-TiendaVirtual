using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaVirtual.Migrations
{
    /// <inheritdoc />
    public partial class AddEnPromocionColumn2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
    name: "PrecioPromocional",
    table: "Productos",
    type: "NUMBER(18,2)", // Oracle usa NUMBER para decimales
    nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
