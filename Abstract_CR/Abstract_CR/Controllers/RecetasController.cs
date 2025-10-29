using Abstract_CR.Helpers;
using Abstract_CR.Models;
using Microsoft.AspNetCore.Mvc;

namespace Abstract_CR.Controllers
{
    public class RecetasController : Controller
    {
        private readonly CometarioRecetaHelper _cometarioRecetaHelper;

        public RecetasController(CometarioRecetaHelper cometarioRecetaHelper)
        {
            _cometarioRecetaHelper = cometarioRecetaHelper;
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
    }
}
