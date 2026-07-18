using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Pedidos.Dtos;
using PasteleriaNancys.Application.Pedidos.Interfaces;
using PasteleriaNancys.Domain.Pedidos;

namespace PasteleriaNancys.Application.Pedidos.Services
{
    public class PortafolioImagenService : IPortafolioImagenService
    {
        public static readonly string[] CategoriasValidas =
        {
            "Bodas", "QuinceAnos", "Bautizos", "BabyShowers", "CumpleanosEspeciales", "TortasTematicas"
        };

        private readonly IPortafolioImagenRepository _portafolioImagenRepository;

        public PortafolioImagenService(IPortafolioImagenRepository portafolioImagenRepository)
        {
            _portafolioImagenRepository = portafolioImagenRepository;
        }

        public async Task<List<PortafolioImagenDto>> ObtenerPorCategoriaAsync(string categoria)
        {
            if (!CategoriasValidas.Contains(categoria))
            {
                throw new ReglaNegocioException($"Categoría inválida. Valores permitidos: {string.Join(", ", CategoriasValidas)}.");
            }

            var imagenes = await _portafolioImagenRepository.ObtenerPorCategoriaAsync(categoria);
            return imagenes.Select(MapearDto).ToList();
        }

        public async Task<List<PortafolioImagenDto>> ObtenerTodasAsync()
        {
            var imagenes = await _portafolioImagenRepository.ObtenerTodasAsync();
            return imagenes.Select(MapearDto).ToList();
        }

        public async Task<PortafolioImagenDto> AgregarAsync(string categoria, string? descripcion, string imagenUrl)
        {
            if (!CategoriasValidas.Contains(categoria))
            {
                throw new ReglaNegocioException($"Categoría inválida. Valores permitidos: {string.Join(", ", CategoriasValidas)}.");
            }

            var imagen = new PortafolioImagen
            {
                Id = Guid.NewGuid(),
                Categoria = categoria,
                ImagenUrl = imagenUrl,
                Descripcion = descripcion,
                Orden = 0,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            await _portafolioImagenRepository.AgregarAsync(imagen);
            await _portafolioImagenRepository.GuardarCambiosAsync();

            return MapearDto(imagen);
        }

        public async Task DesactivarAsync(Guid id)
        {
            var imagen = await _portafolioImagenRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró la imagen con id {id}.");

            imagen.Activo = false;
            await _portafolioImagenRepository.GuardarCambiosAsync();
        }

        private static PortafolioImagenDto MapearDto(PortafolioImagen imagen) => new()
        {
            Id = imagen.Id,
            Categoria = imagen.Categoria,
            ImagenUrl = imagen.ImagenUrl,
            Descripcion = imagen.Descripcion,
            Orden = imagen.Orden,
            Activo = imagen.Activo,
            FechaCreacion = imagen.FechaCreacion
        };
    }
}
