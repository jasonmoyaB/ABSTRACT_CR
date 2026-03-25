using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class Usuario
    {
        [Key]
        public int UsuarioID { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [Display(Name = "Apellido")]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato inválido")]
        [Display(Name = "Correo Electrónico")]
        [StringLength(100)]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Display(Name = "Contraseña")]
        [StringLength(255)]
        [DataType(DataType.Password)]
        public string? ContrasenaHash { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime? FechaRegistro { get; set; }

        [Display(Name = "Rol")]
        public int? RolID { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Permitir Descarga de Ebook")]
        public bool PermitirDescargaEbook { get; set; } = false;

        public string CorreoNorm { get; set; } = string.Empty;

        [Display(Name = "Puntos Acumulados")]
        public int PuntosTotales { get; set; }

        [Display(Name = "Dirección")]
        [StringLength(250)]
        public string? Direccion { get; set; }

        [Display(Name = "Teléfono")]
        [StringLength(50)]
        public string? Telefono { get; set; }

        public virtual Rol? Rol { get; set; }

        [Display(Name = "Nombre Completo")]
        public string NombreCompleto => $"{Nombre} {Apellido}";

        public virtual ICollection<PlanNutricional> PlanesNutricionales { get; set; } = new List<PlanNutricional>();
        public virtual ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
        public virtual ICollection<Suscripcion> Suscripciones { get; set; } = new List<Suscripcion>();
        public virtual ICollection<MensajeInteraccion> Mensajes { get; set; } = new List<MensajeInteraccion>();
        public virtual ICollection<PuntosUsuario> HistorialPuntos { get; set; } = new List<PuntosUsuario>();
        public virtual ICollection<RestriccionAlimentaria> RestriccionesAlimentarias { get; set; } = new List<RestriccionAlimentaria>();
    }

    public class UsuarioPorAsignar
    {
        public int UsuarioID { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; }
    }
}
