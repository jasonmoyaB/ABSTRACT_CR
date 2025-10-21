namespace Abstract_CR.Models.ViewModels
{
    public class PedidoHistorialViewModel
    {
        public int PedidoId { get; set; }
        public DateTime FechaPedido { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string DireccionEntrega { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int CantidadProductos { get; set; }
    }
}
