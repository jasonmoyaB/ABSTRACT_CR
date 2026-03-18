using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Abstract_CR.Data;
using Abstract_CR.Models;
using Abstract_CR.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Abstract_CR.Controllers
{
    public class PasswordResetController : Controller
    {
        private const int TokenExpirationHours = 24;

        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordResetController> _logger;

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

                // Invalidar tokens previos del usuario
                var tokensPrevios = await _context.Tokens
                    .Where(t => t.UsuarioID == usuario.UsuarioID)
                    .ToListAsync();
                _context.Tokens.RemoveRange(tokensPrevios);
                await _context.SaveChangesAsync();

                // Generar token único (URL-safe para que no se corrompa en el enlace)
                var token = GenerateUrlSafeToken();

                var nuevoToken = new PassResetTokens
                {
                    UsuarioID = usuario.UsuarioID,
                    Token = token,
                    FechaCreacion = DateTime.UtcNow
                };
                _context.Tokens.Add(nuevoToken);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Token generado para {email}: {token.Substring(0, Math.Min(8, token.Length))}...");

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
                var mensaje = "Ocurrió un error inesperado. Inténtalo de nuevo.";
                if (HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
                {
                    mensaje = $"{mensaje} (Detalle: {ex.Message})";
                }
                TempData["Error"] = mensaje;
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Token inválido.";
                return RedirectToAction("RequestReset");
            }

            var resetToken = await _context.Tokens
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.Token == token);

            if (resetToken == null)
            {
                TempData["Error"] = "Token inválido o expirado.";
                return RedirectToAction("RequestReset");
            }

            if (resetToken.FechaCreacion.AddHours(TokenExpirationHours) < DateTime.UtcNow)
            {
                _context.Tokens.Remove(resetToken);
                await _context.SaveChangesAsync();
                TempData["Error"] = "Token expirado. Solicita uno nuevo.";
                return RedirectToAction("RequestReset");
            }

            ViewBag.Token = token;
            ViewBag.Email = resetToken.Usuario?.CorreoElectronico ?? "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string token, string password, string confirmPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    TempData["Error"] = "Token inválido.";
                    return RedirectToAction("RequestReset");
                }

                var resetToken = await _context.Tokens
                    .Include(t => t.Usuario)
                    .FirstOrDefaultAsync(t => t.Token == token);

                if (resetToken == null)
                {
                    TempData["Error"] = "Token inválido o expirado.";
                    return RedirectToAction("RequestReset");
                }

                if (resetToken.FechaCreacion.AddHours(TokenExpirationHours) < DateTime.UtcNow)
                {
                    _context.Tokens.Remove(resetToken);
                    await _context.SaveChangesAsync();
                    TempData["Error"] = "Token expirado. Solicita uno nuevo.";
                    return RedirectToAction("RequestReset");
                }

                if (string.IsNullOrEmpty(password) || password.Length < 6)
                {
                    TempData["Error"] = "La contraseña debe tener al menos 6 caracteres.";
                    ViewBag.Token = token;
                    ViewBag.Email = resetToken.Usuario?.CorreoElectronico ?? "";
                    return View();
                }

                if (password != confirmPassword)
                {
                    TempData["Error"] = "Las contraseñas no coinciden.";
                    ViewBag.Token = token;
                    ViewBag.Email = resetToken.Usuario?.CorreoElectronico ?? "";
                    return View();
                }

                var usuario = resetToken.Usuario;
                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("RequestReset");
                }

                usuario.ContrasenaHash = HashPassword(password);
                _context.Tokens.Remove(resetToken);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Contraseña actualizada para usuario: {resetToken.Usuario?.CorreoElectronico}");

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

        /// <summary>
        /// Genera un token seguro compatible con URLs (evita +, /, = que se corrompen en enlaces)
        /// </summary>
        private static string GenerateUrlSafeToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
