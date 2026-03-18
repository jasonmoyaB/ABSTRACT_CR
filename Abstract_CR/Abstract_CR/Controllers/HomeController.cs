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
        private readonly MenuSemanalHelper _menuSemanalHelper;

        public HomeController(ILogger<HomeController> logger, CometarioRecetaHelper cometarioRecetaHelper, MenuSemanalHelper menuSemanalHelper)
        {
            _logger = logger;
            _cometarioRecetaHelper = cometarioRecetaHelper;
            _menuSemanalHelper = menuSemanalHelper;
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
            
            // Cargar menús semanales
            var menus = _menuSemanalHelper.ObtenerTodosLosMenusViewModel();
            ViewBag.MenusSemanal = menus;
            
            return View();
        }
    }
}
