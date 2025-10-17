using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abstract_CR.Models
{
    [Table("EbookEdicion", Schema = "dbo")] 
    public class EbookEdicion
    {
        [Key]
        public int EbookEdicionID { get; set; }

        [Required, StringLength(255)]
        [Display(Name = "Título de la HU")]
        public string TituloHU { get; set; }

        [Required]
        [Display(Name = "Escenario")]
        public int Escenario { get; set; }

        [Required, StringLength(255)]
        [Display(Name = "Criterio de Aceptación")]
        public string CriterioAceptacion { get; set; }

        [Required, StringLength(500)]
        [Display(Name = "Contexto")]
        public string Contexto { get; set; }

        [Required, StringLength(500)]
        [Display(Name = "Evento")]
        public string Evento { get; set; }

        [Required, StringLength(500)]
        [Display(Name = "Resultado")]
        public string Resultado { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}