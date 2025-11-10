using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class MensajeInteraccion
    {
        [Key]
        public int MensajeInteraccionId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        public int? RemitenteId { get; set; }

        [Required]
        public bool EnviadoPorChef { get; set; }

        [Required(ErrorMessage = "El contenido del mensaje es obligatorio")]
        [StringLength(1000, ErrorMessage = "El mensaje no puede exceder los 1000 caracteres")]
        public string Contenido { get; set; } = string.Empty;

        [Required]
        public DateTime FechaEnvio { get; set; } = DateTime.Now;

        [Display(Name = "Tipo de Mensaje")]
        public TipoMensajeInteraccion Tipo { get; set; } = TipoMensajeInteraccion.Mensaje;

        public bool Leido { get; set; }

        public virtual Usuario Usuario { get; set; } = null!;
        public virtual Usuario? Remitente { get; set; }
    }

    public enum TipoMensajeInteraccion
    {
        Mensaje = 0,
        ResumenSemanal = 1
    }
}

