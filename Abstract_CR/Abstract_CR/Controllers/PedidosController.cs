using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstract_CR.Filters;
using Abstract_CR.Models.ViewModels;
using Abstract_CR.Services.Pedidos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Abstract_CR.Controllers
{
    [SessionAuthorize("Administrador", "Cliente")]
    public class PedidosController : Controller
    {
        private readonly IPedidoService _pedidoService;
        private readonly ILogger<PedidosController> _logger;

        public PedidosController(IPedidoService pedidoService, ILogger<PedidosController> logger)
        {
            _pedidoService = pedidoService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId is null)
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            var pedidos = await _pedidoService.ObtenerPedidosPorUsuarioAsync(usuarioId.Value, cancellationToken);
            return View(pedidos);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId is null)
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            var model = new PedidoFormViewModel
            {
                UsuarioId = usuarioId.Value
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PedidoFormViewModel model, CancellationToken cancellationToken)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId is null)
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            if (!ModelState.IsValid)
            {
                model.UsuarioId = usuarioId.Value;
                return View(model);
            }

            var pedido = new CrearPedidoDto
            {
                UsuarioId = usuarioId.Value,
                MetodoPago = model.MetodoPago,
                DireccionEnvio = model.DireccionEnvio,
                Detalles = model.Detalles.Select(detalle => new CrearPedidoDetalleDto
                {
                    Descripcion = detalle.Descripcion,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario
                }).ToList()
            };

            try
            {
                await _pedidoService.CrearPedidoAsync(pedido, cancellationToken);
                TempData["Success"] = "Pedido creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el pedido para el usuario {UsuarioId}.", usuarioId.Value);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el pedido. Intenta nuevamente más tarde.");
                model.UsuarioId = usuarioId.Value;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId is null)
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            var pedido = await _pedidoService.ObtenerPedidoPorIdAsync(id, usuarioId.Value, cancellationToken);
            if (pedido is null)
            {
                return NotFound();
            }

            return View(pedido);
        }
    }
}
