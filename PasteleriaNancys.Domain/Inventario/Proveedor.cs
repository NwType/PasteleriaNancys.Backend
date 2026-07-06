using System;
using System.Collections.Generic;

namespace PasteleriaNancys.Domain.Inventario
{
    public class Proveedor
    {
        public Guid Id { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public string? Contacto { get; set; }
        public string? Telefono { get; set; }
        public bool Activo { get; set; }

        // Propiedades de navegación
        public ICollection<LotePeps> LotesPeps { get; set; } = new List<LotePeps>();
    }
}
