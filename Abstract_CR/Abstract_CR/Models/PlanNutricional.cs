using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abstract_CR.Models
{
    public class PlanNutricional
    {
        [Key] // <-- Indica a EF Core que esta es la clave primaria
        public int PlanID { get; set; }

        public int UsuarioID { get; set; }

        [Required(ErrorMessage = "El nombre del plan es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre del Plan")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Descripcion { get; set; }

        [Display(Name = "Fecha de Carga")]
        public DateTime FechaCarga { get; set; } = DateTime.Now;

        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        public DateTime? FechaInicio { get; set; }

        [Display(Name = "Fecha de Vencimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaVencimiento { get; set; }

        [Display(Name = "URL del Documento")]
        [StringLength(255)]
        public string? DocumentoURL { get; set; }

        [Display(Name = "Tipo de Plan")]
        [Required(ErrorMessage = "El tipo de plan es requerido")]
        public string TipoPlan { get; set; }

        [Display(Name = "Calorías Diarias")]
        [Range(800, 5000, ErrorMessage = "Las calorías deben estar entre 800 y 5000")]
        public int? CaloriasDiarias { get; set; }

        [Display(Name = "Proteínas (g)")]
        [Range(0, 999, ErrorMessage = "Las proteínas deben ser un valor positivo")]
        public decimal? Proteinas { get; set; }

        [Display(Name = "Carbohidratos (g)")]
        [Range(0, 999, ErrorMessage = "Los carbohidratos deben ser un valor positivo")]
        public decimal? Carbohidratos { get; set; }

        [Display(Name = "Grasas (g)")]
        [Range(0, 999, ErrorMessage = "Las grasas deben ser un valor positivo")]
        public decimal? Grasas { get; set; }

        [Display(Name = "Fibra (g)")]
        [Range(0, 100, ErrorMessage = "La fibra debe ser un valor positivo")]
        public decimal? Fibra { get; set; }

        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El estado es requerido")]
        public string Estado { get; set; } = "Activo";

        // Propiedades de navegación
        public virtual Usuario? Usuario { get; set; }

        // Propiedad no mapeada para el archivo
        [NotMapped]
        [Display(Name = "Archivo del Plan")]
        public IFormFile? Archivo { get; set; }

        // Validación personalizada
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaVencimiento.HasValue && FechaInicio.HasValue)
            {
                if (FechaVencimiento <= FechaInicio)
                {
                    yield return new ValidationResult(
                        "La fecha de vencimiento debe ser posterior a la fecha de inicio.",
                        new[] { nameof(FechaVencimiento) });
                }
            }

            if (FechaInicio.HasValue && FechaInicio < DateTime.Today)
            {
                yield return new ValidationResult(
                    "La fecha de inicio no puede ser anterior al día de hoy.",
                    new[] { nameof(FechaInicio) });
            }
        }
    }
}
