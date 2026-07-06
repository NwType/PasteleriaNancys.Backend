using System;

namespace PasteleriaNancys.Application.Caja.Dtos
{
    public class CierreTurnoDto
    {
        public Guid IdTurno { get; set; }
        public decimal TotalEsperado { get; set; }
        public decimal MontoFisicoContado { get; set; }
        public decimal DiferenciaArqueo { get; set; }
        public bool DiferenciaSignificativa { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCierre { get; set; }
    }
}
