using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abstract_CR.Models
{
    public class PedidoDetalle
    {
        [Key]
        public int PedidoDetalleID { get; set; }

        [Required]
        public int PedidoID { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Producto")]
        public string NombreProducto { get; set; } = string.Empty;

        [Range(1, 100)]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }

        public virtual Pedido? Pedido { get; set; }
    }
}
