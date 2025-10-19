using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Abstract_CR.Services.Pedidos
{
    public interface IPedidoService
    {
        Task<PedidoDto> CrearPedidoAsync(CrearPedidoDto pedido, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PedidoDto>> ObtenerPedidosPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);

        Task<bool> ActualizarEstadoAsync(ActualizarEstadoPedidoDto pedidoEstado, CancellationToken cancellationToken = default);
    }
}
