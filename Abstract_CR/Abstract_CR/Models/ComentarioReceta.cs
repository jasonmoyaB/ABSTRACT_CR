namespace Abstract_CR.Models
{
    public class ComentarioReceta
    {
        public int ComentarioID { get; set; }
        public int RecetaID { get; set; }
        public int UsuarioID { get; set; }
        public string Comentario { get; set; }
        public DateTime? FechaComentario { get; set; }
    }
}
