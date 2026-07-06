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
        public DbSet<ItemCatalogo> ItemsCatalogo { get; set; } = null!;
        public DbSet<Proveedor> Proveedores { get; set; } = null!;
        public DbSet<LotePeps> LotesPeps { get; set; } = null!;
        public DbSet<ViajeDespacho> ViajesDespacho { get; set; } = null!;
        public DbSet<ViajeDetalle> ViajesDetalle { get; set; } = null!;
        public DbSet<StockMinimo> StockMinimos { get; set; } = null!;
        public DbSet<RecetaItem> RecetasItem { get; set; } = null!;
        public DbSet<EventoFestivo> EventosFestivos { get; set; } = null!;
        public DbSet<LotePepsOrdenado> LotesPepsOrdenados { get; set; } = null!;

        // Esquema: Caja
        public DbSet<Turno> Turnos { get; set; } = null!;
        public DbSet<VentaPos> VentasPos { get; set; } = null!;
        public DbSet<VentaDetalle> VentasDetalle { get; set; } = null!;
        public DbSet<GastoExtra> GastosExtra { get; set; } = null!;

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
            modelBuilder.Entity<ItemCatalogo>(entity =>
            {
                entity.ToTable("Item_Catalogo", "Inventario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdItem");
                entity.Property(e => e.CodigoReferencia).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.CodigoReferencia).IsUnique();
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Categoria).IsRequired().HasMaxLength(80);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(15);
                entity.Property(e => e.UnidadMedida).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
            });

            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.ToTable("Proveedor", "Inventario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdProveedor");
                entity.Property(e => e.NombreEmpresa).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Contacto).HasMaxLength(100);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
            });

            modelBuilder.Entity<LotePeps>(entity =>
            {
                entity.ToTable("Lote_PEPS", "Inventario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdLote");
                entity.Property(e => e.Ubicacion).IsRequired().HasMaxLength(30);
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(10).HasDefaultValue("Óptimo");
                entity.Property(e => e.CantidadInicial).HasColumnType("decimal(8,2)").IsRequired();
                entity.Property(e => e.CantidadDisponible).HasColumnType("decimal(8,2)").IsRequired();
                entity.Property(e => e.FechaElaboracion).IsRequired();
                entity.Property(e => e.FechaCaducidad).IsRequired();
                entity.Property(e => e.FechaRegistro).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.LotesPeps)
                    .HasForeignKey(d => d.IdItem)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Proveedor)
                    .WithMany(p => p.LotesPeps)
                    .HasForeignKey(d => d.IdProveedor)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ViajeDespacho>(entity =>
            {
                entity.ToTable("Viaje_Despacho", "Inventario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdViaje");
                entity.Property(e => e.IdUsuarioConductor).HasColumnName("IdUsuario_Conductor");
                entity.Property(e => e.Conductor).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FechaDespacho).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Programado");
                entity.Property(e => e.Observaciones).HasMaxLength(500);
            });

            modelBuilder.Entity<ViajeDetalle>(entity =>
            {
                entity.ToTable("Viaje_Detalle", "Inventario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdDetalle");
                entity.Property(e => e.CantidadEnviada).HasColumnType("decimal(8,2)").IsRequired();

                entity.HasOne(d => d.Viaje)
                    .WithMany(p => p.Detalles)
                    .HasForeignKey(d => d.IdViaje)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Lote)
                    .WithMany()
                    .HasForeignKey(d => d.IdLote)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StockMinimo>(entity =>
            {
                entity.ToTable("Stock_Minimo", "Inventario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdStockMinimo");
                entity.Property(e => e.CantidadMinima).HasColumnType("decimal(8,2)").IsRequired();
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
                entity.HasIndex(e => e.IdItem).IsUnique();

                entity.HasOne(d => d.Item)
                    .WithMany()
                    .HasForeignKey(d => d.IdItem)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RecetaItem>(entity =>
            {
                entity.ToTable("Receta_Item", "Inventario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdReceta");
                entity.Property(e => e.CantidadRequerida).HasColumnType("decimal(8,2)").IsRequired();
                entity.HasIndex(e => new { e.IdItemTerminado, e.IdItemInsumo }).IsUnique();

                entity.HasOne(d => d.ItemTerminado)
                    .WithMany()
                    .HasForeignKey(d => d.IdItemTerminado)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ItemInsumo)
                    .WithMany()
                    .HasForeignKey(d => d.IdItemInsumo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EventoFestivo>(entity =>
            {
                entity.ToTable("Evento_Festivo", "Inventario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdEvento");
                entity.Property(e => e.NombreEvento).IsRequired().HasMaxLength(150);
                entity.Property(e => e.FechaEvento).IsRequired().HasColumnType("date");
                entity.Property(e => e.MultiplicadorDemanda).HasColumnType("decimal(5,2)").IsRequired();
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.FechaRegistro).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
            });

            modelBuilder.Entity<LotePepsOrdenado>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_Lotes_PEPS_Ordenados", "Inventario");
                entity.Property(e => e.CantidadInicial).HasColumnType("decimal(8,2)");
                entity.Property(e => e.CantidadDisponible).HasColumnType("decimal(8,2)");
            });

            // ==========================================
            // ESQUEMA: Caja
            // ==========================================
            modelBuilder.Entity<Turno>(entity =>
            {
                entity.ToTable("Turno", "Caja");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdTurno");
                entity.Property(e => e.IdUsuarioResponsable).HasColumnName("IdUsuario_Responsable").IsRequired();
                entity.Property(e => e.FechaApertura).IsRequired();
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(10);
                entity.Property(e => e.SaldoInicial).HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.TotalIngresosSistema).HasColumnType("decimal(10,2)").IsRequired().HasDefaultValue(0m);
                entity.Property(e => e.TotalGastosExtras).HasColumnType("decimal(10,2)").IsRequired().HasDefaultValue(0m);
                entity.Property(e => e.DiferenciaArqueo).HasColumnType("decimal(10,2)");

                // IdUsuario_Responsable no tiene FK física en la BD real (ver vw_Turno_Responsable);
                // la lectura cruzada con Seguridad.Usuario se resuelve a nivel de vista, no de EF.
            });

            modelBuilder.Entity<VentaPos>(entity =>
            {
                entity.ToTable("Venta_POS", "Caja");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdVenta");
                entity.Property(e => e.FechaHora).IsRequired();
                entity.Property(e => e.MetodoPago).IsRequired().HasMaxLength(10);
                entity.Property(e => e.TotalPagado).HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.Anulada).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.MotivoAnulacion).HasMaxLength(200);

                entity.HasOne(d => d.Turno)
                    .WithMany(p => p.VentasPos)
                    .HasForeignKey(d => d.IdTurno)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VentaDetalle>(entity =>
            {
                entity.ToTable("Venta_Detalle", "Caja");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdDetalle");
                entity.Property(e => e.Cantidad).HasColumnType("decimal(8,2)").IsRequired();
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.Subtotal).HasColumnType("decimal(10,2)").IsRequired();

                entity.HasOne(d => d.Venta)
                    .WithMany(p => p.Detalles)
                    .HasForeignKey(d => d.IdVenta)
                    .OnDelete(DeleteBehavior.Restrict);

                // IdItem no tiene FK física a Inventario.Item_Catalogo (ver vw_Ventas_Dia);
                // la lectura cruzada se resuelve a nivel de vista, no de EF.
            });

            modelBuilder.Entity<GastoExtra>(entity =>
            {
                entity.ToTable("Gasto_Extra", "Caja");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdGasto");
                entity.Property(e => e.Concepto).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Monto).HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.FechaHora).IsRequired();

                entity.HasOne(d => d.Turno)
                    .WithMany(p => p.GastosExtra)
                    .HasForeignKey(d => d.IdTurno)
                    .OnDelete(DeleteBehavior.Restrict);
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
