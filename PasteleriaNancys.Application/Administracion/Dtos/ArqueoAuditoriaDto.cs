namespace PasteleriaNancys.Application.Administracion.Dtos
{
    public class ArqueoAuditoriaDto
    {
        public Guid IdTurno { get; set; }
        public Guid IdUsuarioResponsable { get; set; }
        public string NombreResponsable { get; set; } = string.Empty;
        public string CorreoResponsable { get; set; } = string.Empty;
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal TotalIngresosSistema { get; set; }
        public decimal TotalGastosExtras { get; set; }
        public decimal TotalEsperado { get; set; }
        public decimal? MontoFisicoContado { get; set; }
        public decimal? DiferenciaArqueo { get; set; }
        public bool DiferenciaSignificativa { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
