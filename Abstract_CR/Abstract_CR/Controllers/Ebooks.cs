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
            int usuarioID = (int)HttpContext.Session.GetInt32("UsuarioID");
            ViewBag.subscripcion = _suscripcionesHelper.GetSuscripcion(usuarioID);

            var ediciones = _ebooksHelper.GetEbooks();

            ViewBag.Count = ediciones.Count; // debug visual en la vista si querés
            return View(ediciones);
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
