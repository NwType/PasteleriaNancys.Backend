namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class CrearEventoFestivoRequest
    {
        public string NombreEvento { get; set; } = string.Empty;
        public DateTime FechaEvento { get; set; }
        public decimal MultiplicadorDemanda { get; set; }
    }
}
