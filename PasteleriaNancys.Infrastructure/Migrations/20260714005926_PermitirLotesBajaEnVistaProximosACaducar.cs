using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PermitirLotesBajaEnVistaProximosACaducar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La vista excluía Estado='Baja' — con el auto-marcado de lotes vencidos como "Baja"
            // (LoteService.SincronizarEstadosVencidosAsync, 2026-07-14), eso hacía que un lote
            // recién vencido desapareciera de "próximos a caducar" justo cuando más importaba
            // avisar que se dio de baja solo. El campo OrdenConsumo de esta vista no se usa en
            // ningún lado del código (la orden real de consumo PEPS la calcula
            // ObtenerDisponiblesParaVentaAsync aparte), así que quitar el filtro es seguro.
            migrationBuilder.Sql(@"
ALTER VIEW [Inventario].[vw_Lotes_PEPS_Ordenados]
AS
SELECT
    l.IdLote,
    l.IdItem,
    i.CodigoReferencia,
    i.Nombre            AS NombreItem,
    i.Tipo,
    i.UnidadMedida,
    l.Ubicacion,
    l.CantidadInicial,
    l.CantidadDisponible,
    l.FechaElaboracion,
    l.FechaCaducidad,
    l.Estado,
    DATEDIFF(DAY, SYSUTCDATETIME(), l.FechaCaducidad) AS DiasParaCaducar,
    ROW_NUMBER() OVER (
        PARTITION BY l.IdItem
        ORDER BY l.FechaElaboracion ASC
    ) AS OrdenConsumo
FROM [Inventario].[Lote_PEPS]      AS l
JOIN [Inventario].[Item_Catalogo]  AS i ON i.IdItem = l.IdItem
WHERE l.CantidadDisponible > 0;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER VIEW [Inventario].[vw_Lotes_PEPS_Ordenados]
AS
SELECT
    l.IdLote,
    l.IdItem,
    i.CodigoReferencia,
    i.Nombre            AS NombreItem,
    i.Tipo,
    i.UnidadMedida,
    l.Ubicacion,
    l.CantidadInicial,
    l.CantidadDisponible,
    l.FechaElaboracion,
    l.FechaCaducidad,
    l.Estado,
    DATEDIFF(DAY, SYSUTCDATETIME(), l.FechaCaducidad) AS DiasParaCaducar,
    ROW_NUMBER() OVER (
        PARTITION BY l.IdItem
        ORDER BY l.FechaElaboracion ASC
    ) AS OrdenConsumo
FROM [Inventario].[Lote_PEPS]      AS l
JOIN [Inventario].[Item_Catalogo]  AS i ON i.IdItem = l.IdItem
WHERE l.CantidadDisponible > 0
  AND l.Estado <> 'Baja';
");
        }
    }
}
