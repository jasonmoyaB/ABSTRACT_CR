using Abstract_CR.Data;
using Abstract_CR.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Abstract_CR.Controllers
{
    public class EbookEdicionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EbookEdicionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Mostrar lista
        public IActionResult Index()
        {
            // Mostrar todos (activos e inactivos)
            var ediciones = _context.EbookEdicion
                .OrderBy(e => e.Escenario)
                .ToList();

            return View(ediciones);
        }

        // ✅ GET: Crear
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ✅ POST: Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EbookEdicion model)
        {
            if (ModelState.IsValid)
            {
                model.FechaRegistro = DateTime.Now;
                _context.Add(model);
                _context.SaveChanges();

                TempData["Success"] = "Nuevo escenario agregado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ✅ GET: Editar
        public IActionResult Edit(int id)
        {
            var item = _context.EbookEdicion.Find(id);
            if (item == null)
                return NotFound();

            return View(item);
        }

        // ✅ POST: Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EbookEdicion model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    _context.SaveChanges();
                    TempData["Success"] = "Cambios guardados exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar los cambios: " + ex.Message);
                }
            }

            return View(model);
        }

        // ✅ POST: Eliminar
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var item = _context.EbookEdicion.Find(id);
            if (item == null)
                return NotFound();

            _context.EbookEdicion.Remove(item);
            _context.SaveChanges();

            TempData["Success"] = " Escenario eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult ToggleEstado(int id)
        {
            var item = _context.EbookEdicion.Find(id);
            if (item == null)
                return NotFound();

            item.Estado = !item.Estado;
            _context.Update(item);
            _context.SaveChanges();

            TempData["Success"] = item.Estado
                ? " Escenario activado correctamente."
                : " Escenario inactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
