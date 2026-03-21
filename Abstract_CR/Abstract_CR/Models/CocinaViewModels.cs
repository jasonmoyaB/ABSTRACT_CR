namespace Abstract_CR.Models
{
    /// <summary>
    /// Fila para la vista Cocina: clientes con suscripción activa, restricciones y dirección.
    /// </summary>
    public class CocinaClienteFilaViewModel
    {
        public int UsuarioID { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        /// <summary>Dirección de entrega (información básica del usuario).</summary>
        public string? DireccionEntrega { get; set; }
        /// <summary>Estado de la suscripción (ej. Activa).</summary>
        public string EstadoSuscripcion { get; set; } = string.Empty;
        /// <summary>Fin de vigencia de la suscripción.</summary>
        public DateTime? FechaFinSuscripcion { get; set; }
        /// <summary>Textos de restricciones alimentarias del usuario.</summary>
        public IReadOnlyList<string> Restricciones { get; set; } = Array.Empty<string>();
    }
}
