using System.Diagnostics;
using Abstract_CR.Models;
using Microsoft.AspNetCore.Mvc;

namespace Abstract_CR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SobreNosotros()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Productos()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Tienda()
        {
            return View();
        }

    }
}
