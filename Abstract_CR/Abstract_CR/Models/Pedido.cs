using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abstract_CR.Models
{
    public class Pedido
    {
        [Key]
        public int PedidoID { get; set; }

        [Required]
        public int UsuarioID { get; set; }

        [Required]
        [Display(Name = "Fecha del Pedido")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de Entrega")]
        public DateTime? FechaEntrega { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Dirección de Entrega")]
        public string DireccionEntrega { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente";

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total")]
        public decimal Total { get; set; }

        public virtual Usuario? Usuario { get; set; }

        public virtual ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
    }
}
