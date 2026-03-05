using Abstract_CR.Helpers;
using Abstract_CR.Models;
using Abstract_CR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.Web;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Abstract_CR.Controllers
{
    public class RecetasController : Controller
    {
        private readonly CometarioRecetaHelper _cometarioRecetaHelper;
        private readonly IEmailService _emailService;
        private readonly UserHelper _userHelper;
        private readonly RecetasHelper _recetasHelper;
        private readonly MenuSemanalHelper _menuSemanalHelper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<RecetasController> _logger;

        // Configuración de seguridad para archivos
        private static readonly long MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
        private static readonly string[] ALLOWED_EXTENSIONS = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly string[] ALLOWED_MIME_TYPES = { 
            "image/jpeg", 
            "image/png", 
            "image/gif", 
            "image/webp" 
        };

        public RecetasController(
            CometarioRecetaHelper cometarioRecetaHelper, 
            IEmailService emailService, 
            UserHelper userHelper, 
            RecetasHelper recetasHelper, 
            MenuSemanalHelper menuSemanalHelper, 
            IWebHostEnvironment webHostEnvironment,
            ILogger<RecetasController> logger)
        {
            _cometarioRecetaHelper = cometarioRecetaHelper;
            _emailService = emailService;
            _userHelper = userHelper;
            _recetasHelper = recetasHelper;
            _menuSemanalHelper = menuSemanalHelper;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AgregarComentarioReceta(int recetaId, string comentario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(comentario))
                {
                    return Json(new { success = false, message = "El comentario no puede estar vacío" });
                }

                var guardado = _cometarioRecetaHelper.AgregarComentario(new ComentarioReceta
                {
                    RecetaID = 1,
                    Comentario = comentario,
                    FechaComentario = DateTime.Now,
                    UsuarioID = HttpContext.Session.GetInt32("UsuarioID").Value,
                });

                if (!guardado)
                {
                    return Json(new { success = false, message = "Ocurrió un error al guardar el comentario" });
                }

                List<ComentarioReceta> comentariosActualizados = _cometarioRecetaHelper.ObtenerComentariosPorReceta(1).ToList();

                string html = RenderizarComentarios(comentariosActualizados);

                return Json(new { success = true, html = html });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult EliminarComentarioReceta(int comentarioId, int recetaId)
        {
            try
            {
                var eliminado = _cometarioRecetaHelper.EliminarComentario(comentarioId);

                var comentariosActualizados = _cometarioRecetaHelper.ObtenerComentariosPorReceta(1).ToList();

                string html = RenderizarComentarios(comentariosActualizados);

                return Json(new { success = true, html = html });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private string RenderizarComentarios(List<ComentarioReceta> comentarios)
        {
            if (comentarios.Count > 0)
            {
                var html = "<ul class='list-unstyled'>";
                foreach (var item in comentarios)
                {
                    // Codificar el comentario para prevenir XSS
                    var comentarioCodificado = HttpUtility.HtmlEncode(item.Comentario);
                    
                    html += $@"<li class='d-flex justify-content-between align-items-start mb-3 p-2 bg-light rounded'>
                        <span class='flex-grow-1'>
                            <i class='fas fa-comment-dots text-secondary me-2'></i>
                            <span class='text-dark'>{comentarioCodificado}</span>
                        </span>
                        <button class='btn btn-sm btn-outline-danger ms-2' 
                                data-comentario-id='{item.ComentarioID}'
                                onclick='event.stopPropagation(); eliminarComentario({item.ComentarioID});'
                                title='Eliminar comentario'>
                            <i class='fas fa-trash'></i>
                        </button>
                    </li>";
                }
                html += "</ul>";
                return html;
            }
            else
            {
                return @"<p class='text-muted text-center py-3'>
                            <i class='fas fa-comment-slash me-2'></i>Sin comentarios aún
                        </p>";
            }
        }

        [HttpGet]
        public IActionResult ObtenerRecetas()
        {
            ViewBag.Recetas = _recetasHelper.GetRecetasViewModel().ToList();
            ViewBag.Personas = _userHelper.GetUsuarioPorAsignar().ToList();
            return View();
        }

        public IActionResult AsignarMenu()
        {
            return View();
        }

        [HttpGet]
        public IActionResult MenuSemanal()
        {
            ViewData["Title"] = "Menú Semanal";
            var menus = _menuSemanalHelper.ObtenerTodosLosMenusViewModel();
            ViewBag.Menus = menus;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(5 * 1024 * 1024)] // Limitar a 5MB a nivel de request
        public async Task<IActionResult> GuardarMenuSemanal(IFormFile? Imagen)
        {
            try
            {
                // Construir el modelo manualmente desde el FormData
                var model = new MenuSemanalViewModel();
                
                if (Request.Form.ContainsKey("MenuSemanalID") && int.TryParse(Request.Form["MenuSemanalID"], out int menuId))
                {
                    model.MenuSemanalID = menuId;
                }
                
                model.DiaSemana = Request.Form["DiaSemana"].ToString();
                model.NombrePlatillo = Request.Form["NombrePlatillo"].ToString();
                model.TipChef = Request.Form["TipChef"].ToString();
                model.Descripcion = Request.Form["Descripcion"].ToString();
                
                // Deserializar características
                if (Request.Form.ContainsKey("Caracteristicas"))
                {
                    var caracteristicasJson = Request.Form["Caracteristicas"].ToString();
                    try
                    {
                        model.Caracteristicas = System.Text.Json.JsonSerializer.Deserialize<List<string>>(caracteristicasJson) ?? new List<string>();
                    }
                    catch
                    {
                        model.Caracteristicas = new List<string>();
                    }
                }
                
                // Deserializar ingredientes
                if (Request.Form.ContainsKey("IngredientesPrincipales"))
                {
                    var ingredientesJson = Request.Form["IngredientesPrincipales"].ToString();
                    try
                    {
                        model.IngredientesPrincipales = System.Text.Json.JsonSerializer.Deserialize<List<string>>(ingredientesJson) ?? new List<string>();
                    }
                    catch
                    {
                        model.IngredientesPrincipales = new List<string>();
                    }
                }

                // Validar datos requeridos
                if (string.IsNullOrWhiteSpace(model.NombrePlatillo))
                {
                    return Json(new { success = false, message = "El nombre del platillo es obligatorio" });
                }
                
                if (string.IsNullOrWhiteSpace(model.DiaSemana))
                {
                    return Json(new { success = false, message = "El día de la semana es obligatorio" });
                }

                string? rutaImagen = null;

                // VALIDACIÓN Y GUARDADO SEGURO DE IMAGEN
                if (Imagen != null && Imagen.Length > 0)
                {
                    // Validar el archivo de forma robusta
                    var validacionResultado = ValidarImagenPlatillo(Imagen);
                    if (!validacionResultado.esValido)
                    {
                        _logger.LogWarning("Intento de subir archivo inválido: {FileName}. Razón: {Mensaje}", 
                            Imagen.FileName, validacionResultado.mensaje);
                        return Json(new { success = false, message = validacionResultado.mensaje });
                    }

                    // Guardar imagen de forma segura
                    try
                    {
                        rutaImagen = await GuardarImagenSegura(Imagen);
                    }
                    catch (Exception exImg)
                    {
                        _logger.LogError(exImg, "Error al guardar imagen del platillo");
                        return Json(new { success = false, message = "Error al guardar la imagen. Por favor, intenta de nuevo." });
                    }
                }

                try
                {
                    var guardado = _menuSemanalHelper.GuardarMenu(model, rutaImagen);
                    
                    if (guardado)
                    {
                        _logger.LogInformation("Menú guardado correctamente para {DiaSemana}", model.DiaSemana);
                        return Json(new { success = true, message = "Menú guardado correctamente" });
                    }
                    
                    return Json(new { success = false, message = "Error al guardar el menú. El método retornó false." });
                }
                catch (Exception helperEx)
                {
                    _logger.LogError(helperEx, "Error al guardar el menú en el helper");
                    var errorMessage = $"Error al guardar el menú: {helperEx.Message}";
                    if (helperEx.InnerException != null)
                    {
                        errorMessage += $" | Detalles: {helperEx.InnerException.Message}";
                    }
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general al procesar GuardarMenuSemanal");
                var errorMessage = $"Error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | InnerException: {ex.InnerException.Message}";
                }
                return Json(new { success = false, message = errorMessage });
            }
        }

        /// <summary>
        /// Valida de forma robusta que el archivo sea una imagen válida
        /// </summary>
        private (bool esValido, string mensaje) ValidarImagenPlatillo(IFormFile archivo)
        {
            // 1. Validar que el archivo existe
            if (archivo == null || archivo.Length == 0)
            {
                return (false, "El archivo está vacío o no existe.");
            }

            // 2. Validar tamaño del archivo
            if (archivo.Length > MAX_FILE_SIZE)
            {
                var tamañoMB = archivo.Length / 1024.0 / 1024.0;
                return (false, $"El archivo es demasiado grande ({tamañoMB:F2} MB). Tamaño máximo permitido: 5MB.");
            }

            // 3. Validar extensión del archivo
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !ALLOWED_EXTENSIONS.Contains(extension))
            {
                var extensionesPermitidas = string.Join(", ", ALLOWED_EXTENSIONS);
                return (false, $"Formato de archivo no válido ({extension}). Solo se permiten: {extensionesPermitidas}");
            }

            // 4. Validar MIME type del archivo
            if (!ALLOWED_MIME_TYPES.Contains(archivo.ContentType.ToLowerInvariant()))
            {
                _logger.LogWarning("MIME type no permitido: {MimeType} para archivo {FileName}", 
                    archivo.ContentType, archivo.FileName);
                return (false, $"Tipo de contenido no válido ({archivo.ContentType}). Solo se permiten imágenes.");
            }

            // 5. Validar que realmente sea una imagen leyendo los magic bytes
            try
            {
                using var stream = archivo.OpenReadStream();
                var header = new byte[8];
                stream.Read(header, 0, 8);
                stream.Position = 0;

                // Magic bytes de formatos de imagen comunes
                bool esImagenValida = 
                    (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) || // JPEG
                    (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) || // PNG
                    (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46) || // GIF
                    (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 && 
                     header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50); // WEBP

                if (!esImagenValida)
                {
                    _logger.LogWarning("Archivo {FileName} no es una imagen válida (magic bytes incorrectos)", archivo.FileName);
                    return (false, "El archivo no es una imagen válida. El contenido no coincide con el formato esperado.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar el contenido del archivo {FileName}", archivo.FileName);
                return (false, "Error al validar el archivo. Por favor, intenta con otra imagen.");
            }

            // 6. Validar que el nombre del archivo no contenga caracteres peligrosos
            var nombreArchivo = Path.GetFileName(archivo.FileName);
            if (nombreArchivo.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                nombreArchivo.Contains("..") || 
                nombreArchivo.Contains("/") || 
                nombreArchivo.Contains("\\"))
            {
                return (false, "El nombre del archivo contiene caracteres no permitidos.");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Guarda la imagen de forma segura con nombre aleatorio y sin extensión original
        /// </summary>
        private async Task<string> GuardarImagenSegura(IFormFile imagen)
        {
            // Crear directorio si no existe
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "menu-semanal");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generar nombre de archivo seguro y único
            var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
            var nombreSeguro = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, nombreSeguro);

            // Guardar el archivo
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await imagen.CopyToAsync(stream);
                await stream.FlushAsync();
            }

            _logger.LogInformation("Imagen guardada correctamente: {NombreArchivo}", nombreSeguro);

            return $"/uploads/menu-semanal/{nombreSeguro}";
        }

        [HttpGet]
        public IActionResult ObtenerMenuPorDia(string diaSemana)
        {
            var menu = _menuSemanalHelper.ObtenerMenuViewModelPorDia(diaSemana);
            return Json(new { success = true, menu = menu });
        }

        [HttpPost]
        public async Task<IActionResult> AsignarReceta(int recetaId, int personaId, string diaSemana)
        {
            try
            {
                var recetaAsignada = _recetasHelper.AsignarRecetas(recetaId, personaId, diaSemana);
                if (recetaAsignada)
                {
                    var usuarioAsignado = _userHelper.GetUsuarioPorId(personaId);
                    var subject = $"Se le ha asignado un nuevo menú";
                    var body = $@"
                            <html>
                                <body style='font-family:Arial,Helvetica,sans-serif; line-height:1.5;'>
                                <h2>El chef le ha asignado un nuevo menú para el día {diaSemana}</h2>
                                <hr/>
                                <p style='font-size:12px;color:#666'>Si ya revisaste el menú, puedes ignorar este mensaje.</p>
                                </body>
                            </html>";
                    try { await _emailService.SendEmailAsync(usuarioAsignado.CorreoElectronico, subject, body); } catch { }

                    return Json(new
                    {
                        success = true,
                        message = $"Receta asignada correctamente a {usuarioAsignado.NombreCompleto}"
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Error al asignar la receta"
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al asignar la receta"
                });
            }
        }
    }
}
