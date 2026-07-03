using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarBloqueoCuentaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Bloqueado",
                schema: "Seguridad",
                table: "Usuario",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IntentosFallidos",
                schema: "Seguridad",
                table: "Usuario",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bloqueado",
                schema: "Seguridad",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "IntentosFallidos",
                schema: "Seguridad",
                table: "Usuario");
        }
    }
}
