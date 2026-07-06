namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class EventoFestivoDto
    {
        public Guid Id { get; set; }
        public string NombreEvento { get; set; } = string.Empty;
        public DateTime FechaEvento { get; set; }
        public decimal MultiplicadorDemanda { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
