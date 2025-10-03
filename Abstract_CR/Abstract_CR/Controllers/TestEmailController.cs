using Microsoft.AspNetCore.Mvc;
using Abstract_CR.Services;

namespace Abstract_CR.Controllers
{
    public class TestEmailController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<TestEmailController> _logger;

        public TestEmailController(IEmailService emailService, ILogger<TestEmailController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendTestEmail(string testEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(testEmail))
                {
                    TempData["Error"] = "Por favor ingresa un email válido";
                    return View("Index");
                }

                var subject = "Test Email - Abstract Healthy Food";
                var body = @"
                    <html>
                    <body>
                        <h2>🧪 Email de Prueba</h2>
                        <p>Si recibes este email, la configuración de correo está funcionando correctamente.</p>
                        <p><strong>Fecha:</strong> " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + @"</p>
                        <p><strong>Desde:</strong> Abstract Healthy Food</p>
                        <hr>
                        <p><em>Este es un email de prueba. Puedes ignorarlo.</em></p>
                    </body>
                    </html>";

                var result = await _emailService.SendEmailAsync(testEmail, subject, body);

                if (result)
                {
                    TempData["Success"] = $"Email de prueba enviado exitosamente a {testEmail}";
                }
                else
                {
                    TempData["Error"] = "Error enviando el email. Revisa los logs para más detalles.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en prueba de email");
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return View("Index");
        }
    }
}
