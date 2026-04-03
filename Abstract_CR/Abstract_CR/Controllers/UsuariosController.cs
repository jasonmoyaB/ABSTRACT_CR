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

            var roles = await _context.Roles
                .OrderBy(r => r.NombreRol)
                .ToListAsync();

            ViewBag.EbookActual = ebookActual;
            ViewBag.Usuarios = usuarios;
            ViewBag.Roles = roles;

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

        // POST: Usuarios/ExtenderSuscripcion - Extender suscripción por 30 días exactos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExtenderSuscripcion(int usuarioId)
        {
            try
            {
                _logger.LogInformation($"ExtenderSuscripcion llamado - UsuarioID: {usuarioId}");

                // Verificar que el usuario sea administrador
                var rol = HttpContext.Session.GetString("Rol");
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

                // Buscar suscripción existente
                var suscripcion = await _context.Suscripciones
                    .FirstOrDefaultAsync(s => s.UsuarioID == usuarioId);

                DateTime nuevaFechaFin;

                if (suscripcion != null && suscripcion.FechaFin.HasValue)
                {
                    // Si ya tiene suscripción, calcular desde la fecha fin
                    var fechaFinActual = suscripcion.FechaFin.Value.Date;
                    
                    // Si la fecha fin ya pasó, calcular desde hoy
                    // Si no, calcular sumando 30 días exactos a la fecha de término actual
                    var fechaBase = fechaFinActual < DateTime.Now.Date 
                        ? DateTime.Now.Date 
                        : fechaFinActual.AddDays(1);
                    
                    // Extensión de 30 días exactos desde la fecha base
                    nuevaFechaFin = fechaBase.AddDays(30);

                    // Actualizar la suscripción existente
                    suscripcion.FechaFin = nuevaFechaFin;
                    suscripcion.Estado = "Activa";
                    suscripcion.ProximaFacturacion = nuevaFechaFin;
                    
                    _logger.LogInformation($"Suscripción extendida 30 días hasta: {nuevaFechaFin:dd/MM/yyyy}");
                }
                else
                {
                    // Crear nueva suscripción por 30 días a partir de hoy
                    var hoy = DateTime.Now.Date;
                    nuevaFechaFin = hoy.AddDays(30);

                    suscripcion = new Suscripcion
                    {
                        UsuarioID = usuarioId,
                        Estado = "Activa",
                        FechaInicio = hoy,
                        FechaFin = nuevaFechaFin,
                        UltimaFacturacion = hoy,
                        ProximaFacturacion = nuevaFechaFin
                    };

                    _context.Suscripciones.Add(suscripcion);
                    _logger.LogInformation($"Nueva suscripción creada 30 días hasta: {nuevaFechaFin:dd/MM/yyyy}");
                }

                await _context.SaveChangesAsync();

                var mensaje = $"Suscripción extendida para {usuario.NombreCompleto} por 30 días, finalizando el {suscripcion.FechaFin.Value:dd/MM/yyyy}";
                _logger.LogInformation($"Admin extendió suscripción para usuario {usuario.NombreCompleto} por 30 días");

                return Json(new { success = true, message = mensaje });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al extender suscripción");
                return Json(new { success = false, message = $"Error al extender la suscripción: {ex.Message}" });
            }
        }

        // GET: Usuarios/ObtenerComprobantesUsuario - Obtener comprobantes de pago de un usuario
        [HttpGet]
        public async Task<IActionResult> ObtenerComprobantesUsuario(int usuarioId)
        {
            try
            {
                // Verificar que el usuario sea administrador
                var rol = HttpContext.Session.GetString("Rol");
                if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Verificar que el usuario existe
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // Obtener los comprobantes del usuario
                var comprobantes = await _context.ComprobantesPago
                    .Where(c => c.UsuarioID == usuarioId)
                    .OrderByDescending(c => c.FechaSubida)
                    .Select(c => new
                    {
                        comprobanteID = c.ComprobanteID,
                        usuarioId = c.UsuarioID,
                        nombreArchivoOriginal = c.NombreArchivoOriginal,
                        tipoArchivo = c.TipoArchivo,
                        rutaArchivo = c.RutaArchivo,
                        fechaSubida = c.FechaSubida,
                        estado = c.Estado,
                        observaciones = c.Observaciones
                    })
                    .ToListAsync();

                return Json(new { success = true, comprobantes = comprobantes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comprobantes del usuario");
                return Json(new { success = false, message = $"Error al cargar los comprobantes: {ex.Message}" });
            }
        }

        // POST: Usuarios/CambiarRol - Cambiar el rol de un usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarRol(int usuarioId, int rolId)
        {
            try
            {
                var rol = HttpContext.Session.GetString("Rol");
                if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);

                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                var nuevoRol = await _context.Roles.FirstOrDefaultAsync(r => r.RolID == rolId);
                if (nuevoRol == null)
                {
                    return Json(new { success = false, message = "Rol no válido" });
                }

                var rolAnterior = usuario.Rol?.NombreRol ?? "Sin rol";
                usuario.RolID = rolId;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Admin cambió rol de {Usuario} de '{RolAnterior}' a '{RolNuevo}'",
                    usuario.NombreCompleto, rolAnterior, nuevoRol.NombreRol);

                return Json(new
                {
                    success = true,
                    message = $"Rol de {usuario.NombreCompleto} cambiado a '{nuevoRol.NombreRol}'",
                    nuevoRolNombre = nuevoRol.NombreRol
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar rol del usuario");
                return Json(new { success = false, message = $"Error al cambiar el rol: {ex.Message}" });
            }
        }

        // POST: Usuarios/CambiarEstadoComprobante - Cambiar el estado de un comprobante
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoComprobante(int comprobanteId, string nuevoEstado)
        {
            try
            {
                // Verificar que el usuario sea administrador
                var rol = HttpContext.Session.GetString("Rol");
                if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Validar el nuevo estado
                if (string.IsNullOrEmpty(nuevoEstado) || 
                    (nuevoEstado != "Aprobado" && nuevoEstado != "Rechazado" && nuevoEstado != "Pendiente"))
                {
                    return Json(new { success = false, message = "Estado no válido" });
                }

                // Buscar el comprobante
                var comprobante = await _context.ComprobantesPago
                    .Include(c => c.Usuario)
                    .FirstOrDefaultAsync(c => c.ComprobanteID == comprobanteId);

                if (comprobante == null)
                {
                    return Json(new { success = false, message = "Comprobante no encontrado" });
                }

                // Guardar el estado anterior
                var estadoAnterior = comprobante.Estado;
                
                // Actualizar el estado
                comprobante.Estado = nuevoEstado;
                
                await _context.SaveChangesAsync();

                var mensaje = $"Estado del comprobante cambiado de '{estadoAnterior}' a '{nuevoEstado}' para {comprobante.Usuario?.NombreCompleto ?? "el usuario"}";
                _logger.LogInformation($"Admin cambió estado del comprobante {comprobanteId} de {estadoAnterior} a {nuevoEstado}");

                return Json(new { success = true, message = mensaje });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del comprobante");
                return Json(new { success = false, message = $"Error al cambiar el estado: {ex.Message}" });
            }
        }
    }
}
