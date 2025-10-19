using System;
using System.Collections.Generic;
using Abstract_CR.Models;

namespace Abstract_CR.Services.Pedidos
{
    public class CrearPedidoDto
    {
        public int UsuarioId { get; set; }

        public MetodoPago MetodoPago { get; set; } = MetodoPago.TarjetaCredito;

        public string? DireccionEnvio { get; set; }

        public IEnumerable<CrearPedidoDetalleDto> Detalles { get; set; } = new List<CrearPedidoDetalleDto>();
    }

    public class CrearPedidoDetalleDto
    {
        public string Descripcion { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
    }

    public class PedidoDetalleDto
    {
        public int PedidoDetalleId { get; set; }

        public string Descripcion { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    public class PedidoDto
    {
        public int PedidoId { get; set; }

        public int UsuarioId { get; set; }

        public DateTime FechaPedido { get; set; }

        public decimal Total { get; set; }

        public EstadoPedido Estado { get; set; }

        public MetodoPago MetodoPago { get; set; }

        public string? DireccionEnvio { get; set; }

        public IReadOnlyCollection<PedidoDetalleDto> Detalles { get; set; } = Array.Empty<PedidoDetalleDto>();
    }

    public class ActualizarEstadoPedidoDto
    {
        public int PedidoId { get; set; }

        public EstadoPedido Estado { get; set; }
    }
}
