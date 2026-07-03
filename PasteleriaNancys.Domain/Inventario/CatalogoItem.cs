using System;
using System.Collections.Generic;

namespace PasteleriaNancys.Domain.Inventario
{
    public class CatalogoItem
    {
        public Guid Id { get; set; }
        public string CodigoReferencia { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string TipoItem { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;

        // Propiedades de navegación
        public ICollection<LotePeps> LotesPeps { get; set; } = new List<LotePeps>();
    }
}
