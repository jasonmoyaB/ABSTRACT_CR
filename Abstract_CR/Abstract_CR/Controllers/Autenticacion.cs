using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Abstract_CR.Models;
using Abstract_CR.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Abstract_CR.Helpers;
using Abstract_CR.Services;

namespace Abstract_CR.Controllers
{
    public class AutenticacionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AutenticacionController> _logger;
        private readonly UserHelper _userHelper;
        private readonly IEmailService _emailService;

        public AutenticacionController(ApplicationDbContext context, ILogger<AutenticacionController> logger, UserHelper userHelper, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _userHelper = userHelper;
            _emailService = emailService;
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Buscar usuario por email
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.CorreoElectronico == model.Email && u.Activo);

                //var usuarioExistente = _userHelper.ObtenerUsuarioPorCorreo(model.Email);

                if (usuario == null)
                {
                    ModelState.AddModelError(string.Empty, "Credenciales inválidas");
                    return View(model);
                }

                // Verificar contraseña (por ahora simple, después implementaremos hash)
                if (usuario.ContrasenaHash != HashPassword(model.Password))
                {
                    ModelState.AddModelError(string.Empty, "Credenciales inválidas");
                    return View(model);
                }

                // Configurar sesión
                HttpContext.Session.SetInt32("UsuarioID", usuario.UsuarioID);
                HttpContext.Session.SetString("NombreUsuario", usuario.NombreCompleto);
                HttpContext.Session.SetString("Rol", usuario.Rol?.NombreRol ?? "Cliente");
                HttpContext.Session.SetString("Email", usuario.CorreoElectronico);

                _logger.LogInformation($"Usuario {usuario.CorreoElectronico} inició sesión");

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                ModelState.AddModelError(string.Empty, "Ocurrió un error durante el inicio de sesión");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult CrearCuenta()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCuenta(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Verificar si el email ya existe
                var usuarioExistente = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.CorreoElectronico == model.Email);
                
                //var usuarioExistente = _userHelper.ObtenerUsuarioPorCorreo(model.Email);

                if (usuarioExistente != null)
                {
                    ModelState.AddModelError("Email", "Este correo electrónico ya está registrado");
                    return View(model);
                }

                // Crear nuevo usuario
                var nuevoUsuario = new Usuario
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    CorreoElectronico = model.Email,
                    ContrasenaHash = HashPassword(model.Password),
                    RolID = 2, // Rol de Cliente por defecto
                    FechaRegistro = DateTime.Now,
                    Activo = true
                };

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nuevo usuario registrado: {model.Email}");

                TempData["Mensaje"] = "¡Cuenta creada exitosamente! Ya puedes iniciar sesión.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro");
                ModelState.AddModelError(string.Empty, "Ocurrió un error durante el registro");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult RecuperarContraseña()
        {
            return View(new RecuperarContraseniaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarContrasenia(RecuperarContraseniaViewModel model)
        {
            try
            {
                _logger.LogInformation($"Intentando recuperar contraseña para: {model.Email}");

                // Buscar usuario por email
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.CorreoElectronico == model.Email && u.Activo);

                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario no encontrado: {model.Email}");
                    TempData["Mensaje"] = "Si el email está registrado, recibirás un enlace para restablecer tu contraseña.";
                    return RedirectToAction("Login");
                }

                _logger.LogInformation($"Usuario encontrado: {usuario.NombreCompleto}");

                // Crear token simple
                var resetToken = Guid.NewGuid().ToString().Replace("-", "");
                
                // Crear registro en base de datos (solo campos que existen)
                var nuevoToken = new PassResetTokens
                {
                    UsuarioID = usuario.UsuarioID,
                    Token = resetToken,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Tokens.Add(nuevoToken);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Token creado: {resetToken}");

                // Enviar email
                try
                {
                    var emailEnviado = await _emailService.SendPasswordResetEmailAsync(
                        usuario.CorreoElectronico, 
                        resetToken, 
                        usuario.NombreCompleto);

                    if (emailEnviado)
                    {
                        _logger.LogInformation($"✅ Email enviado exitosamente a {usuario.CorreoElectronico}");
                        TempData["Mensaje"] = "✅ Email enviado exitosamente. Revisa tu bandeja de entrada.";
                    }
                    else
                    {
                        _logger.LogError($"❌ Error enviando email a {usuario.CorreoElectronico}");
                        TempData["Error"] = "❌ Error enviando el email. Revisa la configuración.";
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, $"❌ Error en envío de email: {emailEx.Message}");
                    TempData["Error"] = $"❌ Error en envío de email: {emailEx.Message}";
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error general en recuperación: {ex.Message}");
                TempData["Error"] = $"❌ Error: {ex.Message}";
                return RedirectToAction("Login");
            }
        }
        // Muestra la vista de confirmación de cierre de sesión
        // GET: muestra la página de confirmación de cierre de sesión
        [HttpGet]
        public IActionResult CerrarSesion()
        {
            return View(); // devuelve la vista CerrarSesion.cshtml
        }

        // POST: realiza el logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // elimina toda la sesión
            return RedirectToAction("Index", "Home"); // redirige al inicio
        }


        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Token de recuperación inválido.";
                return RedirectToAction("Login");
            }

            var resetToken = await _context.Tokens
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.Token == token);

            if (resetToken == null)
            {
                TempData["Error"] = "El enlace de recuperación ha expirado o es inválido.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = resetToken.Usuario?.CorreoElectronico ?? ""
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var resetToken = await _context.Tokens
                    .Include(t => t.Usuario)
                    .FirstOrDefaultAsync(t => t.Token == model.Token);

                if (resetToken == null)
                {
                    TempData["Error"] = "El enlace de recuperación ha expirado o es inválido.";
                    return RedirectToAction("Login");
                }

                // Actualizar contraseña del usuario
                resetToken.Usuario.ContrasenaHash = HashPassword(model.Password);
                
                // Eliminar el token usado
                _context.Tokens.Remove(resetToken);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Contraseña restablecida para usuario {resetToken.Usuario?.CorreoElectronico}");

                TempData["Mensaje"] = "Tu contraseña ha sido restablecida exitosamente. Ya puedes iniciar sesión.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restableciendo contraseña");
                TempData["Error"] = "Ocurrió un error. Por favor, inténtalo de nuevo.";
                return View(model);
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private string GenerateResetToken()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }

    // ViewModels para el formulario
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [Display(Name = "Apellido")]
        [StringLength(100, ErrorMessage = "El apellido no puede tener más de 100 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de contraseña es obligatoria")]
        [Display(Name = "Confirmar Contraseña")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class RecuperarContraseniaViewModel
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [Display(Name = "Nueva Contraseña")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de contraseña es obligatoria")]
        [Display(Name = "Confirmar Nueva Contraseña")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
