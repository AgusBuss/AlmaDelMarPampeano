using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlmaDelMarPampeano.Migrations
{
    /// <inheritdoc />
    public partial class AjustesModelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoBaseMaderaId",
                table: "DetallesPedido");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoBaseMaderaId",
                table: "DetallesPedido",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
