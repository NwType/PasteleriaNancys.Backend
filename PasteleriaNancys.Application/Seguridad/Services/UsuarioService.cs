using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Seguridad.Dtos;
using PasteleriaNancys.Application.Seguridad.Interfaces;
using PasteleriaNancys.Domain.Seguridad;

namespace PasteleriaNancys.Application.Seguridad.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRolRepository _rolRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IRolRepository rolRepository,
            IPasswordHasher passwordHasher)
        {
            _usuarioRepository = usuarioRepository;
            _rolRepository = rolRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<UsuarioDto> CrearAsync(CrearUsuarioRequest request)
        {
            var correo = request.Correo.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(request.NombreCompleto))
            {
                throw new ReglaNegocioException("El nombre completo es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(correo))
            {
                throw new ReglaNegocioException("El correo es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            {
                throw new ReglaNegocioException("La contraseña debe tener al menos 8 caracteres.");
            }

            if (await _usuarioRepository.ObtenerPorCorreoAsync(correo) is not null)
            {
                throw new ConflictoException($"Ya existe un usuario con el correo '{correo}'.");
            }

            var rol = await _rolRepository.ObtenerPorIdAsync(request.IdRol)
                ?? throw new NoEncontradoException($"No se encontró el rol con id {request.IdRol}.");

            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                NombreCompleto = request.NombreCompleto.Trim(),
                Correo = correo,
                PasswordHash = _passwordHasher.Hash(request.Password),
                IdRol = rol.IdRol,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            await _usuarioRepository.AgregarAsync(usuario);
            await _usuarioRepository.GuardarCambiosAsync();

            return MapearDto(usuario, rol.Nombre);
        }

        public async Task<List<UsuarioDto>> ObtenerTodosAsync()
        {
            var usuarios = await _usuarioRepository.ObtenerTodosAsync();
            return usuarios.Select(u => MapearDto(u, u.Rol.Nombre)).ToList();
        }

        public async Task<UsuarioDto> ObtenerPorIdAsync(Guid id)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el usuario con id {id}.");

            return MapearDto(usuario, usuario.Rol.Nombre);
        }

        public async Task<UsuarioDto> ActualizarAsync(Guid id, ActualizarUsuarioRequest request)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el usuario con id {id}.");

            var correo = request.Correo.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(request.NombreCompleto))
            {
                throw new ReglaNegocioException("El nombre completo es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(correo))
            {
                throw new ReglaNegocioException("El correo es obligatorio.");
            }

            var usuarioConCorreo = await _usuarioRepository.ObtenerPorCorreoAsync(correo);
            if (usuarioConCorreo is not null && usuarioConCorreo.Id != id)
            {
                throw new ConflictoException($"Ya existe un usuario con el correo '{correo}'.");
            }

            var rol = await _rolRepository.ObtenerPorIdAsync(request.IdRol)
                ?? throw new NoEncontradoException($"No se encontró el rol con id {request.IdRol}.");

            usuario.NombreCompleto = request.NombreCompleto.Trim();
            usuario.Correo = correo;
            usuario.IdRol = rol.IdRol;

            await _usuarioRepository.GuardarCambiosAsync();

            return MapearDto(usuario, rol.Nombre);
        }

        public async Task DesactivarAsync(Guid id)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el usuario con id {id}.");

            usuario.Activo = false;
            await _usuarioRepository.GuardarCambiosAsync();
        }

        public async Task DesbloquearAsync(Guid id)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el usuario con id {id}.");

            usuario.Bloqueado = false;
            usuario.IntentosFallidos = 0;
            await _usuarioRepository.GuardarCambiosAsync();
        }

        private static UsuarioDto MapearDto(Usuario usuario, string rolNombre) => new()
        {
            Id = usuario.Id,
            NombreCompleto = usuario.NombreCompleto,
            Correo = usuario.Correo,
            IdRol = usuario.IdRol,
            RolNombre = rolNombre,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            Bloqueado = usuario.Bloqueado,
            IntentosFallidos = usuario.IntentosFallidos
        };
    }
}
