using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abstract_CR.Models
{
    public class RestriccionAlimentaria
    {
        [Key]
        public int RestriccionID { get; set; }

        [Required]
        [ForeignKey("Usuario")]
        public int UsuarioID { get; set; }

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        public virtual Usuario? Usuario { get; set; }
    }
}
