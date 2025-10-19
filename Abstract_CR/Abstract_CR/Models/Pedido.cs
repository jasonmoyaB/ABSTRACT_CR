using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class Pedido
    {
        [Key]
        public int PedidoID { get; set; }

        [Required]
        [Display(Name = "Usuario")]
        public int UsuarioID { get; set; }

        [Required]
        [Display(Name = "Fecha del Pedido")]
        [DataType(DataType.DateTime)]
        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Total")]
        [Range(typeof(decimal), "0", "999999.99", ErrorMessage = "El total debe ser un valor positivo")]
        [DataType(DataType.Currency)]
        public decimal Total { get; set; }

        [Required]
        [Display(Name = "Estado")]
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

        [Required]
        [Display(Name = "Método de Pago")]
        public MetodoPago MetodoPago { get; set; } = MetodoPago.TarjetaCredito;

        [Display(Name = "Dirección de Envío")]
        [StringLength(250)]
        public string? DireccionEnvio { get; set; }

        public virtual Usuario? Usuario { get; set; }

        public virtual ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
    }

    public enum EstadoPedido
    {
        Pendiente,
        Procesando,
        Enviado,
        Entregado,
        Cancelado
    }

    public enum MetodoPago
    {
        TarjetaCredito,
        TarjetaDebito,
        TransferenciaBancaria,
        Efectivo
    }
}
