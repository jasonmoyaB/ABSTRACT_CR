namespace Abstract_CR.Models
{
    public class DireccionUsuarioItem
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
    }

    public class DireccionesUsuariosViewModel
    {
        public List<DireccionUsuarioItem> Usuarios { get; set; } = new();
    }
}