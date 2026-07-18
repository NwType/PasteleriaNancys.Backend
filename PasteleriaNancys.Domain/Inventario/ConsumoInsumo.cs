using System;

namespace PasteleriaNancys.Domain.Inventario
{
    /// <summary>
    /// Ledger de descuentos de materia prima que no vienen de una venta: consumo fijo por
    /// horneada (harina/azúcar/maicena/polvo/caramelina) o consumo manual parametrizado por
    /// el Encargado de Almacén (cremas, jaleas, y demás insumos de cantidad fluctuante).
    /// </summary>
    public class ConsumoInsumo
    {
        public Guid Id { get; set; }
        public Guid? IdHorneada { get; set; }
        public Guid IdItem { get; set; }
        public Guid IdLote { get; set; }
        public decimal CantidadDescontada { get; set; }
        public string? Motivo { get; set; }
        public DateTime Fecha { get; set; }
        public Guid IdUsuarioRegistro { get; set; }

        public Horneada? Horneada { get; set; }
        public ItemCatalogo Item { get; set; } = null!;
        public LotePeps Lote { get; set; } = null!;
    }
}
