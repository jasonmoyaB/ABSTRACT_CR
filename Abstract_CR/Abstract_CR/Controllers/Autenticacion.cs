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

        public IActionResult PasswordReset()
        {
            return View();


        }

        public IActionResult Registro()
        {
            return View();


        }
    }
}