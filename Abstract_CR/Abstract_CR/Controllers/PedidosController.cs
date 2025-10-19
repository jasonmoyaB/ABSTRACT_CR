using System;
using Abstract_CR.Models;
using Abstract_CR.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Abstract_CR.Controllers
{
    public class PedidosController : Controller
    {
        private readonly IPedidoService _pedidoService;
        private readonly ISessionService _sessionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PedidosController(IPedidoService pedidoService, ISessionService sessionService, IHttpContextAccessor httpContextAccessor)
        {
            _pedidoService = pedidoService;
            _sessionService = sessionService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarioId = _sessionService.ObtenerUsuarioId();
            if (!usuarioId.HasValue)
            {
                TempData["Error"] = "Debes iniciar sesión para consultar tus pedidos.";
                return RedirectToLogin();
            }

            var pedidos = await _pedidoService.ObtenerPedidosPorUsuarioAsync(usuarioId.Value);
            return View(pedidos);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var usuarioId = _sessionService.ObtenerUsuarioId();
            if (!usuarioId.HasValue)
            {
                TempData["Error"] = "Debes iniciar sesión para crear un pedido.";
                return RedirectToLogin();
            }

            var pedido = new Pedido
            {
                UsuarioID = usuarioId.Value,
                FechaPedido = DateTime.UtcNow
            };

            return View(pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pedido pedido)
        {
            var usuarioId = _sessionService.ObtenerUsuarioId();
            if (!usuarioId.HasValue)
            {
                TempData["Error"] = "Debes iniciar sesión para crear un pedido.";
                return RedirectToLogin();
            }

            pedido.UsuarioID = usuarioId.Value;

            if (!ModelState.IsValid)
            {
                return View(pedido);
            }

            await _pedidoService.CrearPedidoAsync(pedido);
            TempData["Success"] = "Pedido creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var usuarioId = _sessionService.ObtenerUsuarioId();
            if (!usuarioId.HasValue)
            {
                TempData["Error"] = "Debes iniciar sesión para consultar los detalles del pedido.";
                return RedirectToLogin();
            }

            var pedido = await _pedidoService.ObtenerPedidoPorIdAsync(id, usuarioId.Value);
            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        private IActionResult RedirectToLogin()
        {
            var returnUrl = _httpContextAccessor.HttpContext?.Request?.Path.Value;
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                TempData["ReturnUrl"] = returnUrl;
            }

            return RedirectToAction("Login", "Autenticacion");
        }
    }
}
