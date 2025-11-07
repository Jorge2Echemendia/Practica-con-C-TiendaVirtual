using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaVirtual.Migrations
{
    /// <inheritdoc />
    public partial class AddDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Check",
                table: "Compra",
                type: "NVARCHAR2(2000)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryPersonId",
                table: "Compra",
                type: "NUMBER(10)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeliveryPersons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Vehiculo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Estado = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryPersons_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Compra_DeliveryPersonId",
                table: "Compra",
                column: "DeliveryPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPersons_UserId",
                table: "DeliveryPersons",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Compra_DeliveryPersons_DeliveryPersonId",
                table: "Compra",
                column: "DeliveryPersonId",
                principalTable: "DeliveryPersons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compra_DeliveryPersons_DeliveryPersonId",
                table: "Compra");

            migrationBuilder.DropTable(
                name: "DeliveryPersons");

            migrationBuilder.DropIndex(
                name: "IX_Compra_DeliveryPersonId",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "Check",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "DeliveryPersonId",
                table: "Compra");
        }
    }
}
