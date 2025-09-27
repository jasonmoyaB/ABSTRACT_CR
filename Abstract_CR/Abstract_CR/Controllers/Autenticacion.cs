using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Abstract_CR.Models;
using Abstract_CR.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Abstract_CR.Controllers
{
    public class AutenticacionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AutenticacionController> _logger;

        public AutenticacionController(ApplicationDbContext context, ILogger<AutenticacionController> logger)
        {
            _context = context;
            _logger = logger;
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
                HttpContext.Session.SetString("Rol", usuario.Rol.NombreRol);
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
            return View();
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


        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
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
}
