using System.Collections.Generic;
using System.Threading.Tasks;
using Abstract_CR.Models;

namespace Abstract_CR.Services
{
    public interface IPedidoService
    {
        Task<IReadOnlyList<Pedido>> ObtenerPedidosPorUsuarioAsync(int usuarioId);
        Task<Pedido?> ObtenerPedidoPorIdAsync(int pedidoId, int usuarioId);
        Task<Pedido> CrearPedidoAsync(Pedido pedido);
    }
}
