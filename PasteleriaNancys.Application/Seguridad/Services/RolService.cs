using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Seguridad.Dtos;
using PasteleriaNancys.Application.Seguridad.Interfaces;
using PasteleriaNancys.Domain.Seguridad;

namespace PasteleriaNancys.Application.Seguridad.Services
{
    public class RolService : IRolService
    {
        private readonly IRolRepository _rolRepository;

        public RolService(IRolRepository rolRepository)
        {
            _rolRepository = rolRepository;
        }

        public async Task<RolDto> CrearAsync(CrearRolRequest request)
        {
            var nombre = request.Nombre.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ReglaNegocioException("El nombre del rol es obligatorio.");
            }

            var roles = await _rolRepository.ObtenerTodosAsync();
            if (roles.Any(r => r.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ConflictoException($"Ya existe un rol con el nombre '{nombre}'.");
            }

            var rol = new Rol { Nombre = nombre };
            await _rolRepository.AgregarAsync(rol);
            await _rolRepository.GuardarCambiosAsync();

            return MapearDto(rol);
        }

        public async Task<List<RolDto>> ObtenerTodosAsync()
        {
            var roles = await _rolRepository.ObtenerTodosAsync();
            return roles.Select(MapearDto).ToList();
        }

        public async Task<RolDto> ObtenerPorIdAsync(int idRol)
        {
            var rol = await _rolRepository.ObtenerPorIdAsync(idRol)
                ?? throw new NoEncontradoException($"No se encontró el rol con id {idRol}.");

            return MapearDto(rol);
        }

        public async Task<RolDto> ActualizarAsync(int idRol, ActualizarRolRequest request)
        {
            var rol = await _rolRepository.ObtenerPorIdAsync(idRol)
                ?? throw new NoEncontradoException($"No se encontró el rol con id {idRol}.");

            var nombre = request.Nombre.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ReglaNegocioException("El nombre del rol es obligatorio.");
            }

            var roles = await _rolRepository.ObtenerTodosAsync();
            if (roles.Any(r => r.IdRol != idRol && r.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ConflictoException($"Ya existe un rol con el nombre '{nombre}'.");
            }

            rol.Nombre = nombre;
            await _rolRepository.GuardarCambiosAsync();

            return MapearDto(rol);
        }

        public async Task EliminarAsync(int idRol)
        {
            var rol = await _rolRepository.ObtenerPorIdAsync(idRol)
                ?? throw new NoEncontradoException($"No se encontró el rol con id {idRol}.");

            if (await _rolRepository.TieneUsuariosAsync(idRol))
            {
                throw new ReglaNegocioException("No se puede eliminar el rol porque tiene usuarios asignados.");
            }

            _rolRepository.Eliminar(rol);
            await _rolRepository.GuardarCambiosAsync();
        }

        private static RolDto MapearDto(Rol rol) => new()
        {
            IdRol = rol.IdRol,
            Nombre = rol.Nombre
        };
    }
}
