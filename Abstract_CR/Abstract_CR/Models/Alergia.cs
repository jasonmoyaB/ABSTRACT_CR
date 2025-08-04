using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class Alergia
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la alergia es obligatorio")]
        [Display(Name = "Nombre de la Alergia")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        [StringLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres")]
        public string? Descripcion { get; set; }

        [Display(Name = "Nivel de Severidad")]
        public string? NivelSeveridad { get; set; }

        [Display(Name = "Fecha de Diagnóstico")]
        [DataType(DataType.Date)]
        public DateTime? FechaDiagnostico { get; set; }

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