using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlmaDelMarPampeano.Migrations
{
    /// <inheritdoc />
    public partial class ColeccionesYBaseEnLamparas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColeccionId",
                table: "Lamparas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialBaseId",
                table: "Lamparas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Colecciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colecciones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lamparas_ColeccionId",
                table: "Lamparas",
                column: "ColeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_Lamparas_MaterialBaseId",
                table: "Lamparas",
                column: "MaterialBaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lamparas_Colecciones_ColeccionId",
                table: "Lamparas",
                column: "ColeccionId",
                principalTable: "Colecciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lamparas_MaterialesBase_MaterialBaseId",
                table: "Lamparas",
                column: "MaterialBaseId",
                principalTable: "MaterialesBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lamparas_Colecciones_ColeccionId",
                table: "Lamparas");

            migrationBuilder.DropForeignKey(
                name: "FK_Lamparas_MaterialesBase_MaterialBaseId",
                table: "Lamparas");

            migrationBuilder.DropTable(
                name: "Colecciones");

            migrationBuilder.DropIndex(
                name: "IX_Lamparas_ColeccionId",
                table: "Lamparas");

            migrationBuilder.DropIndex(
                name: "IX_Lamparas_MaterialBaseId",
                table: "Lamparas");

            migrationBuilder.DropColumn(
                name: "ColeccionId",
                table: "Lamparas");

            migrationBuilder.DropColumn(
                name: "MaterialBaseId",
                table: "Lamparas");
        }
    }
}
