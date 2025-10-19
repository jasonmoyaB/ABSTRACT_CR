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
        public string? Documento { get; set; }

        // Nuevas propiedades para control de descarga y manejo de archivos
        [Display(Name = "Permitir Descarga")]
        public bool PermitirDescarga { get; set; } = true;

        [Display(Name = "Nombre del Archivo")]
        public string? NombreArchivo { get; set; }

        [Display(Name = "Ruta del Archivo")]
        public string? RutaArchivo { get; set; }

        [Display(Name = "Tamaño del Archivo")]
        public long? TamañoArchivo { get; set; }

        [Display(Name = "Tipo MIME")]
        public string? TipoMime { get; set; }

        [Display(Name = "Fecha de Subida")]
        public DateTime? FechaSubida { get; set; }

        // Propiedad calculada para la URL completa del archivo
        [Display(Name = "URL de Descarga")]
        public string UrlCompleta => !string.IsNullOrEmpty(RutaArchivo) ? $"/uploads/eBooks/{RutaArchivo}" : "";

        // Propiedad calculada para mostrar el tamaño formateado
        [Display(Name = "Tamaño Formateado")]
        public string TamañoFormateado
        {
            get
            {
                if (!TamañoArchivo.HasValue) return "N/A";
                var bytes = TamañoArchivo.Value;
                if (bytes < 1024) return $"{bytes} B";
                if (bytes < 1024 * 1024) return $"{bytes / 1024:F1} KB";
                return $"{bytes / (1024 * 1024):F1} MB";
            }
        }
    }
}