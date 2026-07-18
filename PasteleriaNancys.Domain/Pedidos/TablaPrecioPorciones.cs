using System;

namespace PasteleriaNancys.Domain.Pedidos
{
    public class TablaPrecioPorciones
    {
        public Guid Id { get; set; }
        public Guid? IdItemTerminado { get; set; }
        public int NumeroPorciones { get; set; }
        public decimal Precio { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
    }
}
