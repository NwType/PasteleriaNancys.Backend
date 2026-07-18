namespace PasteleriaNancys.Application.Reportes.Dtos
{
    public class ProyeccionHorneadoDto
    {
        public List<ProyeccionProductoDto> Productos { get; set; } = new();
        public List<InsumoRequeridoDto> InsumosNecesarios { get; set; } = new();
        public RecomendacionSaboresDto RecomendacionSabores { get; set; } = new();
        public List<ProductoMasVendidoHoyDto> MasVendidosHoy { get; set; } = new();
    }
}
