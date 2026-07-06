using System;

namespace PasteleriaNancys.Domain.Inventario
{
    public class EventoFestivo
    {
        public Guid Id { get; set; }
        public string NombreEvento { get; set; } = string.Empty;
        public DateTime FechaEvento { get; set; }
        public decimal MultiplicadorDemanda { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
