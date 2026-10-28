using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlmaDelMarPampeano.Migrations
{
    /// <inheritdoc />
    public partial class AuditoriaDetallada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Entidad",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntidadId",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoAccion",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioEmail",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Entidad",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "EntidadId",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "TipoAccion",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "UsuarioEmail",
                table: "Auditorias");
        }
    }
}
