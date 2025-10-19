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

        // DbSets para las tablas principales
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<PassResetTokens> Tokens { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de la tabla Usuarios
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

                // Relación con Rol
                entity.HasOne(e => e.Rol)
                      .WithMany(r => r.Usuarios)
                      .HasForeignKey(e => e.RolID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de la tabla Roles
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.HasKey(e => e.RolID);
                entity.Property(e => e.NombreRol).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.NombreRol).IsUnique();
            });

            // Datos iniciales para roles
            modelBuilder.Entity<Rol>().HasData(
                new Rol { RolID = 1, NombreRol = "Administrador" },
                new Rol { RolID = 2, NombreRol = "Cliente" },
                new Rol { RolID = 3, NombreRol = "Nutricionista" }
            );

            modelBuilder.Entity<PassResetTokens>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UsuarioID).IsRequired();
                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.FechaCreacion).IsRequired();

                // Relación con Usuario
                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

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
