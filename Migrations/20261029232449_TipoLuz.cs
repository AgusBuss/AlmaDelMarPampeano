using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlmaDelMarPampeano.Migrations
{
    /// <inheritdoc />
    public partial class TipoLuz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoLuzId",
                table: "DetallesPedido",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TiposLuz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposLuz", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedido_TipoLuzId",
                table: "DetallesPedido",
                column: "TipoLuzId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPedido_TiposLuz_TipoLuzId",
                table: "DetallesPedido",
                column: "TipoLuzId",
                principalTable: "TiposLuz",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesPedido_TiposLuz_TipoLuzId",
                table: "DetallesPedido");

            migrationBuilder.DropTable(
                name: "TiposLuz");

            migrationBuilder.DropIndex(
                name: "IX_DetallesPedido_TipoLuzId",
                table: "DetallesPedido");

            migrationBuilder.DropColumn(
                name: "TipoLuzId",
                table: "DetallesPedido");
        }
    }
}
