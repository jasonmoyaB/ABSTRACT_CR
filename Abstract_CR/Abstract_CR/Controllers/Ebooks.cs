// Controllers/EbooksController.cs
using Abstract_CR.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Abstract_CR.Controllers
{
    public class EbooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EbooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var ediciones = _context.EbookEdicion
                .Where(e => e.Estado)
                .OrderBy(e => e.Escenario)
                .AsNoTracking()
                .ToList();

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
