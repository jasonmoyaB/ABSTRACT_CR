using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abstract_CR.Models
{
    [Table("MenusSemanales")]
    public class MenuSemanal
    {
        public int MenuSemanalID { get; set; }

        public int TipoMenuID { get; set; }

        public DateTime SemanaDel { get; set; }

        [Required(ErrorMessage = "El nombre del platillo es obligatorio")]
        [Display(Name = "Nombre del Platillo")]
        [StringLength(200, ErrorMessage = "El nombre no puede tener más de 200 caracteres")]
        public string NombrePlatillo { get; set; } = string.Empty;

        [Display(Name = "Día de la Semana")]
        [Required(ErrorMessage = "El día de la semana es obligatorio")]
        [StringLength(20)]
        public string DiaSemana { get; set; } = string.Empty;

        [Display(Name = "Características Principales")]
        public string? Caracteristicas { get; set; } // JSON o string separado por comas: "Sin gluten, Alto en proteína, Deslactosado"

        [Display(Name = "Ingredientes Principales")]
        public string? IngredientesPrincipales { get; set; } // JSON o string separado por comas

        [Display(Name = "Tip del Chef")]
        [StringLength(500, ErrorMessage = "El tip no puede tener más de 500 caracteres")]
        public string? TipChef { get; set; }

        [Display(Name = "Ruta de Imagen")]
        public string? RutaImagen { get; set; }

        [Display(Name = "Descripción")]
        [StringLength(1000, ErrorMessage = "La descripción no puede tener más de 1000 caracteres")]
        public string? Descripcion { get; set; }

        // Campo existente de la tabla MenusSemanales (se usa Descripcion de la tabla)
        public byte[]? RowVer { get; set; }
    }

    public class MenuSemanalViewModel
    {
        public int? MenuSemanalID { get; set; }
        public string NombrePlatillo { get; set; } = string.Empty;
        public string DiaSemana { get; set; } = string.Empty;
        public string DiaSemanaLimpio { get; set; } = string.Empty; // Sin acentos
        public List<string> Caracteristicas { get; set; } = new List<string>();
        public List<string> IngredientesPrincipales { get; set; } = new List<string>();
        public string? TipChef { get; set; }
        public string? RutaImagen { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
    }
}

