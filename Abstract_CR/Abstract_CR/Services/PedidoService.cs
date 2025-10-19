using System;
using System.Linq;
using Abstract_CR.Data;
using Abstract_CR.Models;
using Microsoft.EntityFrameworkCore;

namespace Abstract_CR.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly ApplicationDbContext _context;

        public PedidoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Pedido> CrearPedidoAsync(Pedido pedido)
        {
            if (pedido == null)
            {
                throw new ArgumentNullException(nameof(pedido));
            }

            pedido.FechaPedido = DateTime.UtcNow;
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            return pedido;
        }

        public async Task<Pedido?> ObtenerPedidoPorIdAsync(int pedidoId, int usuarioId)
        {
            return await _context.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.PedidoID == pedidoId && p.UsuarioID == usuarioId);
        }

        public async Task<IReadOnlyList<Pedido>> ObtenerPedidosPorUsuarioAsync(int usuarioId)
        {
            return await _context.Pedidos
                .Where(p => p.UsuarioID == usuarioId)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();
        }
    }
}
