using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Abstract_CR.Controllers
{
    public class Autenticacion : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {

            return View();
        }

        public IActionResult CrearCuenta()
        {
            return View();


        }
        public IActionResult RecuperarContraseña()
        {
            return View();


        }


    }
    }
