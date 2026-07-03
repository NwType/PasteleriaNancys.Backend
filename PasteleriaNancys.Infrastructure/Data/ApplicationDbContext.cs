using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Domain.Seguridad;
using PasteleriaNancys.Domain.Inventario;
using PasteleriaNancys.Domain.Caja;
using PasteleriaNancys.Domain.Pedidos;

namespace PasteleriaNancys.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Esquema: Seguridad
        public DbSet<Rol> Roles { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;

        // Esquema: Inventario
        public DbSet<CatalogoItem> CatalogoItems { get; set; } = null!;
        public DbSet<LotePeps> LotesPeps { get; set; } = null!;

        // Esquema: Caja
        public DbSet<TurnoCaja> TurnosCaja { get; set; } = null!;
        public DbSet<VentaPos> VentasPos { get; set; } = null!;

        // Esquema: Web (Pedidos)
        public DbSet<PedidoWeb> PedidosWeb { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // ESQUEMA: Seguridad
            // ==========================================
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("Rol", "Seguridad");
                entity.HasKey(e => e.IdRol);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Nombre).IsUnique();

                entity.HasData(
                    new Rol { IdRol = 1, Nombre = "Administrador" },
                    new Rol { IdRol = 2, Nombre = "Vendedora" },
                    new Rol { IdRol = 3, Nombre = "Encargado de Almacen" }
                );
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario", "Seguridad");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdUsuario");
                entity.Property(e => e.NombreCompleto).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Correo).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Correo).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.IntentosFallidos).IsRequired().HasDefaultValue(0);
                entity.Property(e => e.Bloqueado).IsRequired().HasDefaultValue(false);

                entity.HasOne(d => d.Rol)
                    .WithMany(p => p.Usuarios)
                    .HasForeignKey(d => d.IdRol)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // ESQUEMA: Inventario
            // ==========================================
            modelBuilder.Entity<CatalogoItem>(entity =>
            {
                entity.ToTable("CatalogoItem", "Inventario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CodigoReferencia).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Categoria).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TipoItem).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UnidadMedida).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<LotePeps>(entity =>
            {
                entity.ToTable("LotePeps", "Inventario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Ubicacion).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(50);

                // Configuración de precisión decimal para inventario
                entity.Property(e => e.CantidadInicial)
                    .HasColumnType("decimal(8,2)")
                    .IsRequired();

                entity.Property(e => e.CantidadDisponible)
                    .HasColumnType("decimal(8,2)")
                    .IsRequired();

                entity.Property(e => e.FechaElaboracion).IsRequired();
                entity.Property(e => e.FechaCaducidad).IsRequired();

                entity.HasOne(d => d.CatalogoItem)
                    .WithMany(p => p.LotesPeps)
                    .HasForeignKey(d => d.CatalogoItemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // ESQUEMA: Caja
            // ==========================================
            modelBuilder.Entity<TurnoCaja>(entity =>
            {
                entity.ToTable("TurnoCaja", "Caja");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UsuarioAperturaId).IsRequired();
                entity.Property(e => e.FechaApertura).IsRequired();
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(50);

                // Configuración de precisión decimal para dinero
                entity.Property(e => e.SaldoInicial)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(e => e.TotalIngresos)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(e => e.DiferenciaArqueo)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();
            });

            modelBuilder.Entity<VentaPos>(entity =>
            {
                entity.ToTable("VentaPos", "Caja");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FechaHora).IsRequired();
                entity.Property(e => e.MetodoPago).IsRequired().HasMaxLength(50);

                // Configuración de precisión decimal para dinero
                entity.Property(e => e.TotalCobrado)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.HasOne(d => d.TurnoCaja)
                    .WithMany(p => p.VentasPos)
                    .HasForeignKey(d => d.TurnoCajaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // ESQUEMA: Web (Pedidos)
            // ==========================================
            modelBuilder.Entity<PedidoWeb>(entity =>
            {
                entity.ToTable("PedidoWeb", "Web");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NombreCliente).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Telefono).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FechaEntrega).IsRequired();
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(50);

                // Configuración de precisión decimal para dinero
                entity.Property(e => e.Total)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();
            });
        }
    }
}
