namespace PasteleriaNancys.Application.Seguridad.Dtos
{
    public class UsuarioDto
    {
        public Guid Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Bloqueado { get; set; }
        public int IntentosFallidos { get; set; }
    }
}
