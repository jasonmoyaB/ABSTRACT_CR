using Abstract_CR.Helpers;
using Abstract_CR.Models;
using Abstract_CR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

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

        public RecetasController(CometarioRecetaHelper cometarioRecetaHelper, IEmailService emailService, UserHelper userHelper, RecetasHelper recetasHelper, MenuSemanalHelper menuSemanalHelper, IWebHostEnvironment webHostEnvironment)
        {
            _cometarioRecetaHelper = cometarioRecetaHelper;
            _emailService = emailService;
            _userHelper = userHelper;
            _recetasHelper = recetasHelper;
            _menuSemanalHelper = menuSemanalHelper;
            _webHostEnvironment = webHostEnvironment;
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
                    html += $@"<li class='d-flex justify-content-between align-items-center mb-2'>
                        <span>
                            <i class='fas fa-comment-dots text-secondary me-2'></i>{item.Comentario}
                        </span>
                        <button class='btn btn-sm btn-danger btn-eliminar-comentario' 
                                data-comentario-id='{item.ComentarioID}'
                                onclick='event.stopPropagation(); eliminarComentario({item.ComentarioID});'>
                            <i class='fas fa-trash'></i>
                        </button>
                    </li>";
                }
                html += "</ul>";
                return html;
            }
            else
            {
                return "<span>Sin comentarios</span>";
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

                // Guardar imagen si se proporciona
                if (Imagen != null && Imagen.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "menu-semanal");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = $"{Guid.NewGuid()}_{Imagen.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Imagen.CopyToAsync(stream);
                    }

                    rutaImagen = $"/uploads/menu-semanal/{fileName}";
                }

                try
                {
                    var guardado = _menuSemanalHelper.GuardarMenu(model, rutaImagen);
                    
                    if (guardado)
                    {
                        return Json(new { success = true, message = "Menú guardado correctamente" });
                    }
                    
                    return Json(new { success = false, message = "Error al guardar el menú. El método retornó false." });
                }
                catch (Exception helperEx)
                {
                    // Capturar excepciones del helper
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
                // Incluir más detalles del error para debugging
                var errorMessage = $"Error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | InnerException: {ex.InnerException.Message}";
                }
                return Json(new { success = false, message = errorMessage });
            }
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
                //var recetaAsignada = true;
                if (recetaAsignada)
                {
                    var usuarioAsignado = _userHelper.GetUsuarioPorId(personaId);
                    var subject = $"Se le ha asignado un nuevo menú";
                    var body = $@"
                            <html>
                                <body style='font-family:Arial,Helvetica,sans-serif; line-height:1.5;'>
                                <h2>El chef le ha asignado un nuevo menú para el día {diaSemana}</h2>
                                <hr/>
                                <p style='font-size:12px;color:#666'>Si ya revisate el menú, puedes ignorar este mensaje.</p>
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
