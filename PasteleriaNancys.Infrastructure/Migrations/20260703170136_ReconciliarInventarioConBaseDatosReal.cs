using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReconciliarInventarioConBaseDatosReal : Migration
    {
        // Migración recortada a mano: el modelo de EF se corrigió para apuntar a las tablas
        // reales (Item_Catalogo, Lote_PEPS, Proveedor, Evento_Festivo, Viaje_Despacho,
        // Viaje_Detalle), que ya existen en la base de datos con datos reales y coinciden
        // columna por columna con el nuevo modelo. La migración generada automáticamente
        // intentaba renombrar/recrear esas tablas (porque el snapshot de EF no sabía que ya
        // existían); se eliminó todo eso a mano, dejando solo las dos tablas genuinamente
        // nuevas: Stock_Minimo y Receta_Item. Mismo criterio que la baseline InitialCreate.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Receta_Item",
                schema: "Inventario",
                columns: table => new
                {
                    IdReceta = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdItemTerminado = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdItemInsumo = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CantidadRequerida = table.Column<decimal>(type: "decimal(8,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receta_Item", x => x.IdReceta);
                    table.ForeignKey(
                        name: "FK_Receta_Item_Item_Catalogo_IdItemInsumo",
                        column: x => x.IdItemInsumo,
                        principalSchema: "Inventario",
                        principalTable: "Item_Catalogo",
                        principalColumn: "IdItem",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receta_Item_Item_Catalogo_IdItemTerminado",
                        column: x => x.IdItemTerminado,
                        principalSchema: "Inventario",
                        principalTable: "Item_Catalogo",
                        principalColumn: "IdItem",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stock_Minimo",
                schema: "Inventario",
                columns: table => new
                {
                    IdStockMinimo = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdItem = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CantidadMinima = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stock_Minimo", x => x.IdStockMinimo);
                    table.ForeignKey(
                        name: "FK_Stock_Minimo_Item_Catalogo_IdItem",
                        column: x => x.IdItem,
                        principalSchema: "Inventario",
                        principalTable: "Item_Catalogo",
                        principalColumn: "IdItem",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Receta_Item_IdItemInsumo",
                schema: "Inventario",
                table: "Receta_Item",
                column: "IdItemInsumo");

            migrationBuilder.CreateIndex(
                name: "IX_Receta_Item_IdItemTerminado_IdItemInsumo",
                schema: "Inventario",
                table: "Receta_Item",
                columns: new[] { "IdItemTerminado", "IdItemInsumo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stock_Minimo_IdItem",
                schema: "Inventario",
                table: "Stock_Minimo",
                column: "IdItem",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Receta_Item",
                schema: "Inventario");

            migrationBuilder.DropTable(
                name: "Stock_Minimo",
                schema: "Inventario");
        }
    }
}
