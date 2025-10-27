using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaVirtual.Migrations
{
    /// <inheritdoc />
    public partial class AddEnPromocionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
     name: "PrecioPromocional",
     table: "Productos",
     type: "NUMBER(18,2)", 
     nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
