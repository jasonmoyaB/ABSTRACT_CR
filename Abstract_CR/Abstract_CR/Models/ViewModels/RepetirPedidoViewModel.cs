using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models.ViewModels
{
    public class RepetirPedidoViewModel
    {
        [Required]
        public int PedidoAnteriorId { get; set; }

        [Display(Name = "Fecha de Entrega")]
        [DataType(DataType.Date)]
        public DateTime? FechaEntrega { get; set; }

        [Required(ErrorMessage = "La dirección de entrega es obligatoria.")]
        [StringLength(200)]
        [Display(Name = "Dirección de Entrega")]
        public string DireccionEntrega { get; set; } = string.Empty;

        public bool TieneProductosNoDisponibles { get; set; }

        public List<PedidoDetalleEdicionViewModel> Detalles { get; set; } = new();
    }
}
