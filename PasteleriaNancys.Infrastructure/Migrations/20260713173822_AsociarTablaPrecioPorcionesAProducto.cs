using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AsociarTablaPrecioPorcionesAProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tabla_Precio_Porciones_NumeroPorciones",
                schema: "Web",
                table: "Tabla_Precio_Porciones");

            migrationBuilder.AddColumn<Guid>(
                name: "IdItemTerminado",
                schema: "Web",
                table: "Tabla_Precio_Porciones",
                type: "uniqueidentifier",
                nullable: true);

            // Las 9 filas originales (sembradas por HasData en una migración anterior) quedaban
            // compartidas por todas las tortas personalizables. Ahora cada torta tiene su propia
            // tabla: las filas existentes pasan a ser las de "Torta Redonda a tu Gusto", y se
            // duplican para "Torta Cuadrada a tu Gusto" y "Torta en Forma de Corazón a tu Gusto"
            // con los mismos precios (mismo comportamiento de hoy, ahora independiente por producto).
            // No se usa HasData porque estos productos se crearon en vivo (no vía migración) y su
            // Id no existe todavía cuando se escribe esta migración — se resuelve por CodigoReferencia.
            // En una base de datos nueva creada solo con `dotnet ef database update`, estos productos
            // todavía no existen y estas filas quedan con IdItemTerminado NULL (columna nullable a
            // propósito) hasta que se carguen vía la API, igual que el resto del catálogo real.
            migrationBuilder.Sql(@"
                UPDATE [Web].[Tabla_Precio_Porciones]
                SET [IdItemTerminado] = (SELECT [IdItem] FROM [Inventario].[Item_Catalogo] WHERE [CodigoReferencia] = 'PT-TORTA-004');

                INSERT INTO [Web].[Tabla_Precio_Porciones] ([IdTablaPrecio], [IdItemTerminado], [NumeroPorciones], [Precio], [Descripcion], [Activo])
                SELECT NEWID(), (SELECT [IdItem] FROM [Inventario].[Item_Catalogo] WHERE [CodigoReferencia] = 'PT-TORTA-005'),
                       [NumeroPorciones], [Precio], [Descripcion], [Activo]
                FROM [Web].[Tabla_Precio_Porciones]
                WHERE [IdItemTerminado] = (SELECT [IdItem] FROM [Inventario].[Item_Catalogo] WHERE [CodigoReferencia] = 'PT-TORTA-004');

                INSERT INTO [Web].[Tabla_Precio_Porciones] ([IdTablaPrecio], [IdItemTerminado], [NumeroPorciones], [Precio], [Descripcion], [Activo])
                SELECT NEWID(), (SELECT [IdItem] FROM [Inventario].[Item_Catalogo] WHERE [CodigoReferencia] = 'PT-TORTA-007'),
                       [NumeroPorciones], [Precio], [Descripcion], [Activo]
                FROM [Web].[Tabla_Precio_Porciones]
                WHERE [IdItemTerminado] = (SELECT [IdItem] FROM [Inventario].[Item_Catalogo] WHERE [CodigoReferencia] = 'PT-TORTA-004');
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Tabla_Precio_Porciones_IdItemTerminado_NumeroPorciones",
                schema: "Web",
                table: "Tabla_Precio_Porciones",
                columns: new[] { "IdItemTerminado", "NumeroPorciones" },
                unique: true,
                filter: "[IdItemTerminado] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tabla_Precio_Porciones_IdItemTerminado_NumeroPorciones",
                schema: "Web",
                table: "Tabla_Precio_Porciones");

            // Vuelve a dejar una sola tabla global: borra las copias de Cuadrada/Corazón y
            // conserva solo las filas originales (las de Redonda).
            migrationBuilder.Sql(@"
                DELETE FROM [Web].[Tabla_Precio_Porciones]
                WHERE [IdItemTerminado] <> (SELECT [IdItem] FROM [Inventario].[Item_Catalogo] WHERE [CodigoReferencia] = 'PT-TORTA-004')
                   OR [IdItemTerminado] IS NULL;
            ");

            migrationBuilder.DropColumn(
                name: "IdItemTerminado",
                schema: "Web",
                table: "Tabla_Precio_Porciones");

            migrationBuilder.CreateIndex(
                name: "IX_Tabla_Precio_Porciones_NumeroPorciones",
                schema: "Web",
                table: "Tabla_Precio_Porciones",
                column: "NumeroPorciones",
                unique: true);
        }
    }
}
