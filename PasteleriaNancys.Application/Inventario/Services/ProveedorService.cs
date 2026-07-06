using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly IProveedorRepository _proveedorRepository;

        public ProveedorService(IProveedorRepository proveedorRepository)
        {
            _proveedorRepository = proveedorRepository;
        }

        public async Task<ProveedorDto> CrearAsync(CrearProveedorRequest request)
        {
            var nombreEmpresa = request.NombreEmpresa.Trim();
            if (string.IsNullOrWhiteSpace(nombreEmpresa))
            {
                throw new ReglaNegocioException("El nombre de la empresa es obligatorio.");
            }

            if (await _proveedorRepository.ObtenerPorNombreAsync(nombreEmpresa) is not null)
            {
                throw new ConflictoException($"Ya existe un proveedor con el nombre '{nombreEmpresa}'.");
            }

            var proveedor = new Proveedor
            {
                Id = Guid.NewGuid(),
                NombreEmpresa = nombreEmpresa,
                Contacto = request.Contacto?.Trim(),
                Telefono = request.Telefono?.Trim(),
                Activo = true
            };

            await _proveedorRepository.AgregarAsync(proveedor);
            await _proveedorRepository.GuardarCambiosAsync();

            return MapearDto(proveedor);
        }

        public async Task<List<ProveedorDto>> ObtenerTodosAsync()
        {
            var proveedores = await _proveedorRepository.ObtenerTodosAsync();
            return proveedores.Select(MapearDto).ToList();
        }

        public async Task<ProveedorDto> ObtenerPorIdAsync(Guid id)
        {
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el proveedor con id {id}.");

            return MapearDto(proveedor);
        }

        public async Task<ProveedorDto> ActualizarAsync(Guid id, ActualizarProveedorRequest request)
        {
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el proveedor con id {id}.");

            var nombreEmpresa = request.NombreEmpresa.Trim();
            if (string.IsNullOrWhiteSpace(nombreEmpresa))
            {
                throw new ReglaNegocioException("El nombre de la empresa es obligatorio.");
            }

            var existente = await _proveedorRepository.ObtenerPorNombreAsync(nombreEmpresa);
            if (existente is not null && existente.Id != id)
            {
                throw new ConflictoException($"Ya existe un proveedor con el nombre '{nombreEmpresa}'.");
            }

            proveedor.NombreEmpresa = nombreEmpresa;
            proveedor.Contacto = request.Contacto?.Trim();
            proveedor.Telefono = request.Telefono?.Trim();

            await _proveedorRepository.GuardarCambiosAsync();

            return MapearDto(proveedor);
        }

        public async Task DesactivarAsync(Guid id)
        {
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el proveedor con id {id}.");

            proveedor.Activo = false;
            await _proveedorRepository.GuardarCambiosAsync();
        }

        private static ProveedorDto MapearDto(Proveedor proveedor) => new()
        {
            Id = proveedor.Id,
            NombreEmpresa = proveedor.NombreEmpresa,
            Contacto = proveedor.Contacto,
            Telefono = proveedor.Telefono,
            Activo = proveedor.Activo
        };
    }
}
