namespace Abstract_CR.Models
{
    public class Suscripcion
    {
        public int SuscripcionID { get; set; }

        public int UsuarioID { get; set; }

        public string Estado { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public DateTime? UltimaFacturacion { get; set; }

        public DateTime? ProximaFacturacion { get; set; }
    }
}
