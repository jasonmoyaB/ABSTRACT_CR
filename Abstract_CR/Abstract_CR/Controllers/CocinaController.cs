using Abstract_CR.Data;
using Abstract_CR.Models;
using Abstract_CR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Abstract_CR.Controllers
{
    /// <summary>
    /// Panel Cocina (admin): usuarios con suscripción activa y vigente, restricciones y dirección de entrega.
    /// </summary>
    public class CocinaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IReportePdfService _reportePdfService;
        private readonly ILogger<CocinaController> _logger;

        public CocinaController(
            ApplicationDbContext context,
            IReportePdfService reportePdfService,
            ILogger<CocinaController> logger)
        {
            _context = context;
            _reportePdfService = reportePdfService;
            _logger = logger;
        }

        private static bool EsAdmin(HttpContext http)
        {
            var rol = http.Session.GetString("Rol");
            return string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Lista usuarios con suscripción activa y no vencida (misma idea que el panel de Suscripciones).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!EsAdmin(HttpContext))
            {
                TempData["Error"] = "No tienes permisos para acceder a esta sección.";
                return RedirectToAction("Index", "Home");
            }

            List<CocinaClienteFilaViewModel> filas;
            try
            {
                filas = await ObtenerFilasCocinaAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando datos de Cocina");
                TempData["Error"] = "No se pudo cargar la lista. Verifica la conexión a la base de datos.";
                filas = new List<CocinaClienteFilaViewModel>();
            }

            return View(filas);
        }

        /// <summary>
        /// Descarga la misma lista que la vista Cocina en formato PDF.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DescargarPdf()
        {
            if (!EsAdmin(HttpContext))
            {
                TempData["Error"] = "No tienes permisos para descargar este documento.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var filas = await ObtenerFilasCocinaAsync();
                var pdfBytes = await _reportePdfService.GenerarCocinaPdfAsync(filas);
                var nombre = $"Cocina_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
                return File(pdfBytes, "application/pdf", nombre);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando PDF de Cocina");
                TempData["Error"] = "No se pudo generar el PDF. Inténtalo de nuevo.";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<List<CocinaClienteFilaViewModel>> ObtenerFilasCocinaAsync()
        {
            var filas = new List<CocinaClienteFilaViewModel>();
            var hoy = DateTime.Today;

            var suscripciones = await _context.Suscripciones
                .AsNoTracking()
                .Include(s => s.Usuario)
                .Where(s => s.Estado == "Activa")
                .Where(s => s.Usuario != null && s.Usuario.Activo)
                .Where(s => !s.FechaFin.HasValue || s.FechaFin.Value.Date >= hoy)
                .ToListAsync();

            var unaPorUsuario = suscripciones
                .GroupBy(s => s.UsuarioID)
                .Select(g => g.OrderByDescending(x => x.SuscripcionID).First())
                .OrderBy(s => s.Usuario!.Nombre)
                .ThenBy(s => s.Usuario!.Apellido)
                .ToList();

            var usuarioIds = unaPorUsuario.Select(s => s.UsuarioID).ToList();

            foreach (var s in unaPorUsuario)
            {
                var u = s.Usuario!;
                filas.Add(new CocinaClienteFilaViewModel
                {
                    UsuarioID = u.UsuarioID,
                    NombreCompleto = u.NombreCompleto,
                    CorreoElectronico = u.CorreoElectronico,
                    Telefono = string.IsNullOrWhiteSpace(u.Telefono) ? null : u.Telefono.Trim(),
                    DireccionEntrega = string.IsNullOrWhiteSpace(u.Direccion) ? null : u.Direccion.Trim(),
                    EstadoSuscripcion = s.Estado,
                    FechaFinSuscripcion = s.FechaFin,
                    Restricciones = Array.Empty<string>()
                });
            }

            if (usuarioIds.Count > 0)
            {
                var restricciones = await _context.RestriccionesAlimentarias
                    .AsNoTracking()
                    .Where(r => usuarioIds.Contains(r.UsuarioID))
                    .OrderBy(r => r.RestriccionID)
                    .Select(r => new { r.UsuarioID, r.Descripcion })
                    .ToListAsync();

                var porUsuario = restricciones
                    .GroupBy(x => x.UsuarioID)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Descripcion).Where(d => !string.IsNullOrWhiteSpace(d)).ToList());

                foreach (var f in filas)
                {
                    if (porUsuario.TryGetValue(f.UsuarioID, out var list))
                        f.Restricciones = list;
                }
            }

            return filas;
        }
    }
}
