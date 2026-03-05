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

            // Guardar el estado anterior de la cuenta
            var estabaActivo = usuario.Activo;
            var estaDesactivando = estabaActivo && !model.Activo;

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

            // Si el usuario desactivó su cuenta, cerrar sesión inmediatamente
            if (estaDesactivando)
            {
                _logger.LogInformation("Usuario {UsuarioID} desactivó su cuenta y se cerrará la sesión", usuario.UsuarioID);
                
                // Limpiar la sesión
                HttpContext.Session.Clear();
                
                // Mensaje para mostrar en login
                TempData["Info"] = "Tu cuenta ha sido desactivada exitosamente. Contacta a un administrador si deseas reactivarla.";
                
                return RedirectToAction("Login", "Autenticacion");
            }

            // Actualizar la sesión con los nuevos datos
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

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == usuarioId.Value);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Perfil));
            }

            if (!usuario.Activo)
            {
                TempData["Error"] = "No puedes enviar mensajes porque tu cuenta está inactiva. Contacta a un administrador.";
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

        // ===============================================================
        //            COMPROBANTES DE PAGO
        // ===============================================================
        [HttpGet]
        public IActionResult SubirComprobante()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null)
            {
                TempData["Error"] = "Debes iniciar sesión para subir un comprobante.";
                return RedirectToAction("Login", "Autenticacion");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirComprobante(ComprobantePago model, IFormFile? archivo)
        {
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
                if (usuarioId == null)
                {
                    TempData["Error"] = "Debes iniciar sesión para subir un comprobante.";
                    return RedirectToAction("Login", "Autenticacion");
                }

                if (archivo == null || archivo.Length == 0)
                {
                    ModelState.AddModelError("Archivo", "Debes seleccionar un archivo.");
                    return View(model);
                }

                // Validar archivo
                var validacion = ValidarArchivoComprobante(archivo);
                if (!validacion.esValido)
                {
                    ModelState.AddModelError("Archivo", validacion.mensaje);
                    return View(model);
                }

                // Guardar archivo
                var rutaArchivo = await GuardarArchivoComprobante(archivo, usuarioId.Value);

                // Guardar en base de datos
                var comprobante = new ComprobantePago
                {
                    UsuarioID = usuarioId.Value,
                    RutaArchivo = rutaArchivo,
                    NombreArchivoOriginal = archivo.FileName,
                    TipoArchivo = Path.GetExtension(archivo.FileName).ToLowerInvariant(),
                    FechaSubida = DateTime.Now,
                    Observaciones = model.Observaciones,
                    Estado = "Pendiente"
                };

                _context.ComprobantesPago.Add(comprobante);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Comprobante de pago subido correctamente. Será revisado por el administrador.";
                return RedirectToAction("Perfil");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir comprobante de pago");
                ModelState.AddModelError("", "Error al guardar el comprobante: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> VerComprobantes()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null)
            {
                TempData["Error"] = "Debes iniciar sesión para ver tus comprobantes.";
                return RedirectToAction("Login", "Autenticacion");
            }

            var comprobantes = await _context.ComprobantesPago
                .Where(c => c.UsuarioID == usuarioId.Value)
                .OrderByDescending(c => c.FechaSubida)
                .ToListAsync();

            return View(comprobantes);
        }

        private (bool esValido, string mensaje) ValidarArchivoComprobante(IFormFile archivo)
        {
            if (archivo.Length > 10 * 1024 * 1024)
                return (false, "El archivo es demasiado grande. El tamaño máximo es 10MB.");

            var extensionesPermitidas = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
                return (false, "Formato de archivo no válido. Solo se permiten archivos PDF, JPG, JPEG, PNG y GIF.");

            return (true, string.Empty);
        }

        private async Task<string> GuardarArchivoComprobante(IFormFile archivo, int usuarioId)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "comprobantes");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{usuarioId}_{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await archivo.CopyToAsync(fileStream);

            return $"/uploads/comprobantes/{fileName}";
        }

        // ===============================================================
        //          GESTIÓN DE USUARIOS (ADMIN/CHEF)
        // ===============================================================
        [HttpGet]
        public async Task<IActionResult> GestionUsuarios()
        {
            var rolId = HttpContext.Session.GetInt32("RolID");
            if (rolId == null || rolId != 1) // Solo administradores/chefs
            {
                TempData["Error"] = "No tienes permisos para acceder a esta sección.";
                return RedirectToAction("Index", "Home");
            }

            var usuarios = await _interaccionHelper.ObtenerUsuariosAsync();
            return View(usuarios);
        }

        // ===============================================================
        //          ACTIVAR/DESACTIVAR USUARIO
        // ===============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoUsuario(int usuarioId, bool activar)
        {
            var rolId = HttpContext.Session.GetInt32("RolID");
            if (rolId == null || rolId != 1) // Solo administradores
            {
                return Json(new { success = false, message = "No tienes permisos." });
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);
            if (usuario == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado." });
            }

            usuario.Activo = activar;
            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = $"Usuario {(activar ? "activado" : "desactivado")} correctamente.",
                nuevoEstado = activar 
            });
        }
    }
}
