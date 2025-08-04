using Microsoft.AspNetCore.Mvc;
using Abstract_CR.Models;
using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Controllers
{
    public class PlanNutricionalController : Controller
    {
        // GET: PlanNutricional/Index
        public IActionResult Index()
        {
            // TODO: Obtener usuario actual desde la sesión/base de datos
            var usuario = ObtenerUsuarioEjemplo();
            var planes = ObtenerPlanesNutricionales(usuario.Id);
            return View(planes);
        }

        // GET: PlanNutricional/CargarPlan
        public IActionResult CargarPlan()
        {
            return View();
        }

        // POST: PlanNutricional/CargarPlan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CargarPlan(PlanNutricional plan, IFormFile? archivo)
        {
            if (ModelState.IsValid)
            {
                // TODO: Procesar archivo y guardar plan
                // GuardarPlanNutricional(plan, archivo);
                TempData["Mensaje"] = "Plan nutricional cargado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(plan);
        }

        // GET: PlanNutricional/Detalles/{id}
        public IActionResult Detalles(int id)
        {
            var plan = ObtenerPlanNutricional(id);
            if (plan == null)
            {
                return NotFound();
            }
            return View(plan);
        }

        // GET: PlanNutricional/Menus/{planId}
        public IActionResult Menus(int planId)
        {
            var plan = ObtenerPlanNutricional(planId);
            if (plan == null)
            {
                return NotFound();
            }
            var menus = ObtenerMenusPersonalizados(planId);
            ViewBag.Plan = plan;
            return View(menus);
        }

        // GET: PlanNutricional/GenerarMenus/{planId}
        public IActionResult GenerarMenus(int planId)
        {
            // TODO: Lógica para generar menús automáticamente
            TempData["Mensaje"] = "Menús personalizados generados exitosamente.";
            return RedirectToAction(nameof(Menus), new { planId });
        }

        // GET: PlanNutricional/Evaluar/{planId}
        public IActionResult Evaluar(int planId)
        {
            var plan = ObtenerPlanNutricional(planId);
            if (plan == null)
            {
                return NotFound();
            }
            ViewBag.Plan = plan;
            return View(new EvaluacionPlan { PlanNutricionalId = planId });
        }

        // POST: PlanNutricional/Evaluar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Evaluar(EvaluacionPlan evaluacion)
        {
            if (ModelState.IsValid)
            {
                // TODO: Guardar evaluación
                // GuardarEvaluacionPlan(evaluacion);
                TempData["Mensaje"] = "Evaluación enviada exitosamente. ¡Gracias por tu feedback!";
                return RedirectToAction(nameof(Index));
            }
            var plan = ObtenerPlanNutricional(evaluacion.PlanNutricionalId);
            ViewBag.Plan = plan;
            return View(evaluacion);
        }

        // GET: PlanNutricional/Notificaciones
        public IActionResult Notificaciones()
        {
            var usuario = ObtenerUsuarioEjemplo();
            var notificaciones = ObtenerNotificacionesVencimiento(usuario.Id);
            return View(notificaciones);
        }

        // Métodos auxiliares (placeholder)
        private Usuario ObtenerUsuarioEjemplo()
        {
            return new Usuario
            {
                Id = 1,
                NombreCompleto = "Juan Pérez",
                Email = "juan@ejemplo.com"
            };
        }

        private List<PlanNutricional> ObtenerPlanesNutricionales(int usuarioId)
        {
            // TODO: Implementar lógica de base de datos
            return new List<PlanNutricional>
            {
                new PlanNutricional
                {
                    Id = 1,
                    Nombre = "Plan de Pérdida de Peso",
                    Descripcion = "Plan personalizado para pérdida de peso saludable",
                    TipoPlan = "PDF",
                    FechaInicio = DateTime.Now.AddDays(-30),
                    FechaVencimiento = DateTime.Now.AddDays(30),
                    CaloriasDiarias = 1800,
                    Estado = "Activo"
                },
                new PlanNutricional
                {
                    Id = 2,
                    Nombre = "Plan de Mantenimiento",
                    Descripcion = "Plan para mantener peso actual",
                    TipoPlan = "Formulario",
                    FechaInicio = DateTime.Now.AddDays(-15),
                    FechaVencimiento = DateTime.Now.AddDays(15),
                    CaloriasDiarias = 2200,
                    Estado = "Activo"
                }
            };
        }

        private PlanNutricional? ObtenerPlanNutricional(int id)
        {
            // TODO: Implementar lógica de base de datos
            var planes = ObtenerPlanesNutricionales(1);
            return planes.FirstOrDefault(p => p.Id == id);
        }

        private List<MenuPersonalizado> ObtenerMenusPersonalizados(int planId)
        {
            // TODO: Implementar lógica de base de datos
            return new List<MenuPersonalizado>
            {
                new MenuPersonalizado
                {
                    Id = 1,
                    Nombre = "Desayuno Energético",
                    TipoComida = "Desayuno",
                    DiaSemana = "Lunes",
                    Calorias = 450,
                    Proteinas = 25,
                    Carbohidratos = 45,
                    Grasas = 15,
                    TiempoPreparacion = 15,
                    Dificultad = "Fácil",
                    GeneradoAutomaticamente = true
                },
                new MenuPersonalizado
                {
                    Id = 2,
                    Nombre = "Almuerzo Balanceado",
                    TipoComida = "Almuerzo",
                    DiaSemana = "Lunes",
                    Calorias = 650,
                    Proteinas = 35,
                    Carbohidratos = 60,
                    Grasas = 20,
                    TiempoPreparacion = 25,
                    Dificultad = "Medio",
                    GeneradoAutomaticamente = true
                }
            };
        }

        private List<object> ObtenerNotificacionesVencimiento(int usuarioId)
        {
            // TODO: Implementar lógica de base de datos
            return new List<object>
            {
                new
                {
                    Tipo = "Vencimiento",
                    Mensaje = "Tu plan 'Plan de Pérdida de Peso' vence en 5 días",
                    Fecha = DateTime.Now.AddDays(5),
                    PlanId = 1
                }
            };
        }
    }
} 