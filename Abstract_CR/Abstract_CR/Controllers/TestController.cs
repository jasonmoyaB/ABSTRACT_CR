using Microsoft.AspNetCore.Mvc;
using Abstract_CR.Data;
using Abstract_CR.Models;
using Microsoft.EntityFrameworkCore;

namespace Abstract_CR.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TestController> _logger;

        public TestController(ApplicationDbContext context, ILogger<TestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> CreateTestUser()
        {
            try
            {
                // Verificar si ya existe el usuario de prueba
                var existingUser = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.CorreoElectronico == "jerevf@gmail.com");

                if (existingUser != null)
                {
                    return Json(new { 
                        success = true, 
                        message = "Usuario de prueba ya existe",
                        userId = existingUser.UsuarioID,
                        email = existingUser.CorreoElectronico
                    });
                }

                // Crear usuario de prueba
                var testUser = new Usuario
                {
                    Nombre = "Jeremy",
                    Apellido = "Test",
                    CorreoElectronico = "jerevf@gmail.com",
                    ContrasenaHash = HashPassword("123456"),
                    RolID = 2, // Cliente
                    FechaRegistro = DateTime.Now,
                    Activo = true
                };

                _context.Usuarios.Add(testUser);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Usuario de prueba creado exitosamente",
                    userId = testUser.UsuarioID,
                    email = testUser.CorreoElectronico
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando usuario de prueba");
                return Json(new { 
                    success = false, 
                    message = $"Error: {ex.Message}" 
                });
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
