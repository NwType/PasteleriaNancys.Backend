using System;

namespace PasteleriaNancys.Domain.Seguridad
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int IntentosFallidos { get; set; }
        public bool Bloqueado { get; set; }

        // Propiedades de navegación
        public Rol Rol { get; set; } = null!;
    }
}
