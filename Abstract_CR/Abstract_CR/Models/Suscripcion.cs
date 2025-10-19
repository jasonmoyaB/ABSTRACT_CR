using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class Suscripcion
    {
        [Key]
        public int SuscripcionID { get; set; }

        [Required]
        [Display(Name = "Usuario")]
        public int UsuarioID { get; set; }

        [Required]
        [Display(Name = "Estado")]
        [StringLength(30)]
        public string Estado { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [Display(Name = "Fecha de Fin")]
        [DataType(DataType.Date)]
        public DateTime? FechaFin { get; set; }

        [Display(Name = "Última Facturación")]
        [DataType(DataType.Date)]
        public DateTime? UltimaFacturacion { get; set; }

        [Display(Name = "Próxima Facturación")]
        [DataType(DataType.Date)]
        public DateTime? ProximaFacturacion { get; set; }

        // Propiedades calculadas
        [Display(Name = "Estado Formateado")]
        public string EstadoFormateado
        {
            get
            {
                return Estado switch
                {
                    "Activa" => "Activa",
                    "Pausada" => "Pausada",
                    "Cancelada" => "Cancelada",
                    _ => Estado
                };
            }
        }

        [Display(Name = "Días Restantes")]
        public int? DiasRestantes
        {
            get
            {
                if (FechaFin.HasValue && Estado == "Activa")
                {
                    var dias = (FechaFin.Value - DateTime.Now.Date).Days;
                    return dias > 0 ? dias : 0;
                }
                return null;
            }
        }

        [Display(Name = "Vence Pronto")]
        public bool VencePronto => DiasRestantes.HasValue && DiasRestantes.Value <= 7;

        // Navegación
        public virtual Usuario? Usuario { get; set; }
    }
}