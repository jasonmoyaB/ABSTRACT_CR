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
        private readonly ILogger<PlanNutricionalController> _logger;


        private readonly IEmailService _emailService;


        public PlanNutricionalController(IConfiguration configuration, IEmailService emailService, ILogger<PlanNutricionalController> logger)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _emailService = emailService;
            _logger = logger;
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


        //NOTIFICACIONES PLAN
        [HttpGet]
        public async Task<IActionResult> Notificaciones()
        {
            var usuarioIdNullable = HttpContext.Session.GetInt32("UsuarioID");
            if (!usuarioIdNullable.HasValue)
            {
                TempData["Error"] = "Debes iniciar sesión para ver tus notificaciones.";
                return RedirectToAction("Login", "Autenticacion");
            }

            int usuarioId = usuarioIdNullable.Value;

            // Procesamos notificaciones para el usuario (inserta y envía email cuando corresponda)
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
                commandPlanes.Parameters.AddWithValue("@UsuarioID", usuarioId);
                using var reader = commandPlanes.ExecuteReader();
                while (reader.Read())
                {
                    planes.Add((
                        PlanID: reader.GetInt32(reader.GetOrdinal("PlanID")),
                        Descripcion: reader["Descripcion"]?.ToString() ?? string.Empty,
                        FechaVencimiento: reader.GetDateTime(reader.GetOrdinal("FechaVencimiento"))
                    ));
                }
            }

            var emailUsuario = ObtenerEmailUsuario(usuarioId);

            foreach (var plan in planes)
            {
                var diasRestantes = (plan.FechaVencimiento.Date - DateTime.Now.Date).Days;

                if (diasRestantes is not (7 or 3 or 1 or 0))
                    continue;

                var token = $"[PID={plan.PlanID};UMBRAL={diasRestantes}]";

                string mensajeLimpio = diasRestantes switch
                {
                    7 => $"7 días antes del vencimiento: Te avisamos que tu plan \"{plan.Descripcion ?? ""}\" expirará en una semana.",
                    3 => $"3 días antes del vencimiento: Un recordatorio cercano para que no olvides tu plan \"{plan.Descripcion ?? ""}\".",
                    1 => $"1 día antes del vencimiento: Último aviso para que tomes acción sobre tu plan \"{plan.Descripcion ?? ""}\" antes de que caduque.",
                    0 => $"El día del vencimiento: Tu plan \"{plan.Descripcion ?? ""}\" ha llegado a su fecha final.",
                    _ => string.Empty
                };

                if (string.IsNullOrWhiteSpace(mensajeLimpio))
                    continue;

                // Evitar duplicados por token
                try
                {
                    if (YaSeEnvioVencimiento(connection, usuarioId, token))
                        continue;
                }
                catch (Exception exYa)
                {
                    _logger?.LogError(exYa, "Error comprobando duplicado de notificación para Usuario {UsuarioId}, Plan {PlanId}", usuarioId, plan.PlanID);
                    // Si no podemos comprobar duplicado, saltamos este plan para evitar envíos erróneos
                    continue;
                }

                var mensajeConToken = $"{token} {mensajeLimpio}";

                // Intentar insertar la notificación y recuperar el ID insertado.
                int? notificacionId = null;
                try
                {
                    // Primero intento con OUTPUT INSERTED.NotificacionID (si tu tabla tiene NotificacionID identity)
                    var sqlInsertOutput = @"
                INSERT INTO dbo.Notificaciones (UsuarioID, Mensaje, Tipo, FechaEnvio)
                OUTPUT INSERTED.NotificacionID
                VALUES (@UsuarioID, @Mensaje, @Tipo, @FechaEnvio);
            ";

                    using var cmdInsertOut = new SqlCommand(sqlInsertOutput, connection);
                    cmdInsertOut.Parameters.AddWithValue("@UsuarioID", usuarioId);
                    cmdInsertOut.Parameters.AddWithValue("@Mensaje", mensajeConToken);
                    cmdInsertOut.Parameters.AddWithValue("@Tipo", "Vencimiento");
                    cmdInsertOut.Parameters.AddWithValue("@FechaEnvio", DateTime.Now);

                    var scalarOut = cmdInsertOut.ExecuteScalar();
                    if (scalarOut != null && scalarOut != DBNull.Value)
                        notificacionId = Convert.ToInt32(scalarOut);
                }
                catch (SqlException exOut)
                {
                    _logger?.LogWarning(exOut, "OUTPUT INSERT falló al insertar notificación. Intentando fallback con SCOPE_IDENTITY()");

                    // Fallback a SCOPE_IDENTITY()
                    try
                    {
                        var sqlInsertScope = @"
                    INSERT INTO dbo.Notificaciones (UsuarioID, Mensaje, Tipo, FechaEnvio)
                    VALUES (@UsuarioID, @Mensaje, @Tipo, @FechaEnvio);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);
                ";
                        using var cmdInsertScope = new SqlCommand(sqlInsertScope, connection);
                        cmdInsertScope.Parameters.AddWithValue("@UsuarioID", usuarioId);
                        cmdInsertScope.Parameters.AddWithValue("@Mensaje", mensajeConToken);
                        cmdInsertScope.Parameters.AddWithValue("@Tipo", "Vencimiento");
                        cmdInsertScope.Parameters.AddWithValue("@FechaEnvio", DateTime.Now);

                        var scalarScope = cmdInsertScope.ExecuteScalar();
                        if (scalarScope != null && scalarScope != DBNull.Value)
                            notificacionId = Convert.ToInt32(scalarScope);
                    }
                    catch (Exception exScope)
                    {
                        _logger?.LogError(exScope, "Fallo el INSERT de notificación (fallback). Usuario {UsuarioId}, Plan {PlanId}", usuarioId, plan.PlanID);
                        // no lanzamos, pero no continuamos con envío de email si no se pudo insertar
                        continue;
                    }
                }
                catch (Exception exGeneralInsert)
                {
                    _logger?.LogError(exGeneralInsert, "Error insertando notificación para Usuario {UsuarioId}, Plan {PlanId}", usuarioId, plan.PlanID);
                    continue;
                }

                // Añadimos a la lista que se mostrará en la vista (sin token visible)
                notificaciones.Add(new Notificacion
                {
                    UsuarioID = usuarioId,
                    Mensaje = mensajeLimpio,
                    Tipo = "Vencimiento",
                    FechaEnvio = DateTime.Now
                });

                // Intentamos enviar el correo (si hay email)
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

                    try
                    {
                        await _emailService.SendEmailAsync(emailUsuario, subject, body);
                    }
                    catch (Exception ex)
                    {
                        // Registra el error para diagnóstico
                        _logger?.LogError(ex, "Error enviando email de vencimiento al usuario {UsuarioId} (email: {Email}) para PlanID {PlanID}", usuarioId, emailUsuario, plan.PlanID);

                        // Marcar la notificación en BD con el error para trazabilidad (si tenemos el id insertado)
                        if (notificacionId.HasValue)
                        {
                            try
                            {
                                var sqlUpdateError = @"
                            UPDATE dbo.Notificaciones
                            SET Mensaje = Mensaje + ' [ERROR-ENVIO: ' + LEFT(@ErrorMsg, 200) + ']'
                            WHERE NotificacionID = @NotificacionID;
                        ";
                                using var cmdUpd = new SqlCommand(sqlUpdateError, connection);
                                cmdUpd.Parameters.AddWithValue("@ErrorMsg", ex.Message ?? "Error desconocido");
                                cmdUpd.Parameters.AddWithValue("@NotificacionID", notificacionId.Value);
                                cmdUpd.ExecuteNonQuery();
                            }
                            catch (Exception updEx)
                            {
                                _logger?.LogWarning(updEx, "No se pudo actualizar la notificación con el error de envío para NotificacionID {Id}", notificacionId.Value);
                            }
                        }
                    }
                }
            } // foreach planes

            notificaciones = notificaciones.OrderBy(n => n.FechaEnvio).ToList();
            return View(notificaciones);
        }

        #region Helpers

        private string? ObtenerEmailUsuario(int usuarioId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                const string sql = @"
            SELECT TOP 1 CorreoElectronico
            FROM dbo.Usuarios
            WHERE UsuarioID = @UsuarioID;
        ";

                using var cmd = new SqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);

                var result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value) return null;

                var email = Convert.ToString(result)?.Trim();
                return string.IsNullOrWhiteSpace(email) ? null : email;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error obteniendo email del usuario {UsuarioId}", usuarioId);
                return null;
            }
        }

        /// <summary>
        /// Comprueba si ya se envió una notificación de vencimiento para (usuario, token).
        /// Usa la conexión existente para evitar crear nuevas conexiones dentro de bucles.
        /// </summary>
        private bool YaSeEnvioVencimiento(SqlConnection connection, int usuarioId, string token)
        {
            const string sql = @"
        SELECT COUNT(1)
        FROM dbo.Notificaciones
        WHERE UsuarioID = @UsuarioID
          AND Tipo = 'Vencimiento'
          AND CHARINDEX(@Token, Mensaje) > 0;
    ";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@Token", token);

            var result = cmd.ExecuteScalar();
            if (result == null || result == DBNull.Value) return false;
            return Convert.ToInt32(result) > 0;
        }

        #endregion

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
