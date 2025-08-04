using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class PreferenciaNutricional
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la preferencia es obligatorio")]
        [Display(Name = "Nombre de la Preferencia")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        [StringLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres")]
        public string? Descripcion { get; set; }

        [Display(Name = "Categoría")]
        public string? Categoria { get; set; }

        [Display(Name = "Valor")]
        public string? Valor { get; set; }

        [Display(Name = "Prioridad")]
        [Range(1, 5, ErrorMessage = "La prioridad debe estar entre 1 y 5")]
        public int? Prioridad { get; set; }

        [Display(Name = "Activa")]
        public bool Activa { get; set; } = true;

        // Relación con Usuario
        public int UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; } = null!;

        // Propiedades de auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
    }
} 