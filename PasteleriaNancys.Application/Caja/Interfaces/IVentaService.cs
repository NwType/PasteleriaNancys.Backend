using PasteleriaNancys.Application.Caja.Dtos;

namespace PasteleriaNancys.Application.Caja.Interfaces
{
    public interface IVentaService
    {
        Task<VentaDto> ObtenerPorIdAsync(Guid id);
        Task<VentaDto> RegistrarAsync(RegistrarVentaRequest request);
        Task<VentaDto> AnularAsync(Guid idVenta, AnularVentaRequest request);
    }
}
