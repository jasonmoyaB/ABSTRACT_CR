using System.Diagnostics;
using Abstract_CR.Helpers;
using Abstract_CR.Models;
using Microsoft.AspNetCore.Mvc;

namespace Abstract_CR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CometarioRecetaHelper _cometarioRecetaHelper;

        public HomeController(ILogger<HomeController> logger, CometarioRecetaHelper cometarioRecetaHelper)
        {
            _logger = logger;
            _cometarioRecetaHelper = cometarioRecetaHelper;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Productos()
        {
            return View();
        }

        public IActionResult Tienda()
        {
            return View();
        }

        public IActionResult SobreNosotros()
        {
            return View();
        }

        public IActionResult MiPerfil()
        {
            return View();
        }

        public IActionResult Ebooks()
        {
            return View();
        }

        public IActionResult MenuSemanal()
        {
            var comentarios = _cometarioRecetaHelper.ObtenerComentariosPorReceta(1);
            ViewBag.ComentariosReceta = comentarios;
            return View();
        }
    }
}
