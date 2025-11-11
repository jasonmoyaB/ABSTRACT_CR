using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Abstract_CR.Services; // IEmailService
using System.Threading;
using System.Threading.Tasks;

namespace Abstract_CR.Services
{
    public class VencimientosNotifier : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly IConfiguration _cfg;
        private readonly ILogger<VencimientosNotifier> _log;

        public VencimientosNotifier(IServiceProvider sp, IConfiguration cfg, ILogger<VencimientosNotifier> log)
        {
            _sp = sp;
            _cfg = cfg;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Corre una vez al inicio, luego todos los días a las 08:00 (hora Costa Rica)
            while (!stoppingToken.IsCancellationRequested)
            {
                var nextRun = NextRunCR(hour: 8, minute: 0); // 08:00 CR
                var delay = nextRun - DateTime.UtcNow;
                if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

                _log.LogInformation("[VencimientosNotifier] Próxima ejecución: {NextRunCRLocal}",
                    TimeZoneInfo.ConvertTimeFromUtc(nextRun, TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time")));

                try { await Task.Delay(delay, stoppingToken); }
                catch { /* cancelado */ }

                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    await ProcesarEnvios(stoppingToken);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "[VencimientosNotifier] Error en procesamiento.");
                }
            }
        }

        private static DateTime NextRunCR(int hour, int minute)
        {
            // Costa Rica: "Central America Standard Time" (UTC-6 sin DST)
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
            var nowCr = TimeZoneInfo.ConvertTime(DateTime.Now, tz);
            var target = new DateTime(nowCr.Year, nowCr.Month, nowCr.Day, hour, minute, 0);
            if (nowCr > target) target = target.AddDays(1);
            return TimeZoneInfo.ConvertTimeToUtc(target, tz);
        }

        private async Task ProcesarEnvios(CancellationToken ct)
        {
            var cs = _cfg.GetConnectionString("DefaultConnection")!;
            using var connection = new SqlConnection(cs);
            await connection.OpenAsync(ct);

            // Trae TODOS los planes que vencen en 7 o 3 días y el correo del usuario
            var sql = @"
SELECT p.PlanID, p.UsuarioID, p.Descripcion, p.FechaVencimiento, u.CorreoElectronico
FROM dbo.PlanesNutricionales p
JOIN dbo.Usuarios u ON u.UsuarioID = p.UsuarioID
WHERE p.FechaVencimiento IS NOT NULL
  AND DATEDIFF(DAY, GETDATE(), p.FechaVencimiento) IN (7,3);
";

            using var cmd = new SqlCommand(sql, connection);
            using var rd = await cmd.ExecuteReaderAsync(ct);

            var planes = new List<(int PlanID, int UsuarioID, string Descripcion, DateTime Vence, string Email)>();
            while (await rd.ReadAsync(ct))
            {
                planes.Add((
                    rd.GetInt32(rd.GetOrdinal("PlanID")),
                    rd.GetInt32(rd.GetOrdinal("UsuarioID")),
                    rd["Descripcion"]?.ToString() ?? "",
                    rd.GetDateTime(rd.GetOrdinal("FechaVencimiento")),
                    rd["CorreoElectronico"]?.ToString() ?? ""
                ));
            }
            rd.Close();

            // Resuelve IEmailService del contenedor
            var email = (IEmailService)_sp.GetService(typeof(IEmailService))!;

            foreach (var p in planes)
            {
                var dias = (p.Vence.Date - DateTime.Now.Date).Days; // 7 o 3 garantizado por SQL
                if (dias != 7 && dias != 3) continue; // por si el reloj del server varía

                var token = $"[PID={p.PlanID};UMBRAL={dias}]";
                if (await YaSeEnvio(connection, p.UsuarioID, token, ct)) continue; // idempotencia Plan+Umbral

                var mensajeLimpio = dias == 7
                    ? $"⚠️ 7 días antes del vencimiento: Te avisamos que tu plan \"{p.Descripcion}\" expirará en una semana."
                    : $"⚠️ 3 días antes del vencimiento: Un recordatorio cercano para que no olvides tu plan \"{p.Descripcion}\".";

                var mensajeParaBD = $"{token} {mensajeLimpio}";

                // 1) Inserta notificación en BD
                var insert = @"
INSERT INTO dbo.Notificaciones (UsuarioID, Mensaje, Tipo, FechaEnvio)
VALUES (@UsuarioID, @Mensaje, @Tipo, @FechaEnvio)";
                using (var ins = new SqlCommand(insert, connection))
                {
                    ins.Parameters.AddWithValue("@UsuarioID", p.UsuarioID);
                    ins.Parameters.AddWithValue("@Mensaje", mensajeParaBD);
                    ins.Parameters.AddWithValue("@Tipo", "Vencimiento");
                    ins.Parameters.AddWithValue("@FechaEnvio", DateTime.Now);
                    await ins.ExecuteNonQueryAsync(ct);
                }

                // 2) Envía email (si hay correo)
                if (!string.IsNullOrWhiteSpace(p.Email))
                {
                    var subject = $"Recordatorio de vencimiento – {p.Descripcion}";
                    var htmlDescripcion = System.Net.WebUtility.HtmlEncode(p.Descripcion);
                    var body = $@"
<html>
  <body style='font-family:Arial,Helvetica,sans-serif; line-height:1.5;'>
    <h2>Recordatorio de tu plan</h2>
    <p>{mensajeLimpio}</p>
    <p><strong>Plan:</strong> {htmlDescripcion}</p>
    <p><strong>Fecha de vencimiento:</strong> {p.Vence:dd/MM/yyyy}</p>
    <hr/>
    <p style='font-size:12px;color:#666'>Si ya renovaste, puedes ignorar este mensaje.</p>
  </body>
</html>";
                    try
                    {
                        await email.SendEmailAsync(p.Email, subject, body);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "[VencimientosNotifier] Error enviando email a {Email}", p.Email);
                    }
                }
            }
        }

        private static async Task<bool> YaSeEnvio(SqlConnection connection, int usuarioId, string token, CancellationToken ct)
        {
            var sql = @"
SELECT COUNT(1)
FROM dbo.Notificaciones
WHERE UsuarioID = @UsuarioID
  AND Tipo = 'Vencimiento'
  AND Mensaje LIKE @Token";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@Token", $"%{token}%");
            var count = (int)await cmd.ExecuteScalarAsync(ct);
            return count > 0;
        }
    }
}
