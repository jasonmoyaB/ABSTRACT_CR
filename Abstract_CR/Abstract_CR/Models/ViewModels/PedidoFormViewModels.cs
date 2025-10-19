using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abstract_CR.Models;

namespace Abstract_CR.Models.ViewModels
{
    public class PedidoFormViewModel
    {
        [Required]
        [Display(Name = "Usuario")]
        public int UsuarioId { get; set; }

        [Display(Name = "Método de Pago")]
        public MetodoPago MetodoPago { get; set; } = MetodoPago.TarjetaCredito;

        [Display(Name = "Dirección de Envío")]
        [StringLength(250)]
        public string? DireccionEnvio { get; set; }

        public List<PedidoDetalleFormViewModel> Detalles { get; set; } = new() { new PedidoDetalleFormViewModel() };
    }

    public class PedidoDetalleFormViewModel
    {
        [Required]
        [Display(Name = "Descripción del Producto")]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; } = 1;

        [Required]
        [Range(typeof(decimal), "0", "999999.99", ErrorMessage = "El precio debe ser un valor positivo")]
        [DataType(DataType.Currency)]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; } = 0.00M;
    }
}
