using Abstract_CR.Data;
using Abstract_CR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Abstract_CR.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(ApplicationDbContext context, ILogger<UsuarioController> logger)
        {
            _context = context;
            _logger = logger;
        }


        // GET: Perfil
        public async Task<IActionResult> Perfil()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null)
                return RedirectToAction("Login");

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }


        // GET: EditarPerfil
        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null)
                return RedirectToAction("Login");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(Usuario model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Error de validación: {Error}", error.ErrorMessage);
                }
                return View(model);
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == model.UsuarioID);
            if (usuario == null)
                return NotFound();

            usuario.Nombre = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.CorreoElectronico = model.CorreoElectronico;
            usuario.Activo = model.Activo;

            if (model.RolID.HasValue)
                usuario.RolID = model.RolID.Value;

            if (!string.IsNullOrWhiteSpace(model.ContrasenaHash))
            {
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(model.ContrasenaHash));
                usuario.ContrasenaHash = Convert.ToBase64String(bytes); 
            }

            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("NombreUsuario", usuario.NombreCompleto);
            HttpContext.Session.SetString("Email", usuario.CorreoElectronico);
            TempData["Mensaje"] = "Perfil actualizado correctamente";

            return RedirectToAction("Perfil");
        }

        [HttpGet]
        public async Task<IActionResult> EditarPerfilAdmin()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null)
                return RedirectToAction("Login", "Autenticacion"); // <- opcional, más explícito

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfilAdmin(Usuario model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Error de validación: {Error}", error.ErrorMessage);
                }
                return View(model);
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == model.UsuarioID);
            if (usuario == null)
                return NotFound();

            usuario.Nombre = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.CorreoElectronico = model.CorreoElectronico;
            usuario.Activo = model.Activo;

            if (model.RolID.HasValue)
                usuario.RolID = model.RolID.Value;

            if (!string.IsNullOrWhiteSpace(model.ContrasenaHash))
            {
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(model.ContrasenaHash));
                usuario.ContrasenaHash = Convert.ToBase64String(bytes);
            }

            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("NombreUsuario", usuario.NombreCompleto);
            HttpContext.Session.SetString("Email", usuario.CorreoElectronico);
            TempData["Mensaje"] = "Perfil actualizado correctamente";

            // 👇 Redirige al Panel de Administración
            return RedirectToAction("PanelAdministracion", "Administracion");
        }




        // GET: Usuario/Alergias
        public IActionResult Alergias()
        {
            // TODO: Obtener alergias del usuario actual
            var alergias = ObtenerAlergiasEjemplo();
            return View(alergias);
        }

        // GET: Usuario/AgregarAlergia
        public IActionResult AgregarAlergia()
        {
            return View(new Alergia());
        }

        // POST: Usuario/AgregarAlergia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarAlergia(Alergia alergia)
        {
            if (ModelState.IsValid)
            {
                // TODO: Guardar alergia en la base de datos
                alergia.UsuarioId = 1; // TODO: Obtener ID del usuario actual

                TempData["Mensaje"] = "Alergia agregada correctamente";
                return RedirectToAction(nameof(Alergias));
            }
            return View(alergia);
        }

        // GET: Usuario/Restricciones
        public IActionResult Restricciones()
        {
            // TODO: Obtener restricciones del usuario actual
            var restricciones = ObtenerRestriccionesEjemplo();
            return View(restricciones);
        }

        // GET: Usuario/AgregarRestriccion
        public IActionResult AgregarRestriccion()
        {
            return View(new Restriccion());
        }

        // POST: Usuario/AgregarRestriccion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarRestriccion(Restriccion restriccion)
        {
            if (ModelState.IsValid)
            {
                // TODO: Guardar restricción en la base de datos
                restriccion.UsuarioId = 1; // TODO: Obtener ID del usuario actual

                TempData["Mensaje"] = "Restricción agregada correctamente";
                return RedirectToAction(nameof(Restricciones));
            }
            return View(restriccion);
        }

        // GET: Usuario/Preferencias
        public IActionResult Preferencias()
        {
            // TODO: Obtener preferencias del usuario actual
            var preferencias = ObtenerPreferenciasEjemplo();
            return View(preferencias);
        }

        // GET: Usuario/AgregarPreferencia
        public IActionResult AgregarPreferencia()
        {
            return View(new PreferenciaNutricional());
        }

        // POST: Usuario/AgregarPreferencia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarPreferencia(PreferenciaNutricional preferencia)
        {
            if (ModelState.IsValid)
            {
                // TODO: Guardar preferencia en la base de datos
                preferencia.UsuarioId = 1; // TODO: Obtener ID del usuario actual

                TempData["Mensaje"] = "Preferencia agregada correctamente";
                return RedirectToAction(nameof(Preferencias));
            }
            return View(preferencia);
        }

        // Métodos auxiliares para datos de ejemplo
        private Usuario ObtenerUsuarioEjemplo()
        {
            return new Usuario
            {
                UsuarioID = 1,
                Nombre = "Juan",
                Apellido = "Pérez",
                CorreoElectronico = "juan.perez@email.com",
                ContrasenaHash = "hashedpassword",
                FechaRegistro = DateTime.Now.AddDays(-30),
                RolID = 2, // Cliente
                Activo = true
            };
        }

        private List<Alergia> ObtenerAlergiasEjemplo()
        {
            return new List<Alergia>
            {
                new Alergia
                {
                    Id = 1,
                    Nombre = "Alergia a los frutos secos",
                    Descripcion = "Reacción alérgica a nueces, almendras y otros frutos secos",
                    NivelSeveridad = "Moderado",
                    FechaDiagnostico = new DateTime(2015, 3, 10),
                    Activa = true,
                    UsuarioId = 1
                },
                new Alergia
                {
                    Id = 2,
                    Nombre = "Intolerancia a la lactosa",
                    Descripcion = "Dificultad para digerir productos lácteos",
                    NivelSeveridad = "Leve",
                    FechaDiagnostico = new DateTime(2018, 7, 22),
                    Activa = true,
                    UsuarioId = 1
                }
            };
        }

        private List<Restriccion> ObtenerRestriccionesEjemplo()
        {
            return new List<Restriccion>
            {
                new Restriccion
                {
                    Id = 1,
                    Nombre = "Sin gluten",
                    Descripcion = "Dieta libre de gluten por sensibilidad",
                    TipoRestriccion = "Dietética",
                    Motivo = "Sensibilidad al gluten",
                    FechaInicio = new DateTime(2020, 1, 1),
                    Activa = true,
                    UsuarioId = 1
                },
                new Restriccion
                {
                    Id = 2,
                    Nombre = "Baja en sodio",
                    Descripcion = "Limitación de sal en las comidas",
                    TipoRestriccion = "Médica",
                    Motivo = "Presión arterial alta",
                    FechaInicio = new DateTime(2021, 6, 15),
                    Activa = true,
                    UsuarioId = 1
                }
            };
        }

        private List<PreferenciaNutricional> ObtenerPreferenciasEjemplo()
        {
            return new List<PreferenciaNutricional>
            {
                new PreferenciaNutricional
                {
                    Id = 1,
                    Nombre = "Proteínas magras",
                    Descripcion = "Preferencia por carnes magras y pescado",
                    Categoria = "Proteínas",
                    Valor = "Alto",
                    Prioridad = 4,
                    Activa = true,
                    UsuarioId = 1
                },
                new PreferenciaNutricional
                {
                    Id = 2,
                    Nombre = "Vegetales orgánicos",
                    Descripcion = "Preferencia por vegetales cultivados orgánicamente",
                    Categoria = "Vegetales",
                    Valor = "Orgánico",
                    Prioridad = 3,
                    Activa = true,
                    UsuarioId = 1
                }
            };
        }
    }
}