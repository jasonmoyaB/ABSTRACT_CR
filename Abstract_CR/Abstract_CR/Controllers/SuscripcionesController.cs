using Abstract_CR.Data;
using Abstract_CR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Abstract_CR.Controllers
{
    public class SuscripcionesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SuscripcionesController> _logger;

        public SuscripcionesController(ApplicationDbContext context, ILogger<SuscripcionesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Suscripciones - Vista principal de gestión de suscripciones
        public async Task<IActionResult> Index()
        {
            // Verificar que el usuario sea administrador
            var rol = HttpContext.Session.GetString("Rol");
            if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home");
            }

            // Obtener todos los usuarios con sus suscripciones
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Suscripciones)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            // Obtener estadísticas de suscripciones
            var totalUsuarios = usuarios.Count;
            var usuariosConSuscripcion = usuarios.Count(u => u.Suscripciones.Any());
            var usuariosSinSuscripcion = totalUsuarios - usuariosConSuscripcion;
            var suscripcionesActivas = usuarios.Count(u => u.Suscripciones.Any(s => s.Estado == "Activa"));

            ViewBag.TotalUsuarios = totalUsuarios;
            ViewBag.UsuariosConSuscripcion = usuariosConSuscripcion;
            ViewBag.UsuariosSinSuscripcion = usuariosSinSuscripcion;
            ViewBag.SuscripcionesActivas = suscripcionesActivas;

            return View(usuarios);
        }

        // POST: Suscripciones/CrearSuscripcion - Crear suscripción para un usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearSuscripcion(int usuarioId, string estado = "Activa", int mesesDuracion = 1)
        {
            try
            {
                _logger.LogInformation($"Intentando crear suscripción para usuario {usuarioId} con estado {estado} y duración {mesesDuracion} meses");

                // Verificar que el usuario sea administrador
                var rol = HttpContext.Session.GetString("Rol");
                _logger.LogInformation($"Rol del usuario actual: {rol}");
                
                if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning($"Usuario no autorizado. Rol: {rol}");
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Verificar que el usuario existe
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);
                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario {usuarioId} no encontrado");
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                _logger.LogInformation($"Usuario encontrado: {usuario.NombreCompleto}");

                // Verificar si ya tiene suscripción
                var suscripcionExistente = await _context.Suscripciones
                    .FirstOrDefaultAsync(s => s.UsuarioID == usuarioId);

                if (suscripcionExistente != null)
                {
                    _logger.LogWarning($"Usuario {usuario.NombreCompleto} ya tiene suscripción con ID {suscripcionExistente.SuscripcionID}");
                    return Json(new { success = false, message = "El usuario ya tiene una suscripción activa" });
                }

                // Crear nueva suscripción
                var nuevaSuscripcion = new Suscripcion
                {
                    UsuarioID = usuarioId,
                    Estado = estado,
                    FechaInicio = DateTime.Now.Date,
                    FechaFin = DateTime.Now.Date.AddMonths(mesesDuracion),
                    UltimaFacturacion = DateTime.Now.Date,
                    ProximaFacturacion = DateTime.Now.Date.AddMonths(1)
                };

                _logger.LogInformation($"Creando nueva suscripción: UsuarioID={nuevaSuscripcion.UsuarioID}, Estado={nuevaSuscripcion.Estado}, FechaInicio={nuevaSuscripcion.FechaInicio}, FechaFin={nuevaSuscripcion.FechaFin}");

                _context.Suscripciones.Add(nuevaSuscripcion);
                var cambios = await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Cambios guardados en la base de datos: {cambios} registros afectados");

                var mensaje = $"Suscripción {estado} creada exitosamente para {usuario.NombreCompleto} por {mesesDuracion} mes";
                _logger.LogInformation($"Admin creó suscripción para usuario {usuario.NombreCompleto}");

                return Json(new { success = true, message = mensaje });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear suscripción");
                return Json(new { success = false, message = $"Error al crear la suscripción: {ex.Message}" });
            }
        }

        // POST: Suscripciones/CambiarEstado - Cambiar estado de suscripción
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int usuarioId, string nuevoEstado)
        {
            try
            {
                // Verificar que el usuario sea administrador
                var rol = HttpContext.Session.GetString("Rol");
                if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Buscar la suscripción del usuario
                var suscripcion = await _context.Suscripciones
                    .FirstOrDefaultAsync(s => s.UsuarioID == usuarioId);

                if (suscripcion == null)
                {
                    return Json(new { success = false, message = "El usuario no tiene suscripción" });
                }

                var estadoAnterior = suscripcion.Estado;
                suscripcion.Estado = nuevoEstado;
                await _context.SaveChangesAsync();

                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);
                var mensaje = $"Estado de suscripción cambiado de '{estadoAnterior}' a '{nuevoEstado}' para {usuario?.NombreCompleto}";
                
                _logger.LogInformation($"Admin cambió estado de suscripción para usuario {usuario?.NombreCompleto}");

                return Json(new { success = true, message = mensaje });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de suscripción");
                return Json(new { success = false, message = "Error al cambiar el estado de la suscripción" });
            }
        }

        // POST: Suscripciones/EliminarSuscripcion - Eliminar suscripción
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarSuscripcion(int usuarioId)
        {
            try
            {
                // Verificar que el usuario sea administrador
                var rol = HttpContext.Session.GetString("Rol");
                if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Buscar la suscripción del usuario
                var suscripcion = await _context.Suscripciones
                    .FirstOrDefaultAsync(s => s.UsuarioID == usuarioId);

                if (suscripcion == null)
                {
                    return Json(new { success = false, message = "El usuario no tiene suscripción" });
                }

                _context.Suscripciones.Remove(suscripcion);
                await _context.SaveChangesAsync();

                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);
                var mensaje = $"Suscripción eliminada para {usuario?.NombreCompleto}";
                
                _logger.LogInformation($"Admin eliminó suscripción para usuario {usuario?.NombreCompleto}");

                return Json(new { success = true, message = mensaje });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar suscripción");
                return Json(new { success = false, message = "Error al eliminar la suscripción" });
            }
        }
    }
}
