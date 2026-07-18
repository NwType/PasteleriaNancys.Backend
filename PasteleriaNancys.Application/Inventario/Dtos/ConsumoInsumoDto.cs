namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class ConsumoInsumoDto
    {
        public Guid Id { get; set; }
        public Guid? IdHorneada { get; set; }
        public Guid IdItem { get; set; }
        public string NombreItem { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal CantidadDescontada { get; set; }
        public string? Motivo { get; set; }
        public DateTime Fecha { get; set; }
    }
}
