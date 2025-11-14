using Abstract_CR.Models;

namespace Abstract_CR.Services
{
    public interface IReportePdfService
    {
        Task<byte[]> GenerarReporteAsync(ReportesDashboardViewModel model);
    }
}

