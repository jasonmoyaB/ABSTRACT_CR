using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class Rol
    {
        [Key]
        public int RolID { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [Display(Name = "Nombre del Rol")]
        [StringLength(50, ErrorMessage = "El nombre del rol no puede tener más de 50 caracteres")]
        public string NombreRol { get; set; } = string.Empty;

        // Propiedades de navegación
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}

