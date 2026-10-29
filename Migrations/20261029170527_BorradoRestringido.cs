using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlmaDelMarPampeano.Migrations
{
    /// <inheritdoc />
    public partial class BorradoRestringido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesPedido_MaterialesBase_MaterialBaseId",
                table: "DetallesPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesPedido_MaterialesCampana_MaterialCampanaId",
                table: "DetallesPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_EstadosPedido_EstadoPedidoId",
                table: "Pedidos");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPedido_MaterialesBase_MaterialBaseId",
                table: "DetallesPedido",
                column: "MaterialBaseId",
                principalTable: "MaterialesBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPedido_MaterialesCampana_MaterialCampanaId",
                table: "DetallesPedido",
                column: "MaterialCampanaId",
                principalTable: "MaterialesCampana",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_EstadosPedido_EstadoPedidoId",
                table: "Pedidos",
                column: "EstadoPedidoId",
                principalTable: "EstadosPedido",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesPedido_MaterialesBase_MaterialBaseId",
                table: "DetallesPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesPedido_MaterialesCampana_MaterialCampanaId",
                table: "DetallesPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_EstadosPedido_EstadoPedidoId",
                table: "Pedidos");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPedido_MaterialesBase_MaterialBaseId",
                table: "DetallesPedido",
                column: "MaterialBaseId",
                principalTable: "MaterialesBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPedido_MaterialesCampana_MaterialCampanaId",
                table: "DetallesPedido",
                column: "MaterialCampanaId",
                principalTable: "MaterialesCampana",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_EstadosPedido_EstadoPedidoId",
                table: "Pedidos",
                column: "EstadoPedidoId",
                principalTable: "EstadosPedido",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
