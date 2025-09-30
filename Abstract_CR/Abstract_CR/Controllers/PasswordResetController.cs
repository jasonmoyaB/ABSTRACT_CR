using Microsoft.AspNetCore.Mvc;
using Abstract_CR.Data;
using Abstract_CR.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Abstract_CR.Controllers
{
    public class PasswordResetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordResetController> _logger;

        // Diccionario temporal para almacenar tokens (en producción usar Redis o similar)
        private static readonly Dictionary<string, PasswordResetInfo> _resetTokens = new();

        public PasswordResetController(ApplicationDbContext context, IEmailService emailService, ILogger<PasswordResetController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult RequestReset()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RequestReset(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    TempData["Error"] = "Por favor, ingresa un email válido.";
                    return View();
                }

                _logger.LogInformation($"Buscando usuario con email: {email}");

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.CorreoElectronico == email);

                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario no encontrado: {email}");
                    TempData["Error"] = "No se encontró un usuario con ese email.";
                    return View();
                }

                // Generar token único
                var token = GenerateSecureToken();
                
                // Almacenar token temporalmente (válido por 30 minutos)
                _resetTokens[token] = new PasswordResetInfo
                {
                    UsuarioId = usuario.UsuarioID,
                    Email = email,
                    Expiration = DateTime.UtcNow.AddMinutes(30)
                };

                _logger.LogInformation($"Token generado para {email}: {token.Substring(0, 8)}...");

                // Enviar email
                var emailSent = await _emailService.SendPasswordResetEmailAsync(email, token, usuario.Nombre);
                
                if (emailSent)
                {
                    _logger.LogInformation($"Email de recuperación enviado a: {email}");
                    TempData["Success"] = "Se ha enviado un enlace de recuperación a tu email.";
                    return View();
                }
                else
                {
                    _logger.LogError($"Error enviando email a: {email}");
                    TempData["Error"] = "Error enviando el email. Inténtalo de nuevo.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en RequestReset para {email}");
                TempData["Error"] = "Ocurrió un error inesperado. Inténtalo de nuevo.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Token inválido.";
                return RedirectToAction("RequestReset");
            }

            if (!_resetTokens.ContainsKey(token))
            {
                TempData["Error"] = "Token inválido o expirado.";
                return RedirectToAction("RequestReset");
            }

            var tokenInfo = _resetTokens[token];
            if (tokenInfo.Expiration < DateTime.UtcNow)
            {
                _resetTokens.Remove(token);
                TempData["Error"] = "Token expirado. Solicita uno nuevo.";
                return RedirectToAction("RequestReset");
            }

            ViewBag.Token = token;
            ViewBag.Email = tokenInfo.Email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string password, string confirmPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    TempData["Error"] = "Token inválido.";
                    return RedirectToAction("RequestReset");
                }

                if (!_resetTokens.ContainsKey(token))
                {
                    TempData["Error"] = "Token inválido o expirado.";
                    return RedirectToAction("RequestReset");
                }

                var tokenInfo = _resetTokens[token];
                if (tokenInfo.Expiration < DateTime.UtcNow)
                {
                    _resetTokens.Remove(token);
                    TempData["Error"] = "Token expirado. Solicita uno nuevo.";
                    return RedirectToAction("RequestReset");
                }

                if (string.IsNullOrEmpty(password) || password.Length < 6)
                {
                    TempData["Error"] = "La contraseña debe tener al menos 6 caracteres.";
                    ViewBag.Token = token;
                    ViewBag.Email = tokenInfo.Email;
                    return View();
                }

                if (password != confirmPassword)
                {
                    TempData["Error"] = "Las contraseñas no coinciden.";
                    ViewBag.Token = token;
                    ViewBag.Email = tokenInfo.Email;
                    return View();
                }

                // Buscar usuario
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.UsuarioID == tokenInfo.UsuarioId);

                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("RequestReset");
                }

                // Actualizar contraseña
                usuario.ContrasenaHash = HashPassword(password);
                await _context.SaveChangesAsync();

                // Eliminar token usado
                _resetTokens.Remove(token);

                _logger.LogInformation($"Contraseña actualizada para usuario: {tokenInfo.Email}");

                TempData["Success"] = "Contraseña actualizada exitosamente. Ya puedes iniciar sesión.";
                return RedirectToAction("Login", "Autenticacion");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error actualizando contraseña para token: {token}");
                TempData["Error"] = "Error actualizando la contraseña. Inténtalo de nuevo.";
                return RedirectToAction("RequestReset");
            }
        }

        private string GenerateSecureToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private class PasswordResetInfo
        {
            public int UsuarioId { get; set; }
            public string Email { get; set; } = string.Empty;
            public DateTime Expiration { get; set; }
        }
    }
}
