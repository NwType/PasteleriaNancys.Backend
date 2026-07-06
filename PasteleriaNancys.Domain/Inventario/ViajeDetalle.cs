using System;

namespace PasteleriaNancys.Domain.Inventario
{
    public class ViajeDetalle
    {
        public Guid Id { get; set; }
        public Guid IdViaje { get; set; }
        public Guid IdLote { get; set; }
        public decimal CantidadEnviada { get; set; }

        // Propiedades de navegación
        public ViajeDespacho Viaje { get; set; } = null!;
        public LotePeps Lote { get; set; } = null!;
    }
}
