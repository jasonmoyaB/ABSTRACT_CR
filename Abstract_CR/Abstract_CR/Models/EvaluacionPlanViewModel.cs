using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class EvaluacionPlanViewModel
    {
        
        public bool TienePlanActivo { get; set; } = false;

       
        public int? PlanID { get; set; }
        public string? NombrePlanActual { get; set; }

        
        [Required(ErrorMessage = "Selecciona una calificación.")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5.")]
        public int? Calificacion { get; set; }

        [Required(ErrorMessage = "Déjanos tu comentario.")]
        [StringLength(1000, ErrorMessage = "Máximo 1000 caracteres.")]
        public string? Comentario { get; set; }

        public string? ErrorMensaje { get; set; }    
        public string? SuccessMensaje { get; set; }  
    }
}
