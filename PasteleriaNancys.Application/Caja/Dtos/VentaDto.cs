using System;
using System.Collections.Generic;

namespace PasteleriaNancys.Application.Caja.Dtos
{
    public class VentaDto
    {
        public Guid Id { get; set; }
        public Guid IdTurno { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal TotalPagado { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public bool Anulada { get; set; }
        public string? MotivoAnulacion { get; set; }
        public List<VentaDetalleDto> Detalles { get; set; } = new();
    }
}
