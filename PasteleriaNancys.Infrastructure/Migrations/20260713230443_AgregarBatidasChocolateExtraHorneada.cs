using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarBatidasChocolateExtraHorneada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CantidadBatidasChocolateExtra",
                schema: "Inventario",
                table: "Horneada",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Inventario_Horneada_BatidasChocolateExtra",
                schema: "Inventario",
                table: "Horneada",
                sql: "[CantidadBatidasChocolateExtra] >= 0 AND [CantidadBatidasChocolateExtra] <= [CantidadBatidas] - 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Inventario_Horneada_BatidasChocolateExtra",
                schema: "Inventario",
                table: "Horneada");

            migrationBuilder.DropColumn(
                name: "CantidadBatidasChocolateExtra",
                schema: "Inventario",
                table: "Horneada");
        }
    }
}
