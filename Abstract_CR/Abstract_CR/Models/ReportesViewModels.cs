using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Models
{
    public class ReportesDashboardViewModel
    {
        [Display(Name = "Fecha Inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [Display(Name = "Fecha Fin")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; }

        public ResumenMetricasViewModel Resumen { get; set; } = new ResumenMetricasViewModel();

        public IEnumerable<SerieTemporalPunto> NuevosUsuarios { get; set; } = new List<SerieTemporalPunto>();

        public IEnumerable<CategoriaValor> SuscripcionesPorEstado { get; set; } = new List<CategoriaValor>();

        public IEnumerable<SuscripcionVencimientoViewModel> SuscripcionesPorVencer { get; set; } = new List<SuscripcionVencimientoViewModel>();

        public IEnumerable<InteraccionPendienteViewModel> MensajesPendientes { get; set; } = new List<InteraccionPendienteViewModel>();
    }

    public class ResumenMetricasViewModel
    {
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int UsuariosInactivos { get; set; }
        public int NuevasAltas { get; set; }
        public int TotalSuscripciones { get; set; }
        public int SuscripcionesActivas { get; set; }
        public int SuscripcionesPausadas { get; set; }
        public int SuscripcionesCanceladas { get; set; }
        public int TotalRecetas { get; set; }
        public int MensajesPendientes { get; set; }
    }

    public class SerieTemporalPunto
    {
        public string Label { get; set; } = string.Empty;
        public int Valor { get; set; }
    }

    public class CategoriaValor
    {
        public string Categoria { get; set; } = string.Empty;
        public int Valor { get; set; }
    }

    public class SuscripcionVencimientoViewModel
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime? FechaFin { get; set; }
        public DateTime? ProximaFacturacion { get; set; }
        public int? DiasRestantes { get; set; }
    }

    public class InteraccionPendienteViewModel
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public DateTime FechaUltimoMensaje { get; set; }
        public string ContenidoUltimoMensaje { get; set; } = string.Empty;
        public int TotalPendientes { get; set; }
    }
}

