using Abstract_CR.Models;
using Microsoft.EntityFrameworkCore;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
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

            
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.HasKey(e => e.RolID);
                entity.Property(e => e.NombreRol).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.NombreRol).IsUnique();
            });

            
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

                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ComentarioReceta>(entity =>
            {
                entity.ToTable("ComentariosRecetas");
                entity.HasKey(e => e.ComentarioID);
                entity.Property(e => e.UsuarioID).IsRequired();
                entity.Property(e => e.RecetaID).IsRequired();
                entity.Property(e => e.ComentarioID).IsRequired();
                entity.Property(e => e.FechaComentario).IsRequired();
            });

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

            modelBuilder.Entity<RecetaPorUsuario>(entity =>
            {
                entity.ToTable("RecetasPorUsuario");
                entity.HasKey(e => e.RecetaPorUsuarioID);
                entity.Property(e => e.RecetaID).IsRequired();
                entity.Property(e => e.UsuarioID).IsRequired();
                entity.Property(e => e.Dia).IsRequired().HasMaxLength(20);
            });

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
        }
    }
}
