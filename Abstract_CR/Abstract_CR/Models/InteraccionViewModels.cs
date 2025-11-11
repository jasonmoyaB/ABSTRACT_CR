using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class InteraccionAdminViewModel
    {
        public List<UsuarioInteraccionResumen> Usuarios { get; set; } = new();
        public Usuario? UsuarioSeleccionado { get; set; }
        public List<MensajeInteraccion> Mensajes { get; set; } = new();
        public List<PuntosUsuario> HistorialPuntos { get; set; } = new();
        public MensajeInteraccionInputModel NuevoMensaje { get; set; } = new();
        public MensajeInteraccionInputModel NuevoResumen { get; set; } = new();
        public AsignacionPuntosInputModel AsignacionPuntos { get; set; } = new();
    }

    public class UsuarioInteraccionResumen
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public int PuntosTotales { get; set; }
        public DateTime? UltimaActividad { get; set; }
    }

    public class MensajeInteraccionInputModel
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El mensaje es obligatorio")]
        [StringLength(1000, ErrorMessage = "El mensaje no puede exceder los 1000 caracteres")]
        public string Contenido { get; set; } = string.Empty;

        public TipoMensajeInteraccion Tipo { get; set; } = TipoMensajeInteraccion.Mensaje;
    }

    public class AsignacionPuntosInputModel
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "Debe ingresar los puntos a asignar")]
        [Range(-1000, 1000, ErrorMessage = "El ajuste debe estar entre -1000 y 1000 puntos")]
        public int Puntos { get; set; }

        [StringLength(250, ErrorMessage = "El motivo no puede exceder los 250 caracteres")]
        public string? Motivo { get; set; }
    }

    public class PerfilInteraccionViewModel
    {
        public Usuario Usuario { get; set; } = null!;
        public List<MensajeInteraccion> Mensajes { get; set; } = new();
        public List<PuntosUsuario> HistorialPuntos { get; set; } = new();
        public MensajeInteraccionInputModel NuevoMensaje { get; set; } = new();
    }
}

