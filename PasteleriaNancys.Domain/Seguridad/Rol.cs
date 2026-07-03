namespace PasteleriaNancys.Domain.Seguridad
{
    public class Rol
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;

        // Propiedades de navegación
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
