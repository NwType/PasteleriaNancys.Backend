using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuitarCantidadBatidasChocolate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Inventario_Horneada_CantidadBatidasChocolate",
                schema: "Inventario",
                table: "Horneada");

            migrationBuilder.DropColumn(
                name: "CantidadBatidasChocolate",
                schema: "Inventario",
                table: "Horneada");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CantidadBatidasChocolate",
                schema: "Inventario",
                table: "Horneada",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Inventario_Horneada_CantidadBatidasChocolate",
                schema: "Inventario",
                table: "Horneada",
                sql: "[CantidadBatidasChocolate] >= 0 AND [CantidadBatidasChocolate] <= [CantidadBatidas]");
        }
    }
}
