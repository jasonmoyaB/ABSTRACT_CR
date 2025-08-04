using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre Completo")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        public string? Telefono { get; set; }

        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [Display(Name = "Género")]
        public string? Genero { get; set; }

        [Display(Name = "Altura (cm)")]
        [Range(100, 250, ErrorMessage = "La altura debe estar entre 100 y 250 cm")]
        public int? Altura { get; set; }

        [Display(Name = "Peso (kg)")]
        [Range(30, 300, ErrorMessage = "El peso debe estar entre 30 y 300 kg")]
        public decimal? Peso { get; set; }

        [Display(Name = "Nivel de Actividad Física")]
        public string? NivelActividad { get; set; }

        [Display(Name = "Objetivo Nutricional")]
        public string? ObjetivoNutricional { get; set; }

        // Propiedades de navegación
        public virtual ICollection<Alergia> Alergias { get; set; } = new List<Alergia>();
        public virtual ICollection<Restriccion> Restricciones { get; set; } = new List<Restriccion>();
        public virtual ICollection<PreferenciaNutricional> PreferenciasNutricionales { get; set; } = new List<PreferenciaNutricional>();
        public virtual ICollection<PlanNutricional> PlanesNutricionales { get; set; } = new List<PlanNutricional>();

        // Propiedades de auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
    }
} 