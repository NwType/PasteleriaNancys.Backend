using System;
using System.Collections.Generic;

namespace PasteleriaNancys.Application.Caja.Dtos
{
    public class RegistrarVentaRequest
    {
        public Guid IdTurno { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public List<VentaDetalleRequest> Productos { get; set; } = new();
    }
}
