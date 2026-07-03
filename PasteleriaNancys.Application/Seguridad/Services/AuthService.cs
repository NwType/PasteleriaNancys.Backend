using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Seguridad.Dtos;
using PasteleriaNancys.Application.Seguridad.Interfaces;
using PasteleriaNancys.Application.Seguridad;

namespace PasteleriaNancys.Application.Seguridad.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly SeguridadSettings _seguridadSettings;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            SeguridadSettings seguridadSettings)
        {
            _usuarioRepository = usuarioRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _seguridadSettings = seguridadSettings;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var correo = request.Correo.Trim().ToLowerInvariant();
            var usuario = await _usuarioRepository.ObtenerPorCorreoAsync(correo);

            if (usuario is null || !usuario.Activo)
            {
                throw new CredencialesInvalidasException("Correo o contraseña incorrectos.");
            }

            if (usuario.Bloqueado)
            {
                throw new CuentaBloqueadaException("Cuenta bloqueada. Contacte al Administrador.");
            }

            if (!_passwordHasher.Verificar(request.Password, usuario.PasswordHash))
            {
                usuario.IntentosFallidos++;
                if (usuario.IntentosFallidos >= _seguridadSettings.MaxIntentosFallidos)
                {
                    usuario.Bloqueado = true;
                }

                await _usuarioRepository.GuardarCambiosAsync();
                throw new CredencialesInvalidasException("Correo o contraseña incorrectos.");
            }

            usuario.IntentosFallidos = 0;
            await _usuarioRepository.GuardarCambiosAsync();

            var (token, expiraEn) = _jwtTokenGenerator.GenerarToken(usuario);

            return new LoginResponse
            {
                Token = token,
                ExpiraEn = expiraEn,
                Usuario = new UsuarioDto
                {
                    Id = usuario.Id,
                    NombreCompleto = usuario.NombreCompleto,
                    Correo = usuario.Correo,
                    IdRol = usuario.IdRol,
                    RolNombre = usuario.Rol.Nombre,
                    Activo = usuario.Activo,
                    FechaCreacion = usuario.FechaCreacion,
                    Bloqueado = usuario.Bloqueado,
                    IntentosFallidos = usuario.IntentosFallidos
                }
            };
        }
    }
}
