using Abstract_CR.Data;
using Abstract_CR.Models;
using Abstract_CR.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Abstract_CR.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportesController> _logger;
        private readonly IReportePdfService _reportePdfService;

        public ReportesController(ApplicationDbContext context, ILogger<ReportesController> logger, IReportePdfService reportePdfService)
        {
            _context = context;
            _logger = logger;
            _reportePdfService = reportePdfService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta)
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            // últimos 30 días
            var fechaInicio = desde ?? DateTime.UtcNow.Date.AddDays(-30);
            var fechaFin = hasta ?? DateTime.UtcNow.Date;

            // FechaInicio - FechaFin
            if (fechaInicio > fechaFin)
            {
                TempData["Error"] = "La fecha de inicio no puede ser mayor que la fecha de fin.";
                fechaInicio = fechaFin.AddDays(-30);
            }

            try
            {
                _logger.LogInformation($"Cargando reportes desde {fechaInicio:yyyy-MM-dd} hasta {fechaFin:yyyy-MM-dd}");

                var viewModel = await ObtenerDatosReporteAsync(fechaInicio, fechaFin);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar reportes: {Message}", ex.Message);
                _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
                TempData["Error"] = $"Ocurrió un error al cargar los reportes: {ex.Message}";
                
                var viewModelError = new ReportesDashboardViewModel
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    Resumen = new ResumenMetricasViewModel(),
                    NuevosUsuarios = new List<SerieTemporalPunto>(),
                    SuscripcionesPorEstado = new List<CategoriaValor>(),
                    SuscripcionesPorVencer = new List<SuscripcionVencimientoViewModel>(),
                    MensajesPendientes = new List<InteraccionPendienteViewModel>()
                };
                return View(viewModelError);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DescargarPdf(DateTime? desde, DateTime? hasta)
        {
            if (!EsAdmin())
            {
                TempData["Error"] = "No tienes permisos para descargar reportes.";
                return RedirectToAction("Index", "Home");
            }

           
            var fechaInicio = desde ?? DateTime.UtcNow.Date.AddDays(-30);
            var fechaFin = hasta ?? DateTime.UtcNow.Date;

            if (fechaInicio > fechaFin)
            {
                TempData["Error"] = "La fecha de inicio no puede ser mayor que la fecha de fin.";
                return RedirectToAction("Index", new { desde, hasta });
            }

            try
            {
                _logger.LogInformation($"Generando PDF de reportes desde {fechaInicio:yyyy-MM-dd} hasta {fechaFin:yyyy-MM-dd}");

                
                var viewModel = await ObtenerDatosReporteAsync(fechaInicio, fechaFin);

                // Generar PDF
                var pdfBytes = await _reportePdfService.GenerarReporteAsync(viewModel);

                // Retornar archivo PDF
                var nombreArchivo = $"Reporte_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de reportes: {Message}", ex.Message);
                TempData["Error"] = $"Ocurrió un error al generar el PDF: {ex.Message}";
                return RedirectToAction("Index", new { desde, hasta });
            }
        }

        private async Task<ReportesDashboardViewModel> ObtenerDatosReporteAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            // Resumen de mewtricas
            var resumen = new ResumenMetricasViewModel
                {
                    TotalUsuarios = await _context.Usuarios.CountAsync(),
                    UsuariosActivos = await _context.Usuarios.CountAsync(u => u.Activo == true),
                    UsuariosInactivos = await _context.Usuarios.CountAsync(u => u.Activo == false),
                    NuevasAltas = await _context.Usuarios
                        .CountAsync(u => u.FechaRegistro.HasValue &&
                                        u.FechaRegistro.Value.Date >= fechaInicio &&
                                        u.FechaRegistro.Value.Date <= fechaFin),
                    TotalSuscripciones = await _context.Suscripciones.CountAsync(),
                    SuscripcionesActivas = await _context.Suscripciones.CountAsync(s => s.Estado == "Activa"),
                    SuscripcionesPausadas = await _context.Suscripciones.CountAsync(s => s.Estado == "Pausada"),
                    SuscripcionesCanceladas = await _context.Suscripciones.CountAsync(s => s.Estado == "Cancelada"),
                    TotalRecetas = await _context.Recetas.CountAsync(),
                    MensajesPendientes = await _context.MensajesInteraccion
                        .CountAsync(m => !m.EnviadoPorChef && !m.Leido)
                };

                // onuevas altas por día
                var nuevasAltasPorDia = await _context.Usuarios
                    .Where(u => u.FechaRegistro.HasValue &&
                               u.FechaRegistro.Value.Date >= fechaInicio &&
                               u.FechaRegistro.Value.Date <= fechaFin)
                    .GroupBy(u => u.FechaRegistro.Value.Date)
                    .Select(g => new SerieTemporalPunto
                    {
                        Label = g.Key.ToString("yyyy-MM-dd"),
                        Valor = g.Count()
                    })
                    .ToListAsync();

                
                var todosLosDias = new List<SerieTemporalPunto>();
                var fechaActual = fechaInicio;
                while (fechaActual <= fechaFin)
                {
                    var punto = nuevasAltasPorDia.FirstOrDefault(p => p.Label == fechaActual.ToString("yyyy-MM-dd"));
                    todosLosDias.Add(new SerieTemporalPunto
                    {
                        Label = fechaActual.ToString("yyyy-MM-dd"),
                        Valor = punto?.Valor ?? 0
                    });
                    fechaActual = fechaActual.AddDays(1);
                }

                // Suscripciones por estado
                var suscripcionesPorEstado = new List<CategoriaValor>
                {
                    new CategoriaValor
                    {
                        Categoria = "Activa",
                        Valor = resumen.SuscripcionesActivas
                    },
                    new CategoriaValor
                    {
                        Categoria = "Pausada",
                        Valor = resumen.SuscripcionesPausadas
                    },
                    new CategoriaValor
                    {
                        Categoria = "Cancelada",
                        Valor = resumen.SuscripcionesCanceladas
                    }
                };

                // Suscripciones proximas a vencer (prooximos 14 días)
                var fechaLimiteVencimiento = DateTime.UtcNow.Date.AddDays(14);
                var suscripcionesPorVencer = await _context.Suscripciones
                    .Include(s => s.Usuario)
                    .Where(s => s.Estado == "Activa" &&
                               s.ProximaFacturacion.HasValue &&
                               s.ProximaFacturacion.Value.Date >= DateTime.UtcNow.Date &&
                               s.ProximaFacturacion.Value.Date <= fechaLimiteVencimiento)
                    .OrderBy(s => s.ProximaFacturacion)
                    .Take(10)
                    .Select(s => new SuscripcionVencimientoViewModel
                    {
                        NombreUsuario = s.Usuario != null ? s.Usuario.NombreCompleto : "N/A",
                        Correo = s.Usuario != null ? s.Usuario.CorreoElectronico : "N/A",
                        Estado = s.Estado,
                        FechaFin = s.FechaFin,
                        ProximaFacturacion = s.ProximaFacturacion,
                        DiasRestantes = s.ProximaFacturacion.HasValue
                            ? (int)(s.ProximaFacturacion.Value.Date - DateTime.UtcNow.Date).TotalDays
                            : null
                    })
                    .ToListAsync();

                // Mensajes pendientes agrupados por usuario
                
                var gruposConteo = await _context.MensajesInteraccion
                    .Where(m => !m.EnviadoPorChef && !m.Leido)
                    .GroupBy(m => m.UsuarioId)
                    .Select(g => new
                    {
                        UsuarioId = g.Key,
                        TotalPendientes = g.Count(),
                        UltimaFecha = g.Max(m => m.FechaEnvio)
                    })
                    .OrderByDescending(x => x.UltimaFecha)
                    .Take(10)
                    .ToListAsync();

                //  ultimos mensajes y usuarios
                var mensajesPendientes = new List<InteraccionPendienteViewModel>();
                foreach (var grupo in gruposConteo)
                {
                    var ultimoMensaje = await _context.MensajesInteraccion
                        .Include(m => m.Usuario)
                        .Where(m => m.UsuarioId == grupo.UsuarioId && 
                                   !m.EnviadoPorChef && 
                                   !m.Leido &&
                                   m.FechaEnvio == grupo.UltimaFecha)
                        .FirstOrDefaultAsync();

                    if (ultimoMensaje != null && ultimoMensaje.Usuario != null)
                    {
                        mensajesPendientes.Add(new InteraccionPendienteViewModel
                        {
                            UsuarioId = grupo.UsuarioId,
                            NombreUsuario = ultimoMensaje.Usuario.NombreCompleto,
                            Correo = ultimoMensaje.Usuario.CorreoElectronico,
                            FechaUltimoMensaje = ultimoMensaje.FechaEnvio,
                            ContenidoUltimoMensaje = ultimoMensaje.Contenido.Length > 100
                                ? ultimoMensaje.Contenido.Substring(0, 100) + "..."
                                : ultimoMensaje.Contenido,
                            TotalPendientes = grupo.TotalPendientes
                        });
                    }
                }

            return new ReportesDashboardViewModel
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Resumen = resumen,
                NuevosUsuarios = todosLosDias,
                SuscripcionesPorEstado = suscripcionesPorEstado,
                SuscripcionesPorVencer = suscripcionesPorVencer,
                MensajesPendientes = mensajesPendientes
            };
        }

        private bool EsAdmin()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (!string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "No tienes permisos para acceder a esta sección.";
                return false;
            }

            return true;
        }
    }
}

