using Abstract_CR.Helpers;
using Abstract_CR.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Abstract_CR.Controllers
{
    public class AdministracionController : Controller
    {
        private readonly InteraccionHelper _interaccionHelper;
        private readonly ILogger<AdministracionController> _logger;

        public AdministracionController(InteraccionHelper interaccionHelper, ILogger<AdministracionController> logger)
        {
            _interaccionHelper = interaccionHelper;
            _logger = logger;
        }

        public IActionResult PanelAdministracion()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                // Si no es admin, lo sacamos
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Interaccion(int? usuarioId)
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            var usuarios = await _interaccionHelper.ObtenerUsuariosAsync();
            if (!usuarios.Any())
            {
                TempData["Mensaje"] = "Aún no hay usuarios para interactuar.";
                return View(new InteraccionAdminViewModel { Usuarios = usuarios });
            }

            var usuarioSeleccionId = usuarioId ?? usuarios.First().UsuarioId;

            var (usuarioSeleccionado, mensajes, historial) = await _interaccionHelper.ObtenerDetalleUsuarioAsync(usuarioSeleccionId);
            if (usuarioSeleccionado != null)
            {
                await _interaccionHelper.MarcarMensajesComoLeidosAsync(usuarioSeleccionado.UsuarioID, paraChef: true);
            }

            var viewModel = new InteraccionAdminViewModel
            {
                Usuarios = usuarios,
                UsuarioSeleccionado = usuarioSeleccionado,
                Mensajes = mensajes,
                HistorialPuntos = historial,
                NuevoMensaje = new MensajeInteraccionInputModel
                {
                    UsuarioId = usuarioSeleccionId,
                    Tipo = TipoMensajeInteraccion.Mensaje
                },
                NuevoResumen = new MensajeInteraccionInputModel
                {
                    UsuarioId = usuarioSeleccionId,
                    Tipo = TipoMensajeInteraccion.ResumenSemanal
                },
                AsignacionPuntos = new AsignacionPuntosInputModel
                {
                    UsuarioId = usuarioSeleccionId
                }
            };

            if (usuarioSeleccionado == null)
            {
                TempData["Error"] = "El usuario seleccionado no existe.";
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarMensajeAdmin(MensajeInteraccionInputModel model)
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor revisa la información del mensaje.";
                return RedirectToAction(nameof(Interaccion), new { usuarioId = model.UsuarioId });
            }

            var adminId = HttpContext.Session.GetInt32("UsuarioID");

            var (success, error) = await _interaccionHelper.RegistrarMensajeAsync(model.UsuarioId, model.Contenido, enviadoPorChef: true, model.Tipo, adminId);

            if (!success)
            {
                TempData["Error"] = error ?? "No se pudo enviar el mensaje.";
            }
            else
            {
                TempData["Mensaje"] = model.Tipo == TipoMensajeInteraccion.ResumenSemanal
                    ? "Resumen semanal enviado al usuario."
                    : "Mensaje enviado correctamente.";
            }

            return RedirectToAction(nameof(Interaccion), new { usuarioId = model.UsuarioId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarPuntos(AsignacionPuntosInputModel model)
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Debes indicar una cantidad de puntos válida.";
                return RedirectToAction(nameof(Interaccion), new { usuarioId = model.UsuarioId });
            }

            var adminId = HttpContext.Session.GetInt32("UsuarioID");
            var (success, error) = await _interaccionHelper.AsignarPuntosAsync(model.UsuarioId, model.Puntos, model.Motivo, adminId);

            if (!success)
            {
                TempData["Error"] = error ?? "No se pudo asignar los puntos.";
            }
            else
            {
                TempData["Mensaje"] = "Los puntos se registraron correctamente.";
            }

            return RedirectToAction(nameof(Interaccion), new { usuarioId = model.UsuarioId });
        }

        private bool EsAdmin()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "No tienes permisos para acceder a esta sección.";
                return false;
            }

            return true;
        }

    }
}
