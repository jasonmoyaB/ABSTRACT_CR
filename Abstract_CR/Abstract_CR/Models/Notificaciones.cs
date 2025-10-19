namespace Abstract_CR.Models
{
    public class Notificacion
    {
        public int NotificacionID { get; set; }
        public int UsuarioID { get; set; }
        public string Tipo { get; set; }
        public string Mensaje { get; set; }
        public DateTime FechaEnvio { get; set; }
    }
}
