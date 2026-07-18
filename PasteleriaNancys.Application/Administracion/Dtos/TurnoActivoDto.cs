namespace PasteleriaNancys.Application.Administracion.Dtos
{
    public class TurnoActivoDto
    {
        public Guid IdTurno { get; set; }
        public Guid IdUsuarioResponsable { get; set; }
        public string NombreResponsable { get; set; } = string.Empty;
        public DateTime FechaApertura { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal TotalIngresosSistema { get; set; }
        public decimal TotalGastosExtras { get; set; }
        public decimal TotalEsperadoParcial { get; set; }
    }
}
