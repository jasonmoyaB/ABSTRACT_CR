using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abstract_CR.Models
{
    public class ComprobantePago
    {
        [Key]
        public int ComprobanteID { get; set; }

        [Required]
        public int UsuarioID { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Ruta del Archivo")]
        public string RutaArchivo { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Nombre del Archivo Original")]
        public string? NombreArchivoOriginal { get; set; }

        [StringLength(50)]
        [Display(Name = "Tipo de Archivo")]
        public string? TipoArchivo { get; set; }

        [Display(Name = "Fecha de Subida")]
        public DateTime FechaSubida { get; set; } = DateTime.Now;

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [StringLength(50)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Aprobado, Rechazado

        // Propiedad de navegación
        public virtual Usuario? Usuario { get; set; }

        // Propiedad no mapeada para el archivo
        [NotMapped]
        [Display(Name = "Comprobante de Pago")]
        public IFormFile? Archivo { get; set; }
    }
}

