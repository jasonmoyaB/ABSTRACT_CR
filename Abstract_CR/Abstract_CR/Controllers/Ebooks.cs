// Controllers/EbooksController.cs
using Abstract_CR.Data;
using Abstract_CR.Helpers;
using Abstract_CR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Abstract_CR.Controllers
{
    public class EbooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EbooksHelper _ebooksHelper;
        private readonly SuscripcionesHelper _suscripcionesHelper;

        public EbooksController(ApplicationDbContext context, EbooksHelper ebooksHelper, SuscripcionesHelper suscripcionesHelper)
        {
            _context = context;
            _ebooksHelper = ebooksHelper;
            _suscripcionesHelper = suscripcionesHelper;
        }

        public IActionResult Index()
        {
            var usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioID == null)
            {
                return RedirectToAction("Login", "Autenticacion");
            }
            
            var suscripcion = _suscripcionesHelper.GetSuscripcion(usuarioID.Value);
            
            // Si no hay suscripción, crear una instancia vacía para evitar errores en la vista
            if (suscripcion == null)
            {
                suscripcion = new Suscripcion { SuscripcionID = 0, Estado = "Sin suscripción" };
            }
            
            ViewBag.subscripcion = suscripcion;

            var ediciones = _ebooksHelper.GetEbooks();
            
            // Obtener información del ebook actual para el pack especial
            var ebookActual = _context.EbookEdicion.FirstOrDefault();
            ViewBag.EbookActual = ebookActual;
            
            // Obtener información del usuario actual
            var usuarioActual = _context.Usuarios.FirstOrDefault(u => u.UsuarioID == usuarioID.Value);
            ViewBag.UsuarioActual = usuarioActual;

            ViewBag.Count = ediciones.Count; // debug visual
            return View(ediciones);
        }

        // GET: Ebooks/SubirArchivo - Vista para subir archivo del ebook
        public IActionResult SubirArchivo()
        {
            // Verificar que el usuario sea administrador
            var rol = HttpContext.Session.GetString("Rol");
            if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home");
            }

            // Obtener información del ebook actual
            var ebookActual = _context.EbookEdicion.FirstOrDefault();
            ViewBag.EbookActual = ebookActual;

            return View();
        }

        // POST: Ebooks/SubirArchivo - Subir archivo del ebook
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirArchivo(IFormFile archivo)
        {
            try
            {
                // Verificar que el usuario sea administrador
                var rol = HttpContext.Session.GetString("Rol");
                if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                if (archivo == null || archivo.Length == 0)
                {
                    return Json(new { success = false, message = "No se ha seleccionado ningún archivo" });
                }

                // Verificar que es un PDF
                if (!archivo.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "Solo se permiten archivos PDF" });
                }

                // Verificar tamaño del archivo (máximo 50MB)
                if (archivo.Length > 50 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "El archivo es demasiado grande. Máximo 50MB" });
                }

                // Obtener o crear el ebook
                var ebook = await _context.EbookEdicion.FirstOrDefaultAsync();
                if (ebook == null)
                {
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
                        PermitirDescarga = true
                    };
                    _context.EbookEdicion.Add(ebook);
                }

                // Crear directorio si no existe
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "eBooks");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generar nombre único para el archivo
                var extension = Path.GetExtension(archivo.FileName);
                var fileName = $"ebook_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Guardar el archivo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                // Actualizar información del ebook
                ebook.NombreArchivo = archivo.FileName;
                ebook.RutaArchivo = fileName;
                ebook.TamañoArchivo = archivo.Length;
                ebook.TipoMime = archivo.ContentType;
                ebook.FechaSubida = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Archivo '{archivo.FileName}' subido exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al subir el archivo: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Descargar(int id)
        {
            // Verificar que el usuario esté logueado
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null)
            {
                TempData["Error"] = "Debes iniciar sesión para descargar el ebook";
                return RedirectToAction("Login", "Autenticacion");
            }

            try
            {
                // Buscar el ebook
                var ebook = await _context.EbookEdicion.FirstOrDefaultAsync(e => e.EbookEdicionID == id);
                
                if (ebook == null)
                {
                    TempData["Error"] = "El ebook solicitado no existe";
                    return RedirectToAction("Index");
                }

                // Verificar si la descarga está habilitada globalmente
                if (!ebook.PermitirDescarga)
                {
                    TempData["Error"] = "La descarga de ebooks está temporalmente deshabilitada por el administrador";
                    return RedirectToAction("Index");
                }

                // Verificar si el usuario específico puede descargar (ebook requiere pago adicional)
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);
                if (usuario != null && !usuario.PermitirDescargaEbook)
                {
                    TempData["Error"] = "No tienes permisos para descargar el ebook. Este es un producto adicional que requiere pago extra. Contacta al administrador.";
                    return RedirectToAction("Index");
                }

                // Verificar si el archivo existe
                if (string.IsNullOrEmpty(ebook.RutaArchivo))
                {
                    TempData["Error"] = "El archivo del ebook no está disponible";
                    return RedirectToAction("Index");
                }

                // Construir la ruta del archivo
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var filePath = Path.Combine(webRootPath, "uploads", "eBooks", ebook.RutaArchivo);

                if (!System.IO.File.Exists(filePath))
                {
                    TempData["Error"] = "El archivo del ebook no se encuentra en el servidor";
                    return RedirectToAction("Index");
                }

                // Leer el archivo y devolverlo
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var fileName = !string.IsNullOrEmpty(ebook.NombreArchivo) ? ebook.NombreArchivo : "ebook.pdf";
                var contentType = !string.IsNullOrEmpty(ebook.TipoMime) ? ebook.TipoMime : "application/pdf";

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                // Log del error
                Console.WriteLine($"Error al descargar ebook: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al descargar el ebook. Inténtalo de nuevo.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Debug()
        {
            var total = _context.EbookEdicion.Count();
            var activos = _context.EbookEdicion.Count(e => e.Estado);
            var cn = _context.Database.GetDbConnection();
            return Content($"DB: {cn.Database} | Server: {cn.DataSource} | Total: {total} | Activos: {activos}");
        }
    }
}
