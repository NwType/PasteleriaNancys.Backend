using System;
using System.Collections.Generic;

namespace PasteleriaNancys.Domain.Inventario
{
    public class ViajeDespacho
    {
        public Guid Id { get; set; }
        public Guid? IdUsuarioConductor { get; set; }
        public string Conductor { get; set; } = string.Empty;
        public DateTime FechaDespacho { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Observaciones { get; set; }

        // Propiedades de navegación
        public ICollection<ViajeDetalle> Detalles { get; set; } = new List<ViajeDetalle>();
    }
}
