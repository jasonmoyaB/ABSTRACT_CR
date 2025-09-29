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
        public string Token { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; }
    }
}
