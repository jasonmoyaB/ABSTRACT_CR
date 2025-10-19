using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstract_CR.Data;
using Abstract_CR.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Abstract_CR.Services.Pedidos
{
    public class PedidoService : IPedidoService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<PedidoService> _logger;

        public PedidoService(ApplicationDbContext dbContext, ILogger<PedidoService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<PedidoDto> CrearPedidoAsync(CrearPedidoDto pedido, CancellationToken cancellationToken = default)
        {
            if (pedido is null)
            {
                throw new ArgumentNullException(nameof(pedido));
            }

            if (pedido.Detalles is null || !pedido.Detalles.Any())
            {
                throw new ArgumentException("El pedido debe contener al menos un detalle.", nameof(pedido));
            }

            var usuarioExiste = await _dbContext.Usuarios
                .AsNoTracking()
                .AnyAsync(u => u.UsuarioID == pedido.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new InvalidOperationException($"No existe un usuario con ID {pedido.UsuarioId}.");
            }

            var pedidoEntidad = new Pedido
            {
                UsuarioID = pedido.UsuarioId,
                MetodoPago = pedido.MetodoPago,
                DireccionEnvio = string.IsNullOrWhiteSpace(pedido.DireccionEnvio) ? null : pedido.DireccionEnvio.Trim(),
                Estado = EstadoPedido.Pendiente,
                FechaPedido = DateTime.UtcNow
            };

            foreach (var detalleDto in pedido.Detalles)
            {
                if (detalleDto.Cantidad <= 0)
                {
                    throw new ArgumentException("La cantidad de cada detalle debe ser mayor a cero.", nameof(pedido));
                }

                if (detalleDto.PrecioUnitario < 0)
                {
                    throw new ArgumentException("El precio unitario no puede ser negativo.", nameof(pedido));
                }

                var detalle = new PedidoDetalle
                {
                    Descripcion = detalleDto.Descripcion?.Trim() ?? string.Empty,
                    Cantidad = detalleDto.Cantidad,
                    PrecioUnitario = detalleDto.PrecioUnitario
                };

                pedidoEntidad.Detalles.Add(detalle);
            }

            pedidoEntidad.Total = pedidoEntidad.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

            _dbContext.Pedidos.Add(pedidoEntidad);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Pedido {PedidoId} creado para el usuario {UsuarioId}.", pedidoEntidad.PedidoID, pedidoEntidad.UsuarioID);

            return MapToDto(pedidoEntidad);
        }

        public async Task<IReadOnlyList<PedidoDto>> ObtenerPedidosPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
        {
            var pedidos = await _dbContext.Pedidos
                .AsNoTracking()
                .Where(p => p.UsuarioID == usuarioId)
                .Include(p => p.Detalles)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync(cancellationToken);

            return pedidos.Select(MapToDto).ToList();
        }

        public async Task<bool> ActualizarEstadoAsync(ActualizarEstadoPedidoDto pedidoEstado, CancellationToken cancellationToken = default)
        {
            if (pedidoEstado is null)
            {
                throw new ArgumentNullException(nameof(pedidoEstado));
            }

            var pedido = await _dbContext.Pedidos
                .FirstOrDefaultAsync(p => p.PedidoID == pedidoEstado.PedidoId, cancellationToken);

            if (pedido is null)
            {
                _logger.LogWarning("No se encontró el pedido con ID {PedidoId} para actualizar su estado.", pedidoEstado.PedidoId);
                return false;
            }

            if (pedido.Estado == pedidoEstado.Estado)
            {
                return true;
            }

            pedido.Estado = pedidoEstado.Estado;
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Estado del pedido {PedidoId} actualizado a {Estado}.", pedido.PedidoID, pedido.Estado);
            return true;
        }

        private static PedidoDto MapToDto(Pedido pedido)
        {
            return new PedidoDto
            {
                PedidoId = pedido.PedidoID,
                UsuarioId = pedido.UsuarioID,
                FechaPedido = pedido.FechaPedido,
                Total = pedido.Total,
                Estado = pedido.Estado,
                MetodoPago = pedido.MetodoPago,
                DireccionEnvio = pedido.DireccionEnvio,
                Detalles = pedido.Detalles
                    .OrderBy(d => d.PedidoDetalleID)
                    .Select(d => new PedidoDetalleDto
                    {
                        PedidoDetalleId = d.PedidoDetalleID,
                        Descripcion = d.Descripcion,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario
                    })
                    .ToList()
            };
        }
    }
}
