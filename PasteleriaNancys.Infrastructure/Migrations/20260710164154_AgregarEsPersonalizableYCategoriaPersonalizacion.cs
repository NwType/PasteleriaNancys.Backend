using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEsPersonalizableYCategoriaPersonalizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoriaPersonalizacion",
                schema: "Inventario",
                table: "Item_Catalogo",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsPersonalizable",
                schema: "Inventario",
                table: "Item_Catalogo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Inventario_Item_CategoriaPersonalizacion",
                schema: "Inventario",
                table: "Item_Catalogo",
                sql: "[CategoriaPersonalizacion] IS NULL OR [CategoriaPersonalizacion] IN ('Relleno','Crema','Colorante')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Inventario_Item_CategoriaPersonalizacion",
                schema: "Inventario",
                table: "Item_Catalogo");

            migrationBuilder.DropColumn(
                name: "CategoriaPersonalizacion",
                schema: "Inventario",
                table: "Item_Catalogo");

            migrationBuilder.DropColumn(
                name: "EsPersonalizable",
                schema: "Inventario",
                table: "Item_Catalogo");
        }
    }
}
