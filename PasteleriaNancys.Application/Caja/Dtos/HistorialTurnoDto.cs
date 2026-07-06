using System;
using System.Collections.Generic;

namespace PasteleriaNancys.Application.Caja.Dtos
{
    public class HistorialTurnoDto
    {
        public Guid IdTurno { get; set; }
        public List<VentaDto> Ventas { get; set; } = new();
        public List<GastoDto> Gastos { get; set; } = new();
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
    }
}
