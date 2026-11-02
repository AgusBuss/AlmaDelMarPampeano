using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlmaDelMarPampeano.Migrations
{
    /// <inheritdoc />
    public partial class Lamparas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lamparas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NumeroPieza = table.Column<int>(type: "int", nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RutaImagen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Disponible = table.Column<bool>(type: "bit", nullable: false),
                    MaterialCampanaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lamparas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lamparas_MaterialesCampana_MaterialCampanaId",
                        column: x => x.MaterialCampanaId,
                        principalTable: "MaterialesCampana",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lamparas_MaterialCampanaId",
                table: "Lamparas",
                column: "MaterialCampanaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lamparas");
        }
    }
}
