using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class PassResetTokens
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioID { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Propiedad de navegación
        public virtual Usuario? Usuario { get; set; }
    }
}
