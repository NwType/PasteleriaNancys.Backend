using PasteleriaNancys.Application.Caja.Dtos;

namespace PasteleriaNancys.Application.Administracion.Dtos
{
    public class ArqueoAuditoriaDetalleDto
    {
        public ArqueoAuditoriaDto Arqueo { get; set; } = null!;
        public List<VentaDto> Ventas { get; set; } = new();
        public List<GastoDto> Gastos { get; set; } = new();

        /// <summary>
        /// Recalculado desde las filas reales de Venta_POS/Gasto_Extra (SUM), para detectar si el
        /// total acumulado guardado en Turno se desincronizó (ej. por una condición de carrera).
        /// </summary>
        public decimal TotalIngresosCalculado { get; set; }
        public decimal TotalGastosCalculado { get; set; }
        public bool DriftDetectado { get; set; }
    }
}
