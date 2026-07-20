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
        public DbSet<Horneada> Horneadas { get; set; } = null!;
        public DbSet<ConsumoInsumo> ConsumosInsumo { get; set; } = null!;
        public DbSet<Merma> Mermas { get; set; } = null!;

        // Esquema: Caja
        public DbSet<Turno> Turnos { get; set; } = null!;
        public DbSet<VentaPos> VentasPos { get; set; } = null!;
        public DbSet<VentaDetalle> VentasDetalle { get; set; } = null!;
        public DbSet<VentaDetalleLote> VentasDetalleLote { get; set; } = null!;
        public DbSet<GastoExtra> GastosExtra { get; set; } = null!;

        // Esquema: Web (Pedidos)
        public DbSet<PedidoCliente> PedidosCliente { get; set; } = null!;
        public DbSet<PedidoConfiguracion> PedidosConfiguracion { get; set; } = null!;
        public DbSet<PagoQr> PagosQr { get; set; } = null!;
        public DbSet<TablaPrecioPorciones> TablaPrecioPorciones { get; set; } = null!;
        public DbSet<PortafolioImagen> PortafolioImagenes { get; set; } = null!;

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
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10,2)").IsRequired().HasDefaultValue(0m);
                entity.Property(e => e.NumeroPorciones);
                entity.Property(e => e.EsPersonalizable).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.CategoriaPersonalizacion).HasMaxLength(20);
                entity.Property(e => e.TipoCremaAsociado).HasMaxLength(20);
                entity.Property(e => e.ImagenUrl).HasMaxLength(500);
                entity.Property(e => e.Descripcion).HasMaxLength(1000);
                entity.Property(e => e.ColorDecoracion).HasMaxLength(50);
                entity.Property(e => e.TipoMasa).HasMaxLength(20);
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Inventario_Item_CategoriaPersonalizacion",
                        "[CategoriaPersonalizacion] IS NULL OR [CategoriaPersonalizacion] IN ('Relleno','Crema','Colorante')");
                    t.HasCheckConstraint("CK_Inventario_Item_TipoCremaAsociado",
                        "[TipoCremaAsociado] IS NULL OR [TipoCremaAsociado] IN ('Mascrean','CremaPil','Fondant')");
                    t.HasCheckConstraint("CK_Inventario_Item_TipoMasa",
                        "[TipoMasa] IS NULL OR [TipoMasa] IN ('Vainilla','Chocolate','Mixto')");
                    t.HasCheckConstraint("CK_Inventario_Item_Categoria",
                        "([Tipo]='Terminado' AND [Categoria] IN ('Tortas Clásicas','Tortas Personalizables')) " +
                        "OR ([Tipo]='MateriaPrima' AND [Categoria] IN ('Harinas y Secos','Lácteos y Cremas','Colorantes y Jaleas','Rellenos','Empaques')) " +
                        "OR ([Tipo]='Intermedio' AND [Categoria] IN ('Bizcochos','Preparados'))");
                });

                entity.HasOne(e => e.InsumoRelleno)
                    .WithMany()
                    .HasForeignKey(e => e.IdInsumoRelleno)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.InsumoCrema)
                    .WithMany()
                    .HasForeignKey(e => e.IdInsumoCrema)
                    .OnDelete(DeleteBehavior.Restrict);
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
                // (12,6): la receta del bizcocho se guarda POR PORCIÓN (batida ÷ 200), y valores
                // como el polvo de hornear (0.000375 kg/porción) se truncaban con la (8,2) original.
                entity.Property(e => e.CantidadRequerida).HasColumnType("decimal(12,6)").IsRequired();
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

            modelBuilder.Entity<Horneada>(entity =>
            {
                entity.ToTable("Horneada", "Inventario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdHorneada");
                entity.Property(e => e.Fecha).IsRequired().HasColumnType("date");
                entity.Property(e => e.CantidadBatidas).IsRequired();
                entity.Property(e => e.CantidadBatidasChocolateExtra).IsRequired().HasDefaultValue(0);
                // IdUsuarioRegistro: cross-schema hacia Seguridad.Usuario, sin FK física (mismo patrón que Turno/Pedido_Cliente).
                entity.Property(e => e.IdUsuarioRegistro).IsRequired();
                entity.Property(e => e.FechaRegistro).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Inventario_Horneada_CantidadBatidas", "[CantidadBatidas] > 0");
                    t.HasCheckConstraint(
                        "CK_Inventario_Horneada_BatidasChocolateExtra",
                        "[CantidadBatidasChocolateExtra] >= 0 AND [CantidadBatidasChocolateExtra] <= [CantidadBatidas] - 1");
                });
            });

            modelBuilder.Entity<ConsumoInsumo>(entity =>
            {
                entity.ToTable("Consumo_Insumo", "Inventario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdConsumo");
                entity.Property(e => e.CantidadDescontada).HasColumnType("decimal(8,2)").IsRequired();
                entity.Property(e => e.Motivo).HasMaxLength(200);
                entity.Property(e => e.Fecha).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
                // IdUsuarioRegistro: cross-schema hacia Seguridad.Usuario, sin FK física.
                entity.Property(e => e.IdUsuarioRegistro).IsRequired();
                // IdPedido: cross-schema hacia Web.Pedido_Cliente, sin FK física — trazabilidad
                // del consumo que dispara producir una torta personalizable (2026-07-18).
                entity.Property(e => e.IdPedido);
                entity.HasIndex(e => e.IdPedido);

                entity.HasOne(e => e.Horneada)
                    .WithMany(h => h.Consumos)
                    .HasForeignKey(e => e.IdHorneada)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Item)
                    .WithMany()
                    .HasForeignKey(e => e.IdItem)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Lote)
                    .WithMany()
                    .HasForeignKey(e => e.IdLote)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LoteProducido)
                    .WithMany()
                    .HasForeignKey(e => e.IdLoteProducido)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Merma>(entity =>
            {
                entity.ToTable("Merma", "Inventario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdMerma");
                entity.Property(e => e.Cantidad).HasColumnType("decimal(8,2)").IsRequired();
                entity.Property(e => e.TipoMerma).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Motivo).HasMaxLength(300);
                entity.Property(e => e.Fecha).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
                // IdUsuarioRegistro: cross-schema hacia Seguridad.Usuario, sin FK física.
                entity.Property(e => e.IdUsuarioRegistro).IsRequired();

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Inventario_Merma_Cantidad", "[Cantidad] > 0");
                    t.HasCheckConstraint("CK_Inventario_Merma_Tipo",
                        "[TipoMerma] IN ('Insumo dañado','Producción fallida','Caducidad','Accidente','Otro')");
                });

                entity.HasOne(e => e.Item)
                    .WithMany()
                    .HasForeignKey(e => e.IdItem)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Lote)
                    .WithMany()
                    .HasForeignKey(e => e.IdLote)
                    .OnDelete(DeleteBehavior.Restrict);
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
                entity.Property(e => e.MontoFisicoContado).HasColumnType("decimal(10,2)");
                entity.Property(e => e.DiferenciaArqueo).HasColumnType("decimal(10,2)");
                entity.Property(e => e.RowVersion).IsRowVersion();

                // Evita que dos aperturas simultáneas del mismo usuario creen dos turnos "Abierto"
                // a la vez (antes solo se prevenía con un check-then-act en la app, con condición
                // de carrera real bajo peticiones concurrentes).
                entity.HasIndex(e => e.IdUsuarioResponsable)
                    .IsUnique()
                    .HasFilter("[Estado] = 'Abierto'")
                    .HasDatabaseName("UX_Caja_Turno_UsuarioAbierto");

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

            modelBuilder.Entity<VentaDetalleLote>(entity =>
            {
                entity.ToTable("Venta_Detalle_Lote", "Caja");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdVentaDetalleLote");
                entity.Property(e => e.CantidadDescontada).HasColumnType("decimal(8,2)").IsRequired();

                entity.HasOne(d => d.VentaDetalle)
                    .WithMany()
                    .HasForeignKey(d => d.IdVentaDetalle)
                    .OnDelete(DeleteBehavior.Restrict);

                // IdLote no tiene FK física a Inventario.Lote_PEPS, mismo patrón cross-schema que
                // Venta_Detalle.IdItem.
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
            modelBuilder.Entity<PedidoCliente>(entity =>
            {
                entity.ToTable("Pedido_Cliente", "Web");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdPedido");
                entity.Property(e => e.NombreCliente).IsRequired().HasMaxLength(150);
                entity.Property(e => e.WhatsApp).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FechaSolicitud).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.FechaEntregaSolicitada).HasColumnName("FechaEntrega_Solicitada").IsRequired();
                entity.Property(e => e.TotalCotizado).HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Pendiente QR");
                entity.Property(e => e.CodigoQrReferencia).HasColumnName("CodigoQR_Referencia").HasMaxLength(100);
                entity.Property(e => e.Observaciones).HasMaxLength(500);
                entity.Property(e => e.IdUsuarioVendedora).HasColumnName("IdUsuario_Vendedora");

                // IdUsuario_Vendedora no tiene FK física en la BD real (cross-schema, sin FK física,
                // mismo patrón que Turno.IdUsuario_Responsable) — la lectura cruzada con
                // Seguridad.Usuario se resuelve a nivel de aplicación, no de EF.
            });

            modelBuilder.Entity<PedidoConfiguracion>(entity =>
            {
                entity.ToTable("Pedido_Configuracion", "Web");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdConfiguracion");
                entity.Property(e => e.IdItemProductoBase).HasColumnName("IdItem_ProductoBase");
                entity.Property(e => e.IdInsumoSaborMasa).HasColumnName("IdInsumo_SaborMasa");
                entity.Property(e => e.IdInsumoRelleno).HasColumnName("IdInsumo_Relleno");
                entity.Property(e => e.SaborMasa).IsRequired().HasMaxLength(80);
                entity.Property(e => e.TipoRelleno).IsRequired().HasMaxLength(80);
                entity.Property(e => e.TamanoRacion).HasMaxLength(30);
                entity.Property(e => e.ColorDecoracion).HasMaxLength(50);
                entity.Property(e => e.DedicatoriaDetalle).HasColumnName("Dedicatoria_Detalle");
                entity.Property(e => e.ImagenReferenciaUrl).HasColumnName("ImagenReferencia_URL").HasMaxLength(500);
                entity.Property(e => e.PorcentajeAnticipo).HasColumnName("Porcentaje_Anticipo").HasColumnType("decimal(5,2)");
                entity.Property(e => e.TipoMasa).HasMaxLength(20);
                entity.Property(e => e.TipoCrema).HasMaxLength(20);
                entity.Property(e => e.IdInsumoCrema).HasColumnName("IdInsumo_Crema");
                entity.Property(e => e.IdInsumoColorDecoracion).HasColumnName("IdInsumo_ColorDecoracion");
                entity.Property(e => e.CantidadVelasNormales).IsRequired().HasDefaultValue(0);
                entity.Property(e => e.VelaNumerica).HasMaxLength(10);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Web_PedidoConfiguracion_TipoMasa",
                        "[TipoMasa] IS NULL OR [TipoMasa] IN ('Vainilla','Chocolate','Mixto')");
                    t.HasCheckConstraint("CK_Web_PedidoConfiguracion_TipoCrema",
                        "[TipoCrema] IS NULL OR [TipoCrema] IN ('Mascrean','CremaPil','Fondant')");
                });

                entity.HasOne(d => d.Pedido)
                    .WithOne(p => p.Configuracion)
                    .HasForeignKey<PedidoConfiguracion>(d => d.IdPedido)
                    .OnDelete(DeleteBehavior.Restrict);

                // IdItem_ProductoBase, IdInsumo_SaborMasa, IdInsumo_Relleno, IdInsumo_Crema e
                // IdInsumo_ColorDecoracion no tienen FK física en la BD real (cross-schema, sin FK física,
                // mismo patrón que Venta_Detalle.IdItem) — la lectura cruzada con Inventario.Item_Catalogo
                // se resuelve a nivel de aplicación, no de EF.
            });

            modelBuilder.Entity<TablaPrecioPorciones>(entity =>
            {
                entity.ToTable("Tabla_Precio_Porciones", "Web");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdTablaPrecio");
                entity.Property(e => e.NumeroPorciones).IsRequired();
                entity.Property(e => e.Precio).HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(50);
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);

                // Cada torta personalizable tiene su propia tabla de precios por porciones (antes era
                // una sola tabla global compartida) — sin FK física porque IdItemTerminado apunta a
                // Inventario.Item_Catalogo (cross-schema), mismo patrón que el resto del sistema.
                entity.HasIndex(e => new { e.IdItemTerminado, e.NumeroPorciones }).IsUnique();

                // Los 9 valores originales quedaron sembrados por SQL en la migración
                // AsociarTablaPrecioPorcionesAProducto (no vía HasData: apuntan a filas reales de
                // Item_Catalogo creadas en vivo, cuyo Id no existe todavía cuando se escribe esta
                // migración).
            });

            modelBuilder.Entity<PagoQr>(entity =>
            {
                entity.ToTable("Pago_QR", "Web");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdPago");
                entity.Property(e => e.FechaGeneracion).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.MontoSolicitado).HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.MontoConfirmado).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Diferencia).HasColumnType("decimal(10,2)");
                entity.Property(e => e.CanalPago).HasMaxLength(50);
                entity.Property(e => e.CodigoRespuestaBanco).HasMaxLength(100);
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(15).HasDefaultValue("Pendiente");

                entity.HasOne(d => d.Pedido)
                    .WithMany(p => p.PagosQr)
                    .HasForeignKey(d => d.IdPedido)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PortafolioImagen>(entity =>
            {
                entity.ToTable("Portafolio_Imagen", "Web");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("IdPortafolioImagen");
                entity.Property(e => e.Categoria).IsRequired().HasMaxLength(30);
                entity.Property(e => e.ImagenUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Descripcion).HasMaxLength(300);
                entity.Property(e => e.Orden).IsRequired().HasDefaultValue(0);
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");

                entity.ToTable(t => t.HasCheckConstraint("CK_Web_PortafolioImagen_Categoria",
                    "[Categoria] IN ('Bodas','QuinceAnos','Bautizos','BabyShowers','CumpleanosEspeciales','TortasTematicas')"));
            });
        }
    }
}
