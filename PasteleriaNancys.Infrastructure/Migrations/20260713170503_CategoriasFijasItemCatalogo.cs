using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CategoriasFijasItemCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normaliza valores de Categoria existentes a las dos listas fijas antes de
            // aplicar el CHECK constraint, para que no falle sobre datos ya guardados.
            migrationBuilder.Sql(@"
                UPDATE [Inventario].[Item_Catalogo] SET [Categoria] = 'Tortas Clásicas'
                WHERE [Tipo] = 'Terminado' AND [Categoria] IN ('Tortas', 'Tortas de Vitrina', 'Torta');

                UPDATE [Inventario].[Item_Catalogo] SET [Categoria] = 'Tortas Personalizables'
                WHERE [Tipo] = 'Terminado' AND [Categoria] = 'Tortas Personalizadas';

                UPDATE [Inventario].[Item_Catalogo] SET [Categoria] = 'Lácteos y Cremas'
                WHERE [Tipo] = 'MateriaPrima' AND [Categoria] IN ('Cremas', 'Insumos Reposteria');

                UPDATE [Inventario].[Item_Catalogo] SET [Categoria] = 'Colorantes y Jaleas'
                WHERE [Tipo] = 'MateriaPrima' AND [Categoria] = 'Colorantes';

                UPDATE [Inventario].[Item_Catalogo] SET [Categoria] = 'Harinas y Secos'
                WHERE [Tipo] = 'MateriaPrima' AND [Categoria] IN ('Insumos Panaderia', 'Insumos');
            ");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Inventario_Item_Categoria",
                schema: "Inventario",
                table: "Item_Catalogo",
                sql: "([Tipo]='Terminado' AND [Categoria] IN ('Tortas Clásicas','Tortas Personalizables')) OR ([Tipo]='MateriaPrima' AND [Categoria] IN ('Harinas y Secos','Lácteos y Cremas','Colorantes y Jaleas','Rellenos','Empaques'))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Inventario_Item_Categoria",
                schema: "Inventario",
                table: "Item_Catalogo");
        }
    }
}
