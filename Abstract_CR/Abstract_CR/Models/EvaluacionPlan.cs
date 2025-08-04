using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class EvaluacionPlan
    {
        public int Id { get; set; }

        [Display(Name = "Calificación General")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int CalificacionGeneral { get; set; }

        [Display(Name = "Calificación de Sabor")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int? CalificacionSabor { get; set; }

        [Display(Name = "Calificación de Variedad")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int? CalificacionVariedad { get; set; }

        [Display(Name = "Calificación de Facilidad")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int? CalificacionFacilidad { get; set; }

        [Display(Name = "Calificación de Efectividad")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int? CalificacionEfectividad { get; set; }

        [Display(Name = "Comentarios")]
        [StringLength(1000, ErrorMessage = "Los comentarios no pueden tener más de 1000 caracteres")]
        public string? Comentarios { get; set; }

        [Display(Name = "Sugerencias")]
        [StringLength(1000, ErrorMessage = "Las sugerencias no pueden tener más de 1000 caracteres")]
        public string? Sugerencias { get; set; }

        [Display(Name = "¿Recomendarías el plan?")]
        public bool Recomendaria { get; set; } = false;

        [Display(Name = "¿Continuarías con el plan?")]
        public bool Continuaria { get; set; } = false;

        [Display(Name = "Dificultades Encontradas")]
        [StringLength(500, ErrorMessage = "Las dificultades no pueden tener más de 500 caracteres")]
        public string? Dificultades { get; set; }

        [Display(Name = "Beneficios Notados")]
        [StringLength(500, ErrorMessage = "Los beneficios no pueden tener más de 500 caracteres")]
        public string? Beneficios { get; set; }

        [Display(Name = "Fecha de Evaluación")]
        [DataType(DataType.Date)]
        public DateTime FechaEvaluacion { get; set; } = DateTime.Now;

        // Relación con PlanNutricional
        public int PlanNutricionalId { get; set; }
        public virtual PlanNutricional PlanNutricional { get; set; } = null!;

        // Propiedades de auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
    }
} 