using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abstract_CR.Models
{
    public class PuntosUsuario
    {
        [Key]
        public int PuntosUsuarioId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "Debe indicar la cantidad de puntos")]
        [Range(-1000, 1000, ErrorMessage = "El ajuste de puntos debe estar entre -1000 y 1000")]
        public int Puntos { get; set; }

        [StringLength(250, ErrorMessage = "El motivo no puede exceder los 250 caracteres")]
        public string? Motivo { get; set; }

        [Display(Name = "Fecha de Asignación")]
        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        [Display(Name = "Asignado por")]
        public int? AsignadoPorId { get; set; }

        [NotMapped]
        public bool EsAjuste => Puntos < 0;

        public virtual Usuario Usuario { get; set; } = null!;
        public virtual Usuario? AsignadoPor { get; set; }
    }
}

