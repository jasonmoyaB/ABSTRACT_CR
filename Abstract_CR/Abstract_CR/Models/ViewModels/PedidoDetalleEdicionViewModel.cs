using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models.ViewModels
{
    public class PedidoDetalleEdicionViewModel
    {
        public int? PedidoDetalleID { get; set; }

        [Required]
        [Display(Name = "Producto")]
        public string NombreProducto { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "La cantidad debe estar entre 0 y 100.")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Display(Name = "Precio Unitario")]
        [Range(typeof(decimal), "0", "999999", ErrorMessage = "El precio debe ser positivo.")]
        public decimal PrecioUnitario { get; set; }

        public bool Disponible { get; set; } = true;

        public List<string> Sugerencias { get; set; } = new();
    }
}
