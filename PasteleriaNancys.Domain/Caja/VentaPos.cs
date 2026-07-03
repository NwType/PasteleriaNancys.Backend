using System;

namespace PasteleriaNancys.Domain.Caja
{
    public class VentaPos
    {
        public Guid Id { get; set; }
        public Guid TurnoCajaId { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal TotalCobrado { get; set; }
        public string MetodoPago { get; set; } = string.Empty;

        // Propiedades de navegación
        public TurnoCaja TurnoCaja { get; set; } = null!;
    }
}
