using System.Collections.Generic;
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
        public string? ContrasenaHash { get; set; } // ← Ya no es obligatorio

        [Display(Name = "Fecha de Registro")]
        public DateTime? FechaRegistro { get; set; }

        [Display(Name = "Rol")]
        public int? RolID { get; set; } // ← Ya no es obligatorio

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Permitir Descarga de Ebook")]
        public bool PermitirDescargaEbook { get; set; } = false;

        public string CorreoNorm { get; set; } = string.Empty;

        public virtual Rol? Rol { get; set; }

        [Display(Name = "Nombre Completo")]
        public string NombreCompleto => $"{Nombre} {Apellido}";

        public virtual ICollection<PlanNutricional> PlanesNutricionales { get; set; } = new List<PlanNutricional>();
        public virtual ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
        public virtual ICollection<Suscripcion> Suscripciones { get; set; } = new List<Suscripcion>();
        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    }
}
