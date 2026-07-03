using System;

namespace PasteleriaNancys.Domain.Inventario
{
    public class LotePeps
    {
        public Guid Id { get; set; }
        public Guid CatalogoItemId { get; set; }
        public string Ubicacion { get; set; } = string.Empty;
        public decimal CantidadInicial { get; set; }
        public decimal CantidadDisponible { get; set; }
        public DateTime FechaElaboracion { get; set; }
        public DateTime FechaCaducidad { get; set; }
        public string Estado { get; set; } = string.Empty;

        // Propiedades de navegación
        public CatalogoItem CatalogoItem { get; set; } = null!;
    }
}
