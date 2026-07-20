using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIntermediosMermasYRecetaAnidada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Inventario_Item_Categoria",
                schema: "Inventario",
                table: "Item_Catalogo");

            migrationBuilder.AlterColumn<decimal>(
                name: "CantidadRequerida",
                schema: "Inventario",
                table: "Receta_Item",
                type: "decimal(12,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)");

            migrationBuilder.AddColumn<Guid>(
                name: "IdLoteProducido",
                schema: "Inventario",
                table: "Consumo_Insumo",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Merma",
                schema: "Inventario",
                columns: table => new
                {
                    IdMerma = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdItem = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdLote = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    TipoMerma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IdUsuarioRegistro = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merma", x => x.IdMerma);
                    table.CheckConstraint("CK_Inventario_Merma_Cantidad", "[Cantidad] > 0");
                    table.CheckConstraint("CK_Inventario_Merma_Tipo", "[TipoMerma] IN ('Insumo dañado','Producción fallida','Caducidad','Accidente','Otro')");
                    table.ForeignKey(
                        name: "FK_Merma_Item_Catalogo_IdItem",
                        column: x => x.IdItem,
                        principalSchema: "Inventario",
                        principalTable: "Item_Catalogo",
                        principalColumn: "IdItem",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Merma_Lote_PEPS_IdLote",
                        column: x => x.IdLote,
                        principalSchema: "Inventario",
                        principalTable: "Lote_PEPS",
                        principalColumn: "IdLote",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Inventario_Item_Categoria",
                schema: "Inventario",
                table: "Item_Catalogo",
                sql: "([Tipo]='Terminado' AND [Categoria] IN ('Tortas Clásicas','Tortas Personalizables')) OR ([Tipo]='MateriaPrima' AND [Categoria] IN ('Harinas y Secos','Lácteos y Cremas','Colorantes y Jaleas','Rellenos','Empaques')) OR ([Tipo]='Intermedio' AND [Categoria] IN ('Bizcochos','Preparados'))");

            migrationBuilder.CreateIndex(
                name: "IX_Consumo_Insumo_IdLoteProducido",
                schema: "Inventario",
                table: "Consumo_Insumo",
                column: "IdLoteProducido");

            migrationBuilder.CreateIndex(
                name: "IX_Merma_IdItem",
                schema: "Inventario",
                table: "Merma",
                column: "IdItem");

            migrationBuilder.CreateIndex(
                name: "IX_Merma_IdLote",
                schema: "Inventario",
                table: "Merma",
                column: "IdLote");

            migrationBuilder.AddForeignKey(
                name: "FK_Consumo_Insumo_Lote_PEPS_IdLoteProducido",
                schema: "Inventario",
                table: "Consumo_Insumo",
                column: "IdLoteProducido",
                principalSchema: "Inventario",
                principalTable: "Lote_PEPS",
                principalColumn: "IdLote",
                onDelete: ReferentialAction.Restrict);

            // CK_Inventario_Item_Tipo vive solo en la BD (viene del script SQL original, no del
            // modelo EF) — se recrea a mano para aceptar el nuevo tipo 'Intermedio'.
            migrationBuilder.Sql(@"
ALTER TABLE [Inventario].[Item_Catalogo] DROP CONSTRAINT [CK_Inventario_Item_Tipo];
ALTER TABLE [Inventario].[Item_Catalogo] ADD CONSTRAINT [CK_Inventario_Item_Tipo]
    CHECK ([Tipo] IN ('MateriaPrima','Terminado','Intermedio'));
");

            // ============================================================================
            // Datos (2026-07-17, decisiones confirmadas con el usuario):
            //  1. Bizcochos como ítems Intermedios, medidos en PORCIONES (bizcocho estándar
            //     = 10 porciones; batida = 200 porciones). Su Receta_Item guarda la fórmula
            //     real de la batida ÷ 200 — incluye los 25 huevos fijos por batida (0.125
            //     huevo/porción) y, en el de chocolate, la caramelina (0.0005 kg/porción,
            //     que reproduce exactamente los 0.050 kg de la mixta estándar y los 0.100 kg
            //     por batida 100% chocolate).
            //  2. Las recetas de las tortas estándar dejan de repetir harina/azúcar/maicena/
            //     polvo/huevo (y caramelina en las de chocolate): esas líneas se reemplazan
            //     por una sola línea de bizcocho = NumeroPorciones porciones. El Brazo Gitano
            //     NO se toca — su base (pionono) es distinta de la batida estándar.
            //  3. "Torta de chipas" era un typo — pasa a "Torta de Chispas de Chocolate" y
            //     recibe receta derivada de una receta pública de internet adaptada al modelo
            //     de producción real (bizcocho vainilla + ~150 g de chispas por cada 10
            //     porciones + crema de armado estándar). Se crea el insumo Chispas de
            //     Chocolate (MP-CHIS-001).
            // ============================================================================
            migrationBuilder.Sql(@"
-- 1. Ítems intermedios (bizcochos)
IF NOT EXISTS (SELECT 1 FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia = 'PI-BIZC-001')
    INSERT INTO [Inventario].[Item_Catalogo]
        (IdItem, CodigoReferencia, Nombre, Categoria, Tipo, UnidadMedida, PrecioUnitario, EsPersonalizable, TipoMasa, Activo)
    VALUES (NEWID(), 'PI-BIZC-001', N'Bizcocho de Vainilla', N'Bizcochos', 'Intermedio', N'porción', 0, 0, 'Vainilla', 1);

IF NOT EXISTS (SELECT 1 FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia = 'PI-BIZC-002')
    INSERT INTO [Inventario].[Item_Catalogo]
        (IdItem, CodigoReferencia, Nombre, Categoria, Tipo, UnidadMedida, PrecioUnitario, EsPersonalizable, TipoMasa, Activo)
    VALUES (NEWID(), 'PI-BIZC-002', N'Bizcocho de Chocolate', N'Bizcochos', 'Intermedio', N'porción', 0, 0, 'Chocolate', 1);

DECLARE @bizVainilla  UNIQUEIDENTIFIER = (SELECT IdItem FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia = 'PI-BIZC-001');
DECLARE @bizChocolate UNIQUEIDENTIFIER = (SELECT IdItem FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia = 'PI-BIZC-002');
DECLARE @harina    UNIQUEIDENTIFIER = (SELECT IdItem FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia = 'MP-HARI-001');
DECLARE @azucar    UNIQUEIDENTIFIER = (SELECT IdItem FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia = 'MP-HARI-004');
DECLARE @maicena   UNIQUEIDENTIFIER = (SELECT IdItem FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia = 'MP-HARI-003');
DECLARE @polvo     UNIQUEIDENTIFIER = (SELECT IdItem FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia = 'MP-HARI-005');
DECLARE @huevo     UNIQUEIDENTIFIER = (SELECT IdItem FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia = 'MP-HUEV-001');
DECLARE @caramelina UNIQUEIDENTIFIER = (SELECT IdItem FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia = 'MP-CARA-001');

-- 2. Receta del bizcocho POR PORCIÓN (fórmula real de la batida ÷ 200 porciones)
INSERT INTO [Inventario].[Receta_Item] (IdReceta, IdItemTerminado, IdItemInsumo, CantidadRequerida)
SELECT NEWID(), b.Id, i.Id, i.Cantidad
FROM (VALUES (@bizVainilla), (@bizChocolate)) AS b(Id)
CROSS JOIN (VALUES
    (@harina,  CAST(0.008820 AS decimal(12,6))),  -- 1.764 kg/batida
    (@azucar,  0.006860),                          -- 1.372 kg/batida
    (@maicena, 0.003875),                          -- 0.775 kg/batida
    (@polvo,   0.000375),                          -- 0.075 kg/batida
    (@huevo,   0.125000)                           -- 25 huevos fijos/batida
) AS i(Id, Cantidad)
WHERE NOT EXISTS (SELECT 1 FROM [Inventario].[Receta_Item] r WHERE r.IdItemTerminado = b.Id AND r.IdItemInsumo = i.Id);

-- Caramelina solo en el bizcocho de chocolate
INSERT INTO [Inventario].[Receta_Item] (IdReceta, IdItemTerminado, IdItemInsumo, CantidadRequerida)
SELECT NEWID(), @bizChocolate, @caramelina, 0.000500
WHERE NOT EXISTS (SELECT 1 FROM [Inventario].[Receta_Item] r WHERE r.IdItemTerminado = @bizChocolate AND r.IdItemInsumo = @caramelina);

-- 3. Recablear las recetas de las tortas estándar: fuera los insumos de la base…
DELETE r FROM [Inventario].[Receta_Item] r
JOIN [Inventario].[Item_Catalogo] t ON t.IdItem = r.IdItemTerminado
JOIN [Inventario].[Item_Catalogo] i ON i.IdItem = r.IdItemInsumo
WHERE t.CodigoReferencia IN ('PT-TORTA-002','PT-TORTA-003','PT-TORTA-006','PT-TORTA-008','PT-TORTA-011','PT-TORTA-013')
  AND i.CodigoReferencia IN ('MP-HARI-001','MP-HARI-003','MP-HARI-004','MP-HARI-005','MP-HUEV-001');

DELETE r FROM [Inventario].[Receta_Item] r
JOIN [Inventario].[Item_Catalogo] t ON t.IdItem = r.IdItemTerminado
JOIN [Inventario].[Item_Catalogo] i ON i.IdItem = r.IdItemInsumo
WHERE t.CodigoReferencia IN ('PT-TORTA-002','PT-TORTA-003')
  AND i.CodigoReferencia = 'MP-CARA-001';

-- …y adentro una sola línea de bizcocho = NumeroPorciones porciones (torta de N = N/2 bizcochos)
INSERT INTO [Inventario].[Receta_Item] (IdReceta, IdItemTerminado, IdItemInsumo, CantidadRequerida)
SELECT NEWID(), t.IdItem, @bizChocolate, t.NumeroPorciones
FROM [Inventario].[Item_Catalogo] t
WHERE t.CodigoReferencia IN ('PT-TORTA-002','PT-TORTA-003')
  AND NOT EXISTS (SELECT 1 FROM [Inventario].[Receta_Item] r WHERE r.IdItemTerminado = t.IdItem AND r.IdItemInsumo = @bizChocolate);

INSERT INTO [Inventario].[Receta_Item] (IdReceta, IdItemTerminado, IdItemInsumo, CantidadRequerida)
SELECT NEWID(), t.IdItem, @bizVainilla, t.NumeroPorciones
FROM [Inventario].[Item_Catalogo] t
WHERE t.CodigoReferencia IN ('PT-TORTA-006','PT-TORTA-008','PT-TORTA-011','PT-TORTA-013')
  AND NOT EXISTS (SELECT 1 FROM [Inventario].[Receta_Item] r WHERE r.IdItemTerminado = t.IdItem AND r.IdItemInsumo = @bizVainilla);

-- TipoMasa faltante en tortas de vitrina (alimenta la Proyección por sabor)
UPDATE [Inventario].[Item_Catalogo] SET TipoMasa = 'Chocolate'
WHERE CodigoReferencia = 'PT-TORTA-003' AND TipoMasa IS NULL;
UPDATE [Inventario].[Item_Catalogo] SET TipoMasa = 'Vainilla'
WHERE CodigoReferencia IN ('PT-TORTA-008','PT-TORTA-011') AND TipoMasa IS NULL;

-- 4. Torta de Chispas de Chocolate (era 'Torta de chipas', typo) + insumo nuevo
IF NOT EXISTS (SELECT 1 FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia = 'MP-CHIS-001')
    INSERT INTO [Inventario].[Item_Catalogo]
        (IdItem, CodigoReferencia, Nombre, Categoria, Tipo, UnidadMedida, PrecioUnitario, EsPersonalizable, Activo)
    VALUES (NEWID(), 'MP-CHIS-001', N'Chispas de Chocolate', N'Harinas y Secos', 'MateriaPrima', N'kg', 0, 0, 1);

UPDATE [Inventario].[Item_Catalogo] SET Nombre = N'Torta de Chispas de Chocolate'
WHERE CodigoReferencia = 'PT-TORTA-012' AND Nombre = N'Torta de chipas';

DECLARE @chispas UNIQUEIDENTIFIER = (SELECT IdItem FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia = 'MP-CHIS-001');
DECLARE @cremaMascrean UNIQUEIDENTIFIER = (SELECT IdItem FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia = 'MP-CREM-002');
DECLARE @tortaChispas UNIQUEIDENTIFIER = (SELECT IdItem FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia = 'PT-TORTA-012');
DECLARE @porcionesChispas DECIMAL(12,6) = (SELECT NumeroPorciones FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia = 'PT-TORTA-012');

INSERT INTO [Inventario].[Receta_Item] (IdReceta, IdItemTerminado, IdItemInsumo, CantidadRequerida)
SELECT NEWID(), @tortaChispas, i.Id, i.Cantidad
FROM (VALUES
    (@bizVainilla,   @porcionesChispas),                     -- bizcocho vainilla, 20 porciones
    (@chispas,       CAST(0.300000 AS decimal(12,6))),       -- ~150 g de chispas por cada 10 porciones
    (@cremaMascrean, 0.350000)                               -- crema de armado estándar (0.30-0.45 kg)
) AS i(Id, Cantidad)
WHERE i.Id IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [Inventario].[Receta_Item] r WHERE r.IdItemTerminado = @tortaChispas AND r.IdItemInsumo = i.Id);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reversión best-effort de los datos: elimina bizcochos/chispas y sus recetas.
            // Las líneas originales de harina/azúcar/etc. de las tortas NO se restauran (la
            // información equivalente queda en la receta del bizcocho); restaurar a mano desde
            // un backup si de verdad se necesita volver al modelo plano.
            migrationBuilder.Sql(@"
DELETE r FROM [Inventario].[Receta_Item] r
JOIN [Inventario].[Item_Catalogo] i ON i.IdItem = r.IdItemInsumo
WHERE i.CodigoReferencia IN ('PI-BIZC-001','PI-BIZC-002','MP-CHIS-001');
DELETE r FROM [Inventario].[Receta_Item] r
JOIN [Inventario].[Item_Catalogo] t ON t.IdItem = r.IdItemTerminado
WHERE t.CodigoReferencia IN ('PI-BIZC-001','PI-BIZC-002');
DELETE FROM [Inventario].[Item_Catalogo] WHERE CodigoReferencia IN ('PI-BIZC-001','PI-BIZC-002','MP-CHIS-001');
ALTER TABLE [Inventario].[Item_Catalogo] DROP CONSTRAINT [CK_Inventario_Item_Tipo];
ALTER TABLE [Inventario].[Item_Catalogo] ADD CONSTRAINT [CK_Inventario_Item_Tipo]
    CHECK ([Tipo] IN ('MateriaPrima','Terminado'));
");

            migrationBuilder.DropForeignKey(
                name: "FK_Consumo_Insumo_Lote_PEPS_IdLoteProducido",
                schema: "Inventario",
                table: "Consumo_Insumo");

            migrationBuilder.DropTable(
                name: "Merma",
                schema: "Inventario");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Inventario_Item_Categoria",
                schema: "Inventario",
                table: "Item_Catalogo");

            migrationBuilder.DropIndex(
                name: "IX_Consumo_Insumo_IdLoteProducido",
                schema: "Inventario",
                table: "Consumo_Insumo");

            migrationBuilder.DropColumn(
                name: "IdLoteProducido",
                schema: "Inventario",
                table: "Consumo_Insumo");

            migrationBuilder.AlterColumn<decimal>(
                name: "CantidadRequerida",
                schema: "Inventario",
                table: "Receta_Item",
                type: "decimal(8,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,6)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Inventario_Item_Categoria",
                schema: "Inventario",
                table: "Item_Catalogo",
                sql: "([Tipo]='Terminado' AND [Categoria] IN ('Tortas Clásicas','Tortas Personalizables')) OR ([Tipo]='MateriaPrima' AND [Categoria] IN ('Harinas y Secos','Lácteos y Cremas','Colorantes y Jaleas','Rellenos','Empaques'))");
        }
    }
}
