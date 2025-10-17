using Abstract_CR.Data;                    
using Abstract_CR.Models;                   
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Abstract_CR.Models;

namespace Abstract_CR.Controllers           
{
    public class EbookEdicionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EbookEdicionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Mostrar la lista de escenarios del eBook
        public IActionResult Index()
        {
            var lista = _context.EbookEdicion
                .OrderBy(e => e.Escenario)
                .ToList();

            return View(lista);
        }

        // ✅ GET: Editar un escenario específico
        public IActionResult Edit(int id)
        {
            var item = _context.EbookEdicion.Find(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // ✅ POST: Guardar cambios del escenario
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

                    TempData["Success"] = "✅ Los cambios se han guardado satisfactoriamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "❌ Error al guardar los cambios: " + ex.Message);
                }
            }

            return View(model);
        }

        // ✅ GET: Crear nuevo escenario
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ✅ POST: Guardar nuevo escenario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EbookEdicion model)
        {
            if (ModelState.IsValid)
            {
                model.FechaRegistro = DateTime.Now;
                _context.Add(model);
                _context.SaveChanges();

                TempData["Success"] = "Se ha agregado un nuevo escenario al eBook.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ✅ Eliminar un escenario
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var item = _context.EbookEdicion.Find(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.EbookEdicion.Remove(item);
            _context.SaveChanges();

            TempData["Success"] = "🗑️ El escenario se eliminó correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
