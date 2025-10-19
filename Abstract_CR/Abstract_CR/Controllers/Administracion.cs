using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Abstract_CR.Controllers
{
    public class AdministracionController : Controller
    {
        // Opcional: protección simple por sesión/rol
        public IActionResult PanelAdministracion()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                // Si no es admin, lo sacamos
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
