using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlmaDelMarPampeano.Migrations
{
    /// <inheritdoc />
    public partial class LamparaEnDetallePedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LamparaId",
                table: "DetallesPedido",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedido_LamparaId",
                table: "DetallesPedido",
                column: "LamparaId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPedido_Lamparas_LamparaId",
                table: "DetallesPedido",
                column: "LamparaId",
                principalTable: "Lamparas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesPedido_Lamparas_LamparaId",
                table: "DetallesPedido");

            migrationBuilder.DropIndex(
                name: "IX_DetallesPedido_LamparaId",
                table: "DetallesPedido");

            migrationBuilder.DropColumn(
                name: "LamparaId",
                table: "DetallesPedido");
        }
    }
}
