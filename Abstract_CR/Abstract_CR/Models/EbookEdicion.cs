using System;
using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{


    public class EbookEdicion
    {
        [Key]
        public int EbookEdicionID { get; set; }

        [Required(ErrorMessage = "El título de la historia de usuario es obligatorio.")]
        [StringLength(255)]
        [Display(Name = "Título de la HU")]
        public string TituloHU { get; set; }

        [Required(ErrorMessage = "Debe indicar el número de escenario.")]
        [Display(Name = "Escenario")]
        public int Escenario { get; set; }

        [Required(ErrorMessage = "Debe indicar el criterio de aceptación.")]
        [StringLength(255)]
        [Display(Name = "Criterio de Aceptación")]
        public string CriterioAceptacion { get; set; }

        [Required(ErrorMessage = "Debe ingresar el contexto.")]
        [StringLength(500)]
        [Display(Name = "Contexto")]
        public string Contexto { get; set; }

        [Required(ErrorMessage = "Debe ingresar el evento.")]
        [StringLength(500)]
        [Display(Name = "Evento")]
        public string Evento { get; set; }

        [Required(ErrorMessage = "Debe ingresar el resultado esperado.")]
        [StringLength(500)]
        [Display(Name = "Resultado")]
        public string Resultado { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
