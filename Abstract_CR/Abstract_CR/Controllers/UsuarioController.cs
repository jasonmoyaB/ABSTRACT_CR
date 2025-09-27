using Abstract_CR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using Dapper;

namespace Abstract_CR.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly ILogger<IConfiguration> _logger;
        private readonly IConfiguration _conf;

        public UsuarioController(ILogger<UsuarioController> logger, IConfiguration conf)
        {
            //_logger = logger;
            _conf = conf;
        }

        // GET: Usuario/Perfil
        public IActionResult Perfil()
        {
            // TODO: Obtener usuario actual desde la sesión/base de datos
            var usuario = ObtenerUsuarioEjemplo();
            return View(usuario);
            //using (var connection = new SqlConnection(_conf.GetConnectionString("DefaultConnection")))
            //{
            //    connection.Open();
            //    casas = connection.Query<CasasModel>(
            //        "ConsultarCasasSistema",
            //        commandType: CommandType.StoredProcedure
            //    );
            //}
            //return View(casas);
        }

        // GET: Usuario/EditarPerfil
        public IActionResult EditarPerfil()
        {
            // TODO: Obtener usuario actual desde la sesión/base de datos
            var usuario = ObtenerUsuarioEjemplo();
            return View(usuario);
        }

        // POST: Usuario/EditarPerfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarPerfil(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                // TODO: Guardar cambios en la base de datos
                // Nota: FechaActualizacion no existe en el modelo actual
                
                TempData["Mensaje"] = "Perfil actualizado correctamente";
                return RedirectToAction(nameof(Perfil));
            }
            return View(usuario);
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