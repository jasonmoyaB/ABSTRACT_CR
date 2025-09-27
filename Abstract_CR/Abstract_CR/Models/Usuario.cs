using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class Usuario
    {
        [Key]
        public int UsuarioID { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [Display(Name = "Apellido")]
        [StringLength(100, ErrorMessage = "El apellido no puede tener más de 100 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Correo Electrónico")]
        [StringLength(100)]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [Display(Name = "Contraseña")]
        [StringLength(255)]
        [DataType(DataType.Password)]
        public string ContrasenaHash { get; set; } = string.Empty;

        [Display(Name = "Fecha de Registro")]
        public DateTime? FechaRegistro { get; set; }

        [Required]
        public int RolID { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Propiedad calculada para correo normalizado (se maneja automáticamente en la BD)
        public string CorreoNorm { get; set; } = string.Empty;

        // Propiedades de navegación
        public virtual Rol Rol { get; set; } = null!;

        // Propiedades calculadas para la vista
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
} 