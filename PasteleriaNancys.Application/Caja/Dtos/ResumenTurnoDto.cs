using System;

namespace PasteleriaNancys.Application.Caja.Dtos
{
    public class ResumenTurnoDto
    {
        public Guid IdTurno { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal TotalIngresosSistema { get; set; }
        public decimal TotalGastosExtras { get; set; }
        public decimal TotalEsperado { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
