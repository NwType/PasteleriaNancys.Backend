using System;
using System.Collections.Generic;

namespace PasteleriaNancys.Domain.Caja
{
    public class Turno
    {
        public Guid Id { get; set; }
        public Guid IdUsuarioResponsable { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal TotalIngresosSistema { get; set; }
        public decimal TotalGastosExtras { get; set; }
        public decimal? DiferenciaArqueo { get; set; }
        public string Estado { get; set; } = string.Empty;

        // Propiedades de navegación
        public ICollection<VentaPos> VentasPos { get; set; } = new List<VentaPos>();
        public ICollection<GastoExtra> GastosExtra { get; set; } = new List<GastoExtra>();
    }
}
