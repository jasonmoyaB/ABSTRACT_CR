using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class PlanNutricional
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del plan es obligatorio")]
        [Display(Name = "Nombre del Plan")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        [StringLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres")]
        public string? Descripcion { get; set; }

        [Display(Name = "Tipo de Plan")]
        public string TipoPlan { get; set; } = string.Empty; // "PDF", "Imagen", "Formulario"

        [Display(Name = "Archivo del Plan")]
        public string? RutaArchivo { get; set; }

        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; } = DateTime.Now;

        [Display(Name = "Fecha de Vencimiento")]
        [DataType(DataType.Date)]
        public DateTime FechaVencimiento { get; set; }

        [Display(Name = "Calorías Diarias")]
        [Range(800, 5000, ErrorMessage = "Las calorías deben estar entre 800 y 5000")]
        public int? CaloriasDiarias { get; set; }

        [Display(Name = "Proteínas (g)")]
        [Range(0, 500, ErrorMessage = "Las proteínas deben estar entre 0 y 500g")]
        public decimal? Proteinas { get; set; }

        [Display(Name = "Carbohidratos (g)")]
        [Range(0, 1000, ErrorMessage = "Los carbohidratos deben estar entre 0 y 1000g")]
        public decimal? Carbohidratos { get; set; }

        [Display(Name = "Grasas (g)")]
        [Range(0, 200, ErrorMessage = "Las grasas deben estar entre 0 y 200g")]
        public decimal? Grasas { get; set; }

        [Display(Name = "Fibra (g)")]
        [Range(0, 100, ErrorMessage = "La fibra debe estar entre 0 y 100g")]
        public decimal? Fibra { get; set; }

        [Display(Name = "Estado del Plan")]
        public string Estado { get; set; } = "Activo"; // "Activo", "Vencido", "Pausado"

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Relación con Usuario
        public int UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; } = null!;

        // Propiedades de navegación
        public virtual ICollection<MenuPersonalizado> MenusPersonalizados { get; set; } = new List<MenuPersonalizado>();
        public virtual ICollection<EvaluacionPlan> Evaluaciones { get; set; } = new List<EvaluacionPlan>();

        // Propiedades de auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
    }
} 