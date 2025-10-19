using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abstract_CR.Models
{
    public class PedidoDetalle
    {
        [Key]
        public int PedidoDetalleID { get; set; }

        [Required]
        [Display(Name = "Pedido")]
        public int PedidoID { get; set; }

        [Required]
        [Display(Name = "Descripción del Producto")]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Range(typeof(decimal), "0", "999999.99", ErrorMessage = "El precio debe ser un valor positivo")]
        [DataType(DataType.Currency)]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }

        [NotMapped]
        [DataType(DataType.Currency)]
        [Display(Name = "Subtotal")]
        public decimal Subtotal => Cantidad * PrecioUnitario;

        public virtual Pedido? Pedido { get; set; }
    }
}
