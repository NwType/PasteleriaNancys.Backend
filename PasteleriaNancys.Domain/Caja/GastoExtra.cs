using System;

namespace PasteleriaNancys.Domain.Caja
{
    public class GastoExtra
    {
        public Guid Id { get; set; }
        public Guid IdTurno { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime FechaHora { get; set; }

        // Propiedades de navegación
        public Turno Turno { get; set; } = null!;
    }
}
