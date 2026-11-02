using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlmaDelMarPampeano.Migrations
{
    /// <inheritdoc />
    public partial class FotosLampara : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FotosLampara",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LamparaId = table.Column<int>(type: "int", nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FotosLampara", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FotosLampara_Lamparas_LamparaId",
                        column: x => x.LamparaId,
                        principalTable: "Lamparas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FotosLampara_LamparaId",
                table: "FotosLampara",
                column: "LamparaId");

            // Migración de datos: cada foto que ya estaba en Lamparas.RutaImagen
            // pasa a la tabla nueva como portada (Orden 0).
            migrationBuilder.Sql(@"
                INSERT INTO FotosLampara (LamparaId, RutaArchivo, Orden)
                SELECT Id, RutaImagen, 0
                FROM Lamparas
                WHERE RutaImagen IS NOT NULL AND RutaImagen <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FotosLampara");
        }
    }
}