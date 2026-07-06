using System;

namespace PasteleriaNancys.Application.Caja.Dtos
{
    public class TurnoDto
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
    }
}
