using System.Collections.Generic;

namespace Abstract_CR.Models
{
    public class RestriccionesUsuariosViewModel
    {
        public List<UsuarioInteraccionResumen> Usuarios { get; set; } = new List<UsuarioInteraccionResumen>();
        public int UsuarioSeleccionadoId { get; set; }
        public List<RestriccionAlimentaria> Restricciones { get; set; } = new List<RestriccionAlimentaria>();
    }
}
