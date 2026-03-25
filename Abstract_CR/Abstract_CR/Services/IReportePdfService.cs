using Abstract_CR.Models;

namespace Abstract_CR.Services
{
    public interface IReportePdfService
    {
        Task<byte[]> GenerarReporteAsync(ReportesDashboardViewModel model);

        /// <summary>Lista Cocina: clientes con suscripción activa, dirección y restricciones.</summary>
        Task<byte[]> GenerarCocinaPdfAsync(IReadOnlyList<CocinaClienteFilaViewModel> filas);
    }
}

