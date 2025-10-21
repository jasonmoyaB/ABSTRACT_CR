using System.Collections.Generic;
using System.Linq;
using Abstract_CR.Data;
using Abstract_CR.Models;
using Abstract_CR.Models.ViewModels;
using Abstract_CR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Abstract_CR.Controllers
{
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductoDisponibilidadService _disponibilidadService;

        public PedidosController(ApplicationDbContext context, IProductoDisponibilidadService disponibilidadService)
        {
            _context = context;
            _disponibilidadService = disponibilidadService;
        }

        public async Task<IActionResult> Historial()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            var pedidos = await _context.Pedidos
                .Where(p => p.UsuarioID == usuarioId.Value)
                .Include(p => p.Detalles)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            var viewModel = pedidos.Select(p => new PedidoHistorialViewModel
            {
                PedidoId = p.PedidoID,
                FechaPedido = p.FechaCreacion,
                FechaEntrega = p.FechaEntrega,
                DireccionEntrega = p.DireccionEntrega,
                Estado = p.Estado,
                Total = p.Total,
                CantidadProductos = p.Detalles.Sum(d => d.Cantidad)
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Repetir(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.PedidoID == id && p.UsuarioID == usuarioId.Value);

            if (pedido == null)
            {
                return NotFound();
            }

            var viewModel = new RepetirPedidoViewModel
            {
                PedidoAnteriorId = pedido.PedidoID,
                FechaEntrega = pedido.FechaEntrega ?? DateTime.Today.AddDays(1),
                DireccionEntrega = pedido.DireccionEntrega,
                Detalles = pedido.Detalles.Select(d => new PedidoDetalleEdicionViewModel
                {
                    PedidoDetalleID = d.PedidoDetalleID,
                    NombreProducto = d.NombreProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                }).ToList()
            };

            ActualizarDisponibilidad(viewModel);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Repetir(RepetirPedidoViewModel model)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            if (model.Detalles == null || model.Detalles.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "El pedido debe tener al menos un producto.");
            }

            if (!ModelState.IsValid)
            {
                ActualizarDisponibilidad(model);
                return View(model);
            }

            ActualizarDisponibilidad(model);

            if (model.TieneProductosNoDisponibles)
            {
                ModelState.AddModelError(string.Empty, "Algunos productos no están disponibles. Revisá las sugerencias.");
                return View(model);
            }

            var detallesValidos = model.Detalles.Where(d => d.Cantidad > 0).ToList();
            if (!detallesValidos.Any())
            {
                ModelState.AddModelError(string.Empty, "Debés seleccionar al menos un producto con cantidad mayor a cero.");
                return View(model);
            }

            var nuevaFechaEntrega = model.FechaEntrega ?? DateTime.Today.AddDays(1);

            var nuevoPedido = new Pedido
            {
                UsuarioID = usuarioId.Value,
                FechaCreacion = DateTime.UtcNow,
                FechaEntrega = nuevaFechaEntrega,
                DireccionEntrega = model.DireccionEntrega,
                Estado = "Pendiente"
            };

            foreach (var detalle in detallesValidos)
            {
                nuevoPedido.Detalles.Add(new PedidoDetalle
                {
                    NombreProducto = detalle.NombreProducto,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario
                });
            }

            nuevoPedido.Total = nuevoPedido.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

            _context.Pedidos.Add(nuevoPedido);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Confirmacion), new { id = nuevoPedido.PedidoID });
        }

        public async Task<IActionResult> Confirmacion(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.PedidoID == id && p.UsuarioID == usuarioId.Value);

            if (pedido == null)
            {
                return NotFound();
            }

            var viewModel = new PedidoConfirmacionViewModel
            {
                PedidoId = pedido.PedidoID,
                FechaPedido = pedido.FechaCreacion,
                FechaEntrega = pedido.FechaEntrega,
                DireccionEntrega = pedido.DireccionEntrega,
                Total = pedido.Total,
                CantidadProductos = pedido.Detalles.Sum(d => d.Cantidad)
            };

            return View(viewModel);
        }

        private void ActualizarDisponibilidad(RepetirPedidoViewModel model)
        {
            if (model.Detalles == null || model.Detalles.Count == 0)
            {
                model.TieneProductosNoDisponibles = false;
                return;
            }

            var disponibilidad = _disponibilidadService.VerificarDisponibilidad(model.Detalles.Select(d => d.NombreProducto));

            bool hayNoDisponibles = false;
            foreach (var detalle in model.Detalles)
            {
                if (disponibilidad.TryGetValue(detalle.NombreProducto, out var estado))
                {
                    detalle.Disponible = estado.Disponible;
                    detalle.Sugerencias = estado.Sugerencias;
                }
                else
                {
                    detalle.Disponible = false;
                    detalle.Sugerencias = new List<string>();
                }

                if (!detalle.Disponible)
                {
                    hayNoDisponibles = true;
                }
            }

            model.TieneProductosNoDisponibles = hayNoDisponibles;
        }
    }
}
