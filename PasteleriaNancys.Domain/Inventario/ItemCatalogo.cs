using System;
using System.Collections.Generic;

namespace PasteleriaNancys.Domain.Inventario
{
    public class ItemCatalogo
    {
        public Guid Id { get; set; }
        public string CodigoReferencia { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public bool Activo { get; set; }

        // Propiedades de navegación
        public ICollection<LotePeps> LotesPeps { get; set; } = new List<LotePeps>();
    }
}
