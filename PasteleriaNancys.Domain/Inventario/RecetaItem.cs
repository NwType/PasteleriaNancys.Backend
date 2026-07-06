using System;

namespace PasteleriaNancys.Domain.Inventario
{
    public class RecetaItem
    {
        public Guid Id { get; set; }
        public Guid IdItemTerminado { get; set; }
        public Guid IdItemInsumo { get; set; }
        public decimal CantidadRequerida { get; set; }

        // Propiedades de navegación
        public ItemCatalogo ItemTerminado { get; set; } = null!;
        public ItemCatalogo ItemInsumo { get; set; } = null!;
    }
}
