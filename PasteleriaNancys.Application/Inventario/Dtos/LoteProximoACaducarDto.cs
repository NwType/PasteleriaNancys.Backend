namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class LoteProximoACaducarDto
    {
        public Guid IdLote { get; set; }
        public Guid IdItem { get; set; }
        public string CodigoReferencia { get; set; } = string.Empty;
        public string NombreItem { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public decimal CantidadDisponible { get; set; }
        public DateTime FechaCaducidad { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int DiasParaCaducar { get; set; }
        public string NivelAlerta { get; set; } = string.Empty;
    }
}
