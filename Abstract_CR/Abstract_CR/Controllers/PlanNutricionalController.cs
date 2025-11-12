using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Abstract_CR.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Abstract_CR.Services;


namespace Abstract_CR.Controllers
{
    public class PlanNutricionalController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;


        private readonly IEmailService _emailService;


        public PlanNutricionalController(IConfiguration configuration, IEmailService emailService)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _emailService = emailService;
        }


        // GET: PlanNutricional/Index
        public IActionResult Index()
        {
            var usuarioId = ObtenerUsuarioIdActual();
            var planes = ObtenerPlanesNutricionales(usuarioId);
            return View(planes);
        }

        // GET: PlanNutricional/CargarPlan
        public IActionResult CargarPlan()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null)
            {
                TempData["Error"] = "Debes iniciar sesión para cargar un plan nutricional.";
                return RedirectToAction("Login", "Autenticacion");
            }
            return View();
        }

        // POST: PlanNutricional/CargarPlan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CargarPlan(PlanNutricional plan, IFormFile? archivo)
        {
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
                if (usuarioId == null)
                {
                    TempData["Error"] = "Debes iniciar sesión para cargar un plan nutricional.";
                    return RedirectToAction("Login", "Autenticacion");
                }

                if (!ModelState.IsValid)
                    return View(plan);

                string? archivoUrl = null;
                if (archivo != null)
                {
                    var validacionArchivo = ValidarArchivo(archivo);
                    if (!validacionArchivo.esValido)
                    {
                        ModelState.AddModelError("archivo", validacionArchivo.mensaje);
                        return View(plan);
                    }

                    archivoUrl = await GuardarArchivo(archivo);
                }

                var planId = await GuardarPlanEnBaseDatos(plan, usuarioId.Value, archivoUrl);

                TempData["Success"] = "Plan nutricional cargado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar el plan: " + ex.Message);
                return View(plan);
            }
        }

        // GET: PlanNutricional/Details/{id}
        public IActionResult Details(int id)
        {
            var plan = ObtenerPlanPorId(id);
            if (plan == null)
                return NotFound();

            return View("Detalles", plan);
        }

        // GET: PlanNutricional/Delete/{id}
        public IActionResult Delete(int id)
        {
            var plan = ObtenerPlanPorId(id);
            if (plan == null)
                return NotFound();

            return View(plan);
        }

        // POST: PlanNutricional/Delete/{id}
        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                EliminarPlan(id);
                TempData["Success"] = "Plan nutricional eliminado exitosamente.";
                return Json(new { success = true, redirectUrl = Url.Action("Index", "PlanNutricional") });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el plan: " + ex.Message;
                return Json(new { success = true, redirectUrl = Url.Action("Index", "PlanNutricional") });
            }
        }

        // MÉTODOS PRIVADOS AUXILIARES
        private int ObtenerUsuarioIdActual()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null || usuarioId <= 0)
                throw new UnauthorizedAccessException("Usuario no autenticado");

            return usuarioId.Value;
        }

        private async Task<int> GuardarPlanEnBaseDatos(PlanNutricional plan, int usuarioId, string? archivoUrl)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"EXEC dbo.spPlan_Create 
                        @UsuarioID, @NombrePlan, @Descripcion, 
                        @FechaVencimiento, @DocumentoURL";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UsuarioID", usuarioId);
            command.Parameters.AddWithValue("@NombrePlan", plan.Nombre);
            command.Parameters.AddWithValue("@Descripcion", plan.Descripcion ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@FechaVencimiento", plan.FechaVencimiento ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DocumentoURL", archivoUrl ?? (object)DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        private List<PlanNutricional> ObtenerPlanesNutricionales(int usuarioId)
        {
            var planes = new List<PlanNutricional>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var sql = @"SELECT PlanID, UsuarioID, NombrePlan, Descripcion, FechaCarga, 
                               FechaVencimiento, DocumentoURL, 'Activo' as Estado
                        FROM dbo.PlanesNutricionales 
                        WHERE UsuarioID = @UsuarioID 
                        ORDER BY FechaCarga DESC";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UsuarioID", usuarioId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                planes.Add(new PlanNutricional
                {
                    PlanID = reader.GetInt32("PlanID"),
                    UsuarioID = reader.GetInt32("UsuarioID"),
                    Nombre = reader.GetString("NombrePlan"),
                    Descripcion = reader.IsDBNull("Descripcion") ? null : reader.GetString("Descripcion"),
                    FechaCarga = reader.GetDateTime("FechaCarga"),
                    FechaVencimiento = reader.IsDBNull("FechaVencimiento") ? null : reader.GetDateTime("FechaVencimiento"),
                    DocumentoURL = reader.IsDBNull("DocumentoURL") ? null : reader.GetString("DocumentoURL"),
                    Estado = reader.GetString("Estado"),
                    TipoPlan = "PDF"
                });
            }

            return planes;
        }

        private PlanNutricional? ObtenerPlanPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var sql = @"SELECT PlanID, UsuarioID, NombrePlan, Descripcion, FechaCarga, 
                               FechaVencimiento, DocumentoURL, 'Activo' as Estado
                        FROM dbo.PlanesNutricionales 
                        WHERE PlanID = @PlanID";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@PlanID", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new PlanNutricional
                {
                    PlanID = reader.GetInt32("PlanID"),
                    UsuarioID = reader.GetInt32("UsuarioID"),
                    Nombre = reader.GetString("NombrePlan"),
                    Descripcion = reader.IsDBNull("Descripcion") ? null : reader.GetString("Descripcion"),
                    FechaCarga = reader.GetDateTime("FechaCarga"),
                    FechaVencimiento = reader.IsDBNull("FechaVencimiento") ? null : reader.GetDateTime("FechaVencimiento"),
                    DocumentoURL = reader.IsDBNull("DocumentoURL") ? null : reader.GetString("DocumentoURL"),
                    Estado = reader.GetString("Estado"),
                    TipoPlan = "PDF"
                };
            }

            return null;
        }

        private void EliminarPlan(int planId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var sql = "DELETE FROM dbo.PlanesNutricionales WHERE PlanID = @PlanID";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@PlanID", planId);
            command.ExecuteNonQuery();
        }

        private (bool esValido, string mensaje) ValidarArchivo(IFormFile archivo)
        {
            if (archivo.Length > 10 * 1024 * 1024)
                return (false, "El archivo es demasiado grande. El tamaño máximo es 10MB.");

            var extensionesPermitidas = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
                return (false, "Formato de archivo no válido. Solo se permiten archivos PDF, JPG, JPEG, PNG y GIF.");

            return (true, string.Empty);
        }

        private async Task<string> GuardarArchivo(IFormFile archivo)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "planes");
            Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await archivo.CopyToAsync(fileStream);

            return $"/uploads/planes/{fileName}";
        }
        [HttpGet]
        public async Task<IActionResult> Notificaciones()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null)
            {
                TempData["Error"] = "Debes iniciar sesión para ver tus notificaciones.";
                return RedirectToAction("Login", "Autenticacion");
            }

            // Solo ejecuta INSERT/EMAIL si vienes con ?run=1
            bool run = string.Equals(Request.Query["run"], "1", StringComparison.Ordinal);

            var notificaciones = new List<Notificacion>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var sqlPlanes = @"
        SELECT PlanID, UsuarioID, Descripcion, FechaVencimiento
        FROM dbo.PlanesNutricionales
        WHERE UsuarioID = @UsuarioID
          AND FechaVencimiento IS NOT NULL
          AND DATEDIFF(DAY, GETDATE(), FechaVencimiento) BETWEEN 0 AND 7
    ";

            var planes = new List<(int PlanID, string Descripcion, DateTime FechaVencimiento)>();

            using (var commandPlanes = new SqlCommand(sqlPlanes, connection))
            {
                commandPlanes.Parameters.AddWithValue("@UsuarioID", usuarioId.Value);
                using var reader = commandPlanes.ExecuteReader();
                while (reader.Read())
                {
                    planes.Add((
                        PlanID: reader.GetInt32(reader.GetOrdinal("PlanID")),
                        Descripcion: reader["Descripcion"]?.ToString() ?? "",
                        FechaVencimiento: reader.GetDateTime(reader.GetOrdinal("FechaVencimiento"))
                    ));
                }
            }

            var emailUsuario = ObtenerEmailUsuario(usuarioId.Value);

            foreach (var plan in planes)
            {
                var diasRestantes = (plan.FechaVencimiento.Date - DateTime.Now.Date).Days;

                if (diasRestantes is 7 or 3 or 1 or 0)
                {
                    var token = $"[PID={plan.PlanID};UMBRAL={diasRestantes}]";

                    string? mensajeLimpio = diasRestantes switch
                    {
                        7 => $" 7 días antes del vencimiento: Te avisamos que tu plan \"{plan.Descripcion}\" expirará en una semana.",
                        3 => $" 3 días antes del vencimiento: Un recordatorio cercano para que no olvides tu plan \"{plan.Descripcion}\".",
                        1 => $" 1 día antes del vencimiento: Último aviso para que tomes acción sobre tu plan \"{plan.Descripcion}\" antes de que caduque.",
                        0 => $" El día del vencimiento: Tu plan \"{plan.Descripcion}\" ha llegado a su fecha final.",
                        _ => null
                    };
                    if (mensajeLimpio == null) continue;


                    notificaciones.Add(new Notificacion
                    {
                        UsuarioID = usuarioId.Value,
                        Mensaje = mensajeLimpio,
                        Tipo = "Vencimiento",
                        FechaEnvio = DateTime.Now
                    });


                    if (!run) continue;


                    if (YaSeEnvioVencimiento(connection, usuarioId.Value, token))
                        continue;


                    var mensajeConToken = $"{token} {mensajeLimpio}";

                    var sqlInsert = @"
                INSERT INTO dbo.Notificaciones (UsuarioID, Mensaje, Tipo, FechaEnvio)
                VALUES (@UsuarioID, @Mensaje, @Tipo, @FechaEnvio)
            ";
                    using (var commandInsert = new SqlCommand(sqlInsert, connection))
                    {
                        commandInsert.Parameters.AddWithValue("@UsuarioID", usuarioId.Value);
                        commandInsert.Parameters.AddWithValue("@Mensaje", mensajeConToken);
                        commandInsert.Parameters.AddWithValue("@Tipo", "Vencimiento");
                        commandInsert.Parameters.AddWithValue("@FechaEnvio", DateTime.Now);
                        commandInsert.ExecuteNonQuery();
                    }


                    if (!string.IsNullOrWhiteSpace(emailUsuario))
                    {
                        var subject = $"Recordatorio de vencimiento – {plan.Descripcion}";
                        var htmlDescripcion = System.Net.WebUtility.HtmlEncode(plan.Descripcion);
                        var body = $@"
                <html>
                  <body style='font-family:Arial,Helvetica,sans-serif; line-height:1.5;'>
                    <h2>Recordatorio de tu plan</h2>
                    <p>{mensajeLimpio}</p>
                    <p><strong>Plan:</strong> {htmlDescripcion}</p>
                    <p><strong>Fecha de vencimiento:</strong> {plan.FechaVencimiento:dd/MM/yyyy}</p>
                    <hr/>
                    <p style='font-size:12px;color:#666'>Si ya renovaste, puedes ignorar este mensaje.</p>
                  </body>
                </html>";
                        try { await _emailService.SendEmailAsync(emailUsuario, subject, body); } catch { }
                    }
                }
            }

            notificaciones = notificaciones.OrderBy(n => n.FechaEnvio).ToList();
            return View(notificaciones);
        }


        private bool YaSeEnvioVencimiento(SqlConnection connection, int usuarioId, string token)
        {
            var sql = @"
        SELECT COUNT(1)
        FROM dbo.Notificaciones
        WHERE UsuarioID = @UsuarioID
          AND Tipo = 'Vencimiento'
          AND CHARINDEX(@Token, Mensaje) > 0
    ";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@Token", token);
            var count = (int)cmd.ExecuteScalar();
            return count > 0;
        }

        private string? ObtenerEmailUsuario(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
        SELECT TOP 1 CorreoElectronico
        FROM dbo.Usuarios
        WHERE UsuarioID = @UsuarioID";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);

            var result = cmd.ExecuteScalar();
            return result == null || result == DBNull.Value ? null : Convert.ToString(result);
        }
        [HttpGet]
        public IActionResult EvaluarPlan()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null)
            {
                var vmNoSesion = new EvaluacionPlanViewModel
                {
                    TienePlanActivo = false,
                    ErrorMensaje = "Debes iniciar sesión para evaluar tu plan."
                };
                return View("EvaluarPlan", vmNoSesion);
            }

            int? planId = null;
            string? nombrePlan = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sqlPlan = @"
            SELECT TOP 1 PlanID, NombrePlan
            FROM dbo.PlanesNutricionales
            WHERE UsuarioID = @UsuarioID
              AND (
                    FechaVencimiento IS NULL
                    OR FechaVencimiento >= CAST(GETDATE() AS date)
                  )
            ORDER BY FechaCarga DESC;
        ";

                using (var cmd = new SqlCommand(sqlPlan, connection))
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", usuarioId.Value);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            planId = reader.GetInt32(reader.GetOrdinal("PlanID"));
                            nombrePlan = reader["NombrePlan"]?.ToString() ?? "(Plan sin nombre)";
                        }
                    }
                }
            }

            if (planId == null)
            {
                var vmSinPlan = new EvaluacionPlanViewModel
                {
                    TienePlanActivo = false,
                    ErrorMensaje = "No tienes un plan nutricional activo en este momento."
                };
                return View("EvaluarPlan", vmSinPlan);
            }

            var vm = new EvaluacionPlanViewModel
            {
                TienePlanActivo = true,
                PlanID = planId,
                NombrePlanActual = nombrePlan
            };

            return View("EvaluarPlan", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EvaluarPlan(EvaluacionPlanViewModel model)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null)
            {
                var vmNoSesion = new EvaluacionPlanViewModel
                {
                    TienePlanActivo = false,
                    ErrorMensaje = "Debes iniciar sesión para enviar la evaluación."
                };
                return View("EvaluarPlan", vmNoSesion);
            }

            bool planEsValido = false;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sqlCheckPlan = @"
            SELECT COUNT(1)
            FROM dbo.PlanesNutricionales
            WHERE PlanID = @PlanID
              AND UsuarioID = @UsuarioID
              AND (
                    FechaVencimiento IS NULL
                    OR FechaVencimiento >= CAST(GETDATE() AS date)
                  );
        ";

                using (var checkCmd = new SqlCommand(sqlCheckPlan, connection))
                {
                    checkCmd.Parameters.AddWithValue("@PlanID", model.PlanID ?? 0);
                    checkCmd.Parameters.AddWithValue("@UsuarioID", usuarioId.Value);
                    var count = (int)checkCmd.ExecuteScalar();
                    planEsValido = (count > 0);
                }
            }

            if (!planEsValido)
            {
                var vmSinAcceso = new EvaluacionPlanViewModel
                {
                    TienePlanActivo = false,
                    ErrorMensaje = "No tienes acceso para evaluar este plan."
                };
                return View("EvaluarPlan", vmSinAcceso);
            }

            if (!ModelState.IsValid)
            {
                model.TienePlanActivo = true;
                model.ErrorMensaje = "Por favor completa todos los campos obligatorios.";
                return View("EvaluarPlan", model);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sqlInsert = @"
            INSERT INTO dbo.EvaluarPlanesNutricionales
                (PlanID, UsuarioID, Calificacion, Comentario, FechaRegistro)
            VALUES
                (@PlanID, @UsuarioID, @Calificacion, @Comentario, SYSUTCDATETIME());
        ";

                using (var insertCmd = new SqlCommand(sqlInsert, connection))
                {
                    insertCmd.Parameters.AddWithValue("@PlanID", model.PlanID ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@UsuarioID", usuarioId.Value);
                    insertCmd.Parameters.AddWithValue("@Calificacion", model.Calificacion ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@Comentario", (object?)model.Comentario ?? DBNull.Value);
                    insertCmd.ExecuteNonQuery();
                }
            }

            var vmGracias = new EvaluacionPlanViewModel
            {
                TienePlanActivo = true,
                PlanID = model.PlanID,
                NombrePlanActual = model.NombrePlanActual,
                Calificacion = model.Calificacion,
                Comentario = model.Comentario,
                SuccessMensaje = "¡Gracias! Tu evaluación fue enviada exitosamente"
            };

            return View("EvaluarPlan", vmGracias);
        }
        public IActionResult ConsultaEvaluaciones()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GetEvaluaciones()
        {
            var lista = new List<object>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = @"
SELECT 
    e.EvaluacionID,
    e.PlanID,
    p.NombrePlan,
    e.UsuarioID,
    u.Nombre AS UsuarioNombre,
    u.Apellido AS UsuarioApellido,
    u.CorreoElectronico AS UsuarioCorreo,
    e.Calificacion,
    e.Comentario,
    e.FechaRegistro
FROM dbo.EvaluarPlanesNutricionales e
LEFT JOIN dbo.PlanesNutricionales p ON e.PlanID = p.PlanID
LEFT JOIN dbo.Usuarios u ON e.UsuarioID = u.UsuarioID
ORDER BY e.FechaRegistro DESC;
";
                using (var cmd = new SqlCommand(sql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int idxEval = reader.GetOrdinal("EvaluacionID");
                            int idxPlan = reader.GetOrdinal("PlanID");
                            int idxNombrePlan = reader.GetOrdinal("NombrePlan");
                            int idxUsuarioID = reader.GetOrdinal("UsuarioID");
                            int idxNombre = reader.GetOrdinal("UsuarioNombre");
                            int idxApellido = reader.GetOrdinal("UsuarioApellido");
                            int idxCorreo = reader.GetOrdinal("UsuarioCorreo");
                            int idxCal = reader.GetOrdinal("Calificacion");
                            int idxCom = reader.GetOrdinal("Comentario");
                            int idxFecha = reader.GetOrdinal("FechaRegistro");

                            // construir nombre de usuario seguro
                            string nombreUsuario = "(Usuario desconocido)";
                            var n = reader.IsDBNull(idxNombre) ? string.Empty : reader.GetString(idxNombre).Trim();
                            var a = reader.IsDBNull(idxApellido) ? string.Empty : reader.GetString(idxApellido).Trim();
                            var c = reader.IsDBNull(idxCorreo) ? string.Empty : reader.GetString(idxCorreo).Trim();

                            if (!string.IsNullOrEmpty(n) || !string.IsNullOrEmpty(a))
                                nombreUsuario = $"{n} {a}".Trim();
                            else if (!string.IsNullOrEmpty(c))
                                nombreUsuario = c;

                            // fecha a ISO (o null)
                            string? fechaIso = null;
                            bool fechaValida = false;
                            if (!reader.IsDBNull(idxFecha))
                            {
                                var val = reader.GetValue(idxFecha);
                                if (val is DateTimeOffset dto)
                                {
                                    fechaIso = dto.ToString("o");
                                    fechaValida = true;
                                }
                                else if (val is DateTime dt)
                                {
                                    // especificar kind local/utc según tu DB
                                    fechaIso = DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToString("o");
                                    fechaValida = true;
                                }
                                else
                                {
                                    fechaIso = val.ToString();
                                }
                            }

                            lista.Add(new
                            {
                                EvaluacionID = reader.GetInt32(idxEval),
                                PlanID = reader.IsDBNull(idxPlan) ? (int?)null : reader.GetInt32(idxPlan),
                                NombrePlan = reader.IsDBNull(idxNombrePlan) ? "(Sin plan)" : reader.GetString(idxNombrePlan),
                                UsuarioID = reader.IsDBNull(idxUsuarioID) ? (int?)null : reader.GetInt32(idxUsuarioID),
                                NombreUsuario = nombreUsuario,
                                Calificacion = reader.IsDBNull(idxCal) ? (int?)null : reader.GetInt32(idxCal),
                                Comentario = reader.IsDBNull(idxCom) ? null : reader.GetString(idxCom),
                                FechaRegistro = (object?)fechaIso ?? null,
                                FechaRegistroValida = fechaValida
                            });
                        }
                    }
                }
            }

            return Json(lista);
        }


    }
}