using System;

namespace PasteleriaNancys.Domain.Pedidos
{
    public class PedidoConfiguracion
    {
        public Guid Id { get; set; }
        public Guid IdPedido { get; set; }
        public Guid? IdItemProductoBase { get; set; }
        public Guid? IdInsumoSaborMasa { get; set; }
        public Guid? IdInsumoRelleno { get; set; }
        public string SaborMasa { get; set; } = string.Empty;
        public string TipoRelleno { get; set; } = string.Empty;
        public string? TamanoRacion { get; set; }
        public string? ColorDecoracion { get; set; }
        public string? DedicatoriaDetalle { get; set; }
        public string? ImagenReferenciaUrl { get; set; }
        public decimal? PorcentajeAnticipo { get; set; }

        // Personalización de torta (masa/crema/relleno/color por porciones)
        public int? NumeroPorciones { get; set; }
        public string? TipoMasa { get; set; }
        public string? TipoCrema { get; set; }
        public Guid? IdInsumoCrema { get; set; }
        public Guid? IdInsumoColorDecoracion { get; set; }
        public int CantidadVelasNormales { get; set; }
        public string? VelaNumerica { get; set; }

        // Propiedad de navegación
        public PedidoCliente Pedido { get; set; } = null!;
    }
}
