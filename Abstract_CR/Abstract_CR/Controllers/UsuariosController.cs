using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Abstract_CR.Data;
using Abstract_CR.Models;
using Microsoft.EntityFrameworkCore;

namespace Abstract_CR.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(ApplicationDbContext context, ILogger<UsuariosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Usuarios/Index - Vista principal de gestión de usuarios
        public async Task<IActionResult> Index()
        {
            // Verificar que el usuario sea administrador
            var rol = HttpContext.Session.GetString("Rol");
            if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home");
            }

            // Obtener todos los usuarios (activos e inactivos)
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            // Obtener información del ebook actual
            var ebookActual = await _context.EbookEdicion
                .FirstOrDefaultAsync();

            ViewBag.EbookActual = ebookActual;
            ViewBag.Usuarios = usuarios;

            return View();
        }

        // POST: Usuarios/ToggleUsuarioDescarga - Cambiar estado de descarga para un usuario específico
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUsuarioDescarga(int usuarioId, bool permitirDescarga)
        {
            try
            {
                _logger.LogInformation($"ToggleUsuarioDescarga llamado - UsuarioID: {usuarioId}, PermitirDescarga: {permitirDescarga}");

                // Verificar que el usuario sea administrador
                var rol = HttpContext.Session.GetString("Rol");
                _logger.LogInformation($"Rol del usuario actual: {rol}");
                
                if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning($"Usuario no autorizado. Rol: {rol}");
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Buscar el usuario
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);
                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario {usuarioId} no encontrado");
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                _logger.LogInformation($"Usuario encontrado: {usuario.NombreCompleto}, Estado actual: {usuario.PermitirDescargaEbook}");

                // Actualizar el estado de descarga del usuario
                var estadoAnterior = usuario.PermitirDescargaEbook;
                usuario.PermitirDescargaEbook = permitirDescarga;
                
                _logger.LogInformation($"Cambiando estado de {estadoAnterior} a {permitirDescarga}");

                var cambios = await _context.SaveChangesAsync();
                _logger.LogInformation($"Cambios guardados en la base de datos: {cambios} registros afectados");

                // Verificar que el cambio se guardó
                var usuarioActualizado = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);
                _logger.LogInformation($"Estado después del guardado: {usuarioActualizado?.PermitirDescargaEbook}");

                var mensaje = permitirDescarga ? 
                    $"Descarga habilitada para {usuario.NombreCompleto}" : 
                    $"Descarga deshabilitada para {usuario.NombreCompleto}";
                
                _logger.LogInformation($"Admin cambió estado de descarga para usuario {usuario.NombreCompleto} a: {permitirDescarga}");

                return Json(new { success = true, message = mensaje });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de descarga del usuario");
                return Json(new { success = false, message = $"Error al actualizar la configuración del usuario: {ex.Message}" });
            }
        }

        // POST: Usuarios/ToggleEbookDownload - Cambiar estado de descarga del ebook (mantener para compatibilidad)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEbookDownload(bool permitirDescarga)
        {
            try
            {
                // Verificar que el usuario sea administrador
                var rol = HttpContext.Session.GetString("Rol");
                if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Buscar o crear el ebook actual
                var ebook = await _context.EbookEdicion.FirstOrDefaultAsync();
                
                if (ebook == null)
                {
                    // Crear un nuevo ebook si no existe
                    ebook = new EbookEdicion
                    {
                        TituloHU = "Recetas Saludables",
                        Escenario = 1,
                        CriterioAceptacion = "Usuario puede descargar ebook de recetas",
                        Contexto = "Usuario logueado en el sistema",
                        Evento = "Usuario solicita descarga del ebook",
                        Resultado = "Sistema permite o deniega la descarga según configuración",
                        Estado = true,
                        FechaRegistro = DateTime.Now,
                        PermitirDescarga = permitirDescarga
                    };
                    _context.EbookEdicion.Add(ebook);
                }
                else
                {
                    // Actualizar el estado de descarga
                    ebook.PermitirDescarga = permitirDescarga;
                }

                await _context.SaveChangesAsync();

                var mensaje = permitirDescarga ? "Descarga de ebook habilitada" : "Descarga de ebook deshabilitada";
                
                _logger.LogInformation($"Admin cambió estado de descarga de ebook a: {permitirDescarga}");

                return Json(new { success = true, message = mensaje });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de descarga del ebook");
                return Json(new { success = false, message = "Error al actualizar la configuración" });
            }
        }

        // GET: Usuarios/UsuariosActivos - Obtener usuarios activos (AJAX)
        [HttpGet]
        public async Task<IActionResult> UsuariosActivos()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Include(u => u.Rol)
                    .Where(u => u.Activo)
                    .Select(u => new
                    {
                        u.UsuarioID,
                        u.Nombre,
                        u.Apellido,
                        u.CorreoElectronico,
                        u.FechaRegistro,
                        RolNombre = u.Rol != null ? u.Rol.NombreRol : "Sin rol"
                    })
                    .OrderBy(u => u.Nombre)
                    .ToListAsync();

                return Json(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios activos");
                return Json(new { error = "Error al cargar usuarios" });
            }
        }

        // GET: Usuarios/Estadisticas - Obtener estadísticas de usuarios (AJAX)
        [HttpGet]
        public async Task<IActionResult> Estadisticas()
        {
            try
            {
                var totalUsuarios = await _context.Usuarios.CountAsync();
                var usuariosActivos = await _context.Usuarios.CountAsync(u => u.Activo);
                var usuariosInactivos = totalUsuarios - usuariosActivos;
                
                var usuariosPorRol = await _context.Usuarios
                    .Include(u => u.Rol)
                    .GroupBy(u => u.Rol != null ? u.Rol.NombreRol : "Sin rol")
                    .Select(g => new { Rol = g.Key, Cantidad = g.Count() })
                    .ToListAsync();

                var estadisticas = new
                {
                    TotalUsuarios = totalUsuarios,
                    UsuariosActivos = usuariosActivos,
                    UsuariosInactivos = usuariosInactivos,
                    UsuariosPorRol = usuariosPorRol
                };

                return Json(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de usuarios");
                return Json(new { error = "Error al cargar estadísticas" });
            }
        }
    }
}
