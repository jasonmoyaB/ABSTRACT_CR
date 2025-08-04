using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class MenuPersonalizado
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del menú es obligatorio")]
        [Display(Name = "Nombre del Menú")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        [StringLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres")]
        public string? Descripcion { get; set; }

        [Display(Name = "Tipo de Comida")]
        public string TipoComida { get; set; } = string.Empty; // "Desayuno", "Almuerzo", "Cena", "Snack"

        [Display(Name = "Día de la Semana")]
        public string DiaSemana { get; set; } = string.Empty;

        [Display(Name = "Calorías")]
        [Range(0, 2000, ErrorMessage = "Las calorías deben estar entre 0 y 2000")]
        public int? Calorias { get; set; }

        [Display(Name = "Proteínas (g)")]
        [Range(0, 100, ErrorMessage = "Las proteínas deben estar entre 0 y 100g")]
        public decimal? Proteinas { get; set; }

        [Display(Name = "Carbohidratos (g)")]
        [Range(0, 200, ErrorMessage = "Los carbohidratos deben estar entre 0 y 200g")]
        public decimal? Carbohidratos { get; set; }

        [Display(Name = "Grasas (g)")]
        [Range(0, 50, ErrorMessage = "Las grasas deben estar entre 0 y 50g")]
        public decimal? Grasas { get; set; }

        [Display(Name = "Fibra (g)")]
        [Range(0, 30, ErrorMessage = "La fibra debe estar entre 0 y 30g")]
        public decimal? Fibra { get; set; }

        [Display(Name = "Tiempo de Preparación (minutos)")]
        [Range(0, 300, ErrorMessage = "El tiempo debe estar entre 0 y 300 minutos")]
        public int? TiempoPreparacion { get; set; }

        [Display(Name = "Dificultad")]
        public string? Dificultad { get; set; } // "Fácil", "Medio", "Difícil"

        [Display(Name = "Ingredientes")]
        public string? Ingredientes { get; set; }

        [Display(Name = "Instrucciones")]
        public string? Instrucciones { get; set; }

        [Display(Name = "Imagen del Menú")]
        public string? RutaImagen { get; set; }

        [Display(Name = "Generado Automáticamente")]
        public bool GeneradoAutomaticamente { get; set; } = true;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Relación con PlanNutricional
        public int PlanNutricionalId { get; set; }
        public virtual PlanNutricional PlanNutricional { get; set; } = null!;

        // Propiedades de auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
    }
} 