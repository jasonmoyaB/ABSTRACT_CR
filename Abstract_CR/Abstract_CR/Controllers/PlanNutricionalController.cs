using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Abstract_CR.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace Abstract_CR.Controllers
{
    // [Authorize] // Comentado temporalmente - descomentar cuando tengas autenticación
    public class PlanNutricionalController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public PlanNutricionalController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                EliminarPlan(id);
                TempData["Success"] = "Plan nutricional eliminado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el plan: " + ex.Message;
                return RedirectToAction(nameof(Index));
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
        public IActionResult Notificaciones()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioID");
            if (usuarioId == null)
            {
                TempData["Error"] = "Debes iniciar sesión para ver tus notificaciones.";
                return RedirectToAction("Login", "Autenticacion");
            }

            var notificaciones = new List<Notificacion>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var sql = @"
        SELECT NotificacionID, UsuarioID, Mensaje, Tipo, FechaEnvio
        FROM dbo.Notificaciones
        WHERE UsuarioID = @UsuarioID
        ORDER BY FechaEnvio DESC";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UsuarioID", usuarioId.Value);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                notificaciones.Add(new Notificacion
                {
                    NotificacionID = reader.GetInt32("NotificacionID"),
                    UsuarioID = reader.GetInt32("UsuarioID"),
                    Mensaje = reader.GetString("Mensaje"),
                    Tipo = reader.GetString("Tipo"),
                    FechaEnvio = reader.GetDateTime("FechaEnvio")
                });
            }

            return View(notificaciones);
        }
    }
}
