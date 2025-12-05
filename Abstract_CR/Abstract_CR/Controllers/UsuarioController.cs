using Abstract_CR.Data;
using Abstract_CR.Helpers;
using Abstract_CR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Abstract_CR.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsuarioController> _logger;
        private readonly InteraccionHelper _interaccionHelper;

        public UsuarioController(ApplicationDbContext context, ILogger<UsuarioController> logger, InteraccionHelper interaccionHelper)
        {
            _context = context;
            _logger = logger;
            _interaccionHelper = interaccionHelper;
        }

        // ===============================================================
        //                PERFIL DEL USUARIO
        // ===============================================================
        public async Task<IActionResult> Perfil()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null)
                return RedirectToAction("Login");

            var (usuario, mensajes, historial) = await _interaccionHelper.ObtenerDetalleUsuarioAsync(usuarioId.Value);

            if (usuario == null)
                return NotFound();

            await _interaccionHelper.MarcarMensajesComoLeidosAsync(usuario.UsuarioID, paraChef: false);

            var viewModel = new PerfilInteraccionViewModel
            {
                Usuario = usuario,
                Mensajes = mensajes,
                HistorialPuntos = historial,
                NuevoMensaje = new MensajeInteraccionInputModel
                {
                    UsuarioId = usuario.UsuarioID
                }
            };

            return View(viewModel);
        }

        // ===============================================================
        //                EDITAR PERFIL
        // ===============================================================
        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null)
                return RedirectToAction("Login");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(Usuario model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    _logger.LogWarning("Error de validación: {Error}", error.ErrorMessage);

                return View(model);
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == model.UsuarioID);
            if (usuario == null)
                return NotFound();

            usuario.Nombre = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.CorreoElectronico = model.CorreoElectronico;
            usuario.Activo = model.Activo;

            if (model.RolID.HasValue)
                usuario.RolID = model.RolID.Value;

            if (!string.IsNullOrWhiteSpace(model.ContrasenaHash))
            {
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(model.ContrasenaHash));
                usuario.ContrasenaHash = Convert.ToBase64String(bytes);
            }

            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("NombreUsuario", usuario.NombreCompleto);
            HttpContext.Session.SetString("Email", usuario.CorreoElectronico);
            TempData["Mensaje"] = "Perfil actualizado correctamente";

            return RedirectToAction("Perfil");
        }

        // ===============================================================
        //              MENSAJES AL CHEF
        // ===============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarMensajeChef(MensajeInteraccionInputModel model)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null)
                return RedirectToAction("Login");

            if (usuarioId.Value != model.UsuarioId)
            {
                TempData["Error"] = "No tienes permisos para enviar mensajes por otro usuario.";
                return RedirectToAction(nameof(Perfil));
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor revisa el contenido del mensaje.";
                return RedirectToAction(nameof(Perfil));
            }

            var (success, error) = await _interaccionHelper.RegistrarMensajeAsync(
                model.UsuarioId,
                model.Contenido,
                enviadoPorChef: false,
                TipoMensajeInteraccion.Mensaje,
                usuarioId
            );

            TempData[success ? "Mensaje" : "Error"] =
                success ? "Tu mensaje fue enviado al chef." : error ?? "No se pudo enviar el mensaje.";

            return RedirectToAction(nameof(Perfil));
        }

        // ===============================================================
        //               EDITAR PERFIL ADMIN
        // ===============================================================
        [HttpGet]
        public async Task<IActionResult> EditarPerfilAdmin()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null)
                return RedirectToAction("Login", "Autenticacion");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfilAdmin(Usuario model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    _logger.LogWarning("Error de validación: {Error}", error.ErrorMessage);

                return View(model);
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == model.UsuarioID);
            if (usuario == null)
                return NotFound();

            usuario.Nombre = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.CorreoElectronico = model.CorreoElectronico;
            usuario.Activo = model.Activo;

            if (model.RolID.HasValue)
                usuario.RolID = model.RolID.Value;

            if (!string.IsNullOrWhiteSpace(model.ContrasenaHash))
            {
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(model.ContrasenaHash));
                usuario.ContrasenaHash = Convert.ToBase64String(bytes);
            }

            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Perfil actualizado correctamente";

            return RedirectToAction("PanelAdministracion", "Administracion");
        }

        [HttpGet("Usuario/Restricciones")]
        public async Task<IActionResult> Restricciones()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID") ?? 0;
            if (usuarioId <= 0)
            {
                _logger.LogError("UsuarioID en sesión inválido: {UsuarioID}", usuarioId);
                return BadRequest("Usuario no válido.");
            }

            var lista = await _context.RestriccionesAlimentarias
                                      .Where(r => r.UsuarioID == usuarioId)
                                      .ToListAsync();

            return View(lista);
        }

        // ===============================================================
        // AGREGAR RESTRICCIÓN (GET)
        // ===============================================================
        public IActionResult AgregarRestriccion()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID") ?? 0;
            if (usuarioId <= 0)
            {
                _logger.LogError("UsuarioID en sesión inválido: {UsuarioID}", usuarioId);
                return BadRequest("Usuario no válido.");
            }

            var model = new RestriccionAlimentaria
            {
                UsuarioID = usuarioId
            };

            return View(model);
        }

        // ===============================================================
        // AGREGAR RESTRICCIÓN (POST)
        // ===============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarRestriccion(RestriccionAlimentaria restriccion)
        {
            // Asegurarse que el UsuarioID venga de la sesión
            var usuarioIdSesion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;
            if (usuarioIdSesion <= 0)
            {
                _logger.LogError("UsuarioID en sesión inválido en POST: {UsuarioID}", usuarioIdSesion);
                return BadRequest("Usuario no válido.");
            }

            restriccion.UsuarioID = usuarioIdSesion;

            if (!ModelState.IsValid)
                return View(restriccion);

            _context.RestriccionesAlimentarias.Add(restriccion);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Restricción agregada correctamente";
            return RedirectToAction("Restricciones", new { id = restriccion.UsuarioID });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarRestriccion(int id)
        {
            var restriccion = await _context.RestriccionesAlimentarias.FindAsync(id);
            if (restriccion == null)
            {
                TempData["Error"] = "La restricción no existe o ya fue eliminada.";
                return RedirectToAction("Restricciones");
            }
            if (restriccion == null)
            {
                TempData["Error"] = "No se encontró la restricción.";
                return RedirectToAction("Restricciones");
            }

            var usuarioIdSesion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;
            if (restriccion.UsuarioID != usuarioIdSesion)
            {
                TempData["Error"] = "No tienes permisos para eliminar esta restricción.";
                return RedirectToAction("Restricciones");
            }

            _context.RestriccionesAlimentarias.Remove(restriccion);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Restricción eliminada correctamente.";
            return RedirectToAction("Restricciones");
        }

        // ===============================================================
        //            PREFERENCIAS NUTRICIONALES
        // ===============================================================
        public IActionResult Preferencias()
        {
            var preferencias = ObtenerPreferenciasEjemplo();
            return View(preferencias);
        }

        public IActionResult AgregarPreferencia()
        {
            return View(new PreferenciaNutricional());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarPreferencia(PreferenciaNutricional preferencia)
        {
            if (!ModelState.IsValid)
                return View(preferencia);

            preferencia.UsuarioId = 1; // TODO: reemplazar por usuario real
            TempData["Mensaje"] = "Preferencia agregada correctamente";

            return RedirectToAction(nameof(Preferencias));
        }

        // ===============================================================
        //     MÉTODOS DE EJEMPLO (NO BASE DE DATOS)
        // ===============================================================
        private List<PreferenciaNutricional> ObtenerPreferenciasEjemplo()
        {
            return new List<PreferenciaNutricional>
            {
                new PreferenciaNutricional
                {
                    Id = 1,
                    Nombre = "Proteínas magras",
                    Descripcion = "Preferencia por carnes magras y pescado",
                    Categoria = "Proteínas",
                    Valor = "Alto",
                    Prioridad = 4,
                    Activa = true,
                    UsuarioId = 1
                },
                new PreferenciaNutricional
                {
                    Id = 2,
                    Nombre = "Vegetales orgánicos",
                    Descripcion = "Preferencia por vegetales cultivados orgánicamente",
                    Categoria = "Vegetales",
                    Valor = "Orgánico",
                    Prioridad = 3,
                    Activa = true,
                    UsuarioId = 1
                }
            };
        }
    }
}
