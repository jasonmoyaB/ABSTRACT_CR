using Abstract_CR.Helpers;
using Abstract_CR.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        // ===============================================================
        // PANEL DE ADMINISTRACION
        // ===============================================================
        public IActionResult PanelAdministracion()
        {
            if (!EsAdmin())
                return RedirectToAction("Index", "Home");

            return View();
        }

        // ===============================================================
        // INTERACCION CON USUARIOS
        // ===============================================================
        [HttpGet]
        public async Task<IActionResult> Interaccion(int? usuarioId)
        {
            if (!EsAdmin())
                return RedirectToAction("Index", "Home");

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
                TempData["Error"] = "El usuario seleccionado no existe.";

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarMensajeAdmin(MensajeInteraccionInputModel model)
        {
            if (!EsAdmin())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor revisa la información del mensaje.";
                return RedirectToAction(nameof(Interaccion), new { usuarioId = model.UsuarioId });
            }

            var adminId = HttpContext.Session.GetInt32("UsuarioID");

            var (success, error) = await _interaccionHelper.RegistrarMensajeAsync(model.UsuarioId, model.Contenido, enviadoPorChef: true, model.Tipo, adminId);

            TempData[success ? "Mensaje" : "Error"] = success
                ? (model.Tipo == TipoMensajeInteraccion.ResumenSemanal
                    ? "Resumen semanal enviado al usuario."
                    : "Mensaje enviado correctamente.")
                : error ?? "No se pudo enviar el mensaje.";

            return RedirectToAction(nameof(Interaccion), new { usuarioId = model.UsuarioId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarPuntos(AsignacionPuntosInputModel model)
        {
            if (!EsAdmin())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Debes indicar una cantidad de puntos válida.";
                return RedirectToAction(nameof(Interaccion), new { usuarioId = model.UsuarioId });
            }

            var adminId = HttpContext.Session.GetInt32("UsuarioID");
            var (success, error) = await _interaccionHelper.AsignarPuntosAsync(model.UsuarioId, model.Puntos, model.Motivo, adminId);

            TempData[success ? "Mensaje" : "Error"] = success
                ? "Los puntos se registraron correctamente."
                : error ?? "No se pudo asignar los puntos.";

            return RedirectToAction(nameof(Interaccion), new { usuarioId = model.UsuarioId });
        }// ===============================================================
         // LISTAR RESTRICCIONES DE USUARIOS
         // ===============================================================
        [HttpGet]
        public async Task<IActionResult> RestriccionesUsuarios(int? usuarioId)
        {
            if (!EsAdmin())
                return RedirectToAction("Index", "Home");

            // Obtenemos todos los usuarios
            var usuarios = await _interaccionHelper.ObtenerUsuariosAsync();
            if (!usuarios.Any())
            {
                TempData["Mensaje"] = "No hay usuarios registrados.";
                return View(new RestriccionesUsuariosViewModel { Usuarios = usuarios });
            }

            // Seleccionamos el usuario por defecto si no se pasó ID
            var idSeleccionado = usuarioId ?? usuarios.First().UsuarioId;

            // Aquí se usan las restricciones desde tu contexto de EF (similar a UsuarioController)
            var restricciones = await _interaccionHelper.ObtenerRestriccionesPorUsuarioAsync(idSeleccionado);

            var viewModel = new RestriccionesUsuariosViewModel
            {
                Usuarios = usuarios,
                UsuarioSeleccionadoId = idSeleccionado,
                Restricciones = restricciones
            };

            return View(viewModel);
        }



        // ===============================================================
        // VERIFICAR ADMIN
        // ===============================================================
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
