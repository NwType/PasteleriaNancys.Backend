using System;
using System.Collections.Generic;

namespace PasteleriaNancys.Domain.Caja
{
    public class TurnoCaja
    {
        public Guid Id { get; set; }
        public Guid UsuarioAperturaId { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal DiferenciaArqueo { get; set; }
        public string Estado { get; set; } = string.Empty;

        // Propiedades de navegación
        public ICollection<VentaPos> VentasPos { get; set; } = new List<VentaPos>();
    }
}
