using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Abstract_CR.Models;

namespace Abstract_CR.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ✅ DbSets combinados de ambas ramas
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<PassResetTokens> Tokens { get; set; }
        public DbSet<EbookEdicion> EbookEdicion { get; set; }
        public DbSet<Suscripcion> Suscripciones { get; set; }
        public DbSet<ComentarioReceta> ComentarioRecetas { get; set; }
        public DbSet<Receta> Recetas { get; set; }
        public DbSet<RecetaPorUsuario> RecetasPorUsuario { get; set; }
        public DbSet<MensajeInteraccion> MensajesInteraccion { get; set; }
        public DbSet<PuntosUsuario> PuntosUsuarios { get; set; }
        public DbSet<MenuSemanal> MenuSemanal { get; set; }
        public DbSet<RestriccionAlimentaria> RestriccionesAlimentarias { get; set; }
        public DbSet<ComprobantePago> ComprobantesPago { get; set; }
        
        // ✅ DbSets de la rama master (Pedidos)
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===============================================================
            // CONFIGURACIÓN DE USUARIO
            // ===============================================================
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.UsuarioID);
                entity.Property(e => e.CorreoElectronico).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.CorreoElectronico).IsUnique();
                entity.Property(e => e.CorreoNorm).HasComputedColumnSql("lower(ltrim(rtrim([CorreoElectronico])))", stored: true);
                entity.HasIndex(e => e.CorreoNorm).IsUnique();
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ContrasenaHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("sysutcdatetime()");
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.PuntosTotales).HasDefaultValue(0);

                entity.HasOne(e => e.Rol)
                      .WithMany(r => r.Usuarios)
                      .HasForeignKey(e => e.RolID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===============================================================
            // CONFIGURACIÓN DE ROL
            // ===============================================================
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.HasKey(e => e.RolID);
                entity.Property(e => e.NombreRol).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.NombreRol).IsUnique();
            });

            // Seed de Roles
            modelBuilder.Entity<Rol>().HasData(
                new Rol { RolID = 1, NombreRol = "Administrador" },
                new Rol { RolID = 2, NombreRol = "Cliente" },
                new Rol { RolID = 3, NombreRol = "Nutricionista" }
            );

            // ===============================================================
            // CONFIGURACIÓN DE TOKENS DE RECUPERACIÓN
            // ===============================================================
            modelBuilder.Entity<PassResetTokens>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UsuarioID).IsRequired();
                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.FechaCreacion).IsRequired();

                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===============================================================
            // CONFIGURACIÓN DE COMENTARIOS DE RECETAS
            // ===============================================================
            modelBuilder.Entity<ComentarioReceta>(entity =>
            {
                entity.ToTable("ComentariosRecetas");
                entity.HasKey(e => e.ComentarioID);
                entity.Property(e => e.UsuarioID).IsRequired();
                entity.Property(e => e.RecetaID).IsRequired();
                entity.Property(e => e.ComentarioID).IsRequired();
                entity.Property(e => e.FechaComentario).IsRequired();
            });

            // ===============================================================
            // CONFIGURACIÓN DE RECETAS
            // ===============================================================
            modelBuilder.Entity<Receta>(entity =>
            {
                entity.HasKey(e => e.RecetaID);
                entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).IsRequired();
                entity.Property(e => e.Instrucciones).IsRequired();
                entity.Property(e => e.EsGratuita).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.EsParteDeEbook).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.ChefID).IsRequired();
            });

            // ===============================================================
            // CONFIGURACIÓN DE RECETAS POR USUARIO
            // ===============================================================
            modelBuilder.Entity<RecetaPorUsuario>(entity =>
            {
                entity.ToTable("RecetasPorUsuario");
                entity.HasKey(e => e.RecetaPorUsuarioID);
                entity.Property(e => e.RecetaID).IsRequired();
                entity.Property(e => e.UsuarioID).IsRequired();
                entity.Property(e => e.Dia).IsRequired().HasMaxLength(20);
            });

            // ===============================================================
            // CONFIGURACIÓN DE MENSAJES DE INTERACCIÓN
            // ===============================================================
            modelBuilder.Entity<MensajeInteraccion>(entity =>
            {
                entity.ToTable("MensajesInteraccion");
                entity.HasKey(e => e.MensajeInteraccionId);
                entity.Property(e => e.Contenido).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.FechaEnvio).IsRequired();
                entity.Property(e => e.Tipo).HasConversion<int>();

                entity.HasOne(e => e.Usuario)
                      .WithMany(u => u.Mensajes)
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Remitente)
                      .WithMany()
                      .HasForeignKey(e => e.RemitenteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===============================================================
            // CONFIGURACIÓN DE PUNTOS DE USUARIO
            // ===============================================================
            modelBuilder.Entity<PuntosUsuario>(entity =>
            {
                entity.ToTable("PuntosUsuarios");
                entity.HasKey(e => e.PuntosUsuarioId);
                entity.Property(e => e.Puntos).IsRequired();
                entity.Property(e => e.Motivo).HasMaxLength(250);
                entity.Property(e => e.FechaAsignacion).IsRequired();

                entity.HasOne(e => e.Usuario)
                      .WithMany(u => u.HistorialPuntos)
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AsignadoPor)
                      .WithMany()
                      .HasForeignKey(e => e.AsignadoPorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===============================================================
            // CONFIGURACIÓN DE MENÚ SEMANAL
            // ===============================================================
            modelBuilder.Entity<MenuSemanal>(entity =>
            {
                entity.ToTable("MenusSemanales");
                entity.HasKey(e => e.MenuSemanalID);
                entity.Property(e => e.TipoMenuID).IsRequired();
                entity.Property(e => e.SemanaDel).IsRequired().HasColumnType("date");
                entity.Property(e => e.NombrePlatillo).HasMaxLength(200);
                entity.Property(e => e.DiaSemana).HasMaxLength(20);
                entity.Property(e => e.Caracteristicas).HasMaxLength(500);
                entity.Property(e => e.IngredientesPrincipales).HasMaxLength(1000);
                entity.Property(e => e.TipChef).HasMaxLength(500);
                entity.Property(e => e.RutaImagen).HasMaxLength(500);
                entity.Ignore(e => e.RowVer);
            });

            // ===============================================================
            // CONFIGURACIÓN DE RESTRICCIONES ALIMENTARIAS
            // ===============================================================
            modelBuilder.Entity<RestriccionAlimentaria>(entity =>
            {
                entity.ToTable("RestriccionesAlimentarias");

                entity.HasKey(e => e.RestriccionID);

                entity.Property(e => e.Descripcion)
                      .HasColumnType("nvarchar(max)");

                entity.Property(e => e.UsuarioID)
                      .IsRequired();

                entity.HasOne(e => e.Usuario)
                      .WithMany(u => u.RestriccionesAlimentarias)
                      .HasForeignKey(e => e.UsuarioID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===============================================================
            // CONFIGURACIÓN DE COMPROBANTES DE PAGO
            // ===============================================================
            modelBuilder.Entity<ComprobantePago>(entity =>
            {
                entity.ToTable("ComprobantesPago");

                entity.HasKey(e => e.ComprobanteID);

                entity.Property(e => e.UsuarioID)
                      .IsRequired();

                entity.Property(e => e.RutaArchivo)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(e => e.NombreArchivoOriginal)
                      .HasMaxLength(255);

                entity.Property(e => e.TipoArchivo)
                      .HasMaxLength(50);

                entity.Property(e => e.FechaSubida)
                      .IsRequired()
                      .HasDefaultValueSql("sysutcdatetime()");

                entity.Property(e => e.Observaciones)
                      .HasMaxLength(500);

                entity.Property(e => e.Estado)
                      .HasMaxLength(50)
                      .HasDefaultValue("Pendiente");

                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===============================================================
            // CONFIGURACIÓN DE PEDIDOS
            // ===============================================================
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasKey(e => e.PedidoID);

                entity.Property(e => e.FechaPedido)
                      .HasDefaultValueSql("sysutcdatetime()");
                entity.Property(e => e.Total)
                      .HasColumnType("decimal(10,2)");
                entity.Property(e => e.DireccionEnvio)
                      .HasMaxLength(250);
                entity.Property(e => e.Estado)
                      .HasConversion<string>()
                      .HasMaxLength(20);
                entity.Property(e => e.MetodoPago)
                      .HasConversion<string>()
                      .HasMaxLength(30);

                entity.HasIndex(e => e.Estado);
                entity.HasIndex(e => e.UsuarioID);

                entity.HasOne(e => e.Usuario)
                      .WithMany(u => u.Pedidos)
                      .HasForeignKey(e => e.UsuarioID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===============================================================
            // CONFIGURACIÓN DE DETALLES DE PEDIDO
            // ===============================================================
            modelBuilder.Entity<PedidoDetalle>(entity =>
            {
                entity.HasKey(e => e.PedidoDetalleID);

                entity.Property(e => e.Descripcion)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(e => e.Cantidad)
                      .IsRequired();
                entity.Property(e => e.PrecioUnitario)
                      .HasColumnType("decimal(10,2)");

                entity.HasIndex(e => e.PedidoID);

                entity.HasOne(e => e.Pedido)
                      .WithMany(p => p.Detalles)
                      .HasForeignKey(e => e.PedidoID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===============================================================
            // SEED DE ESTADOS DE PEDIDOS (OPCIONAL)
            // ===============================================================
            var seedEstadosVariable = Environment.GetEnvironmentVariable("SEED_PEDIDOS_ESTADOS");
            if (!string.IsNullOrWhiteSpace(seedEstadosVariable) &&
                bool.TryParse(seedEstadosVariable, out var shouldSeedEstados) &&
                shouldSeedEstados)
            {
                const int estadoSeedUsuarioId = -1;
                modelBuilder.Entity<Usuario>().HasData(new Usuario
                {
                    UsuarioID = estadoSeedUsuarioId,
                    Nombre = "Estados",
                    Apellido = "Pedido",
                    CorreoElectronico = "seed-estados@abstract-cr.local",
                    ContrasenaHash = "SeedEstadosHash",
                    FechaRegistro = DateTime.UtcNow,
                    RolID = 2,
                    Activo = true
                });

                var estadosSeed = Enum.GetValues<EstadoPedido>().Select((estado, index) => new Pedido
                {
                    PedidoID = -(index + 1),
                    UsuarioID = estadoSeedUsuarioId,
                    FechaPedido = DateTime.UtcNow,
                    Total = 0m,
                    Estado = estado,
                    MetodoPago = MetodoPago.TarjetaCredito,
                    DireccionEnvio = string.Empty
                });

                modelBuilder.Entity<Pedido>().HasData(estadosSeed);
            }
        }
    }
}
