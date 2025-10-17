using Abstract_CR.Models;
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
        public DbSet<EbookEdicion> EbookEdicion { get; set; }


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
        }
    }
}
