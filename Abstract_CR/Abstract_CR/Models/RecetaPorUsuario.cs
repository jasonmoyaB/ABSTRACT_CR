using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class RecetaPorUsuario
    {
        [Key]
        public int RecetaPorUsuarioID { get; set; }
        public string Dia { get; set; }
        public int RecetaID { get; set; }
        public int UsuarioID { get; set; }
    }
}
