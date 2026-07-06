using System;

namespace PasteleriaNancys.Domain.Inventario
{
    public class StockMinimo
    {
        public Guid Id { get; set; }
        public Guid IdItem { get; set; }
        public decimal CantidadMinima { get; set; }
        public bool Activo { get; set; }

        // Propiedades de navegación
        public ItemCatalogo Item { get; set; } = null!;
    }
}
