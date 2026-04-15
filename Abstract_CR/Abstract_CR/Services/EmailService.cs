using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Http;

namespace Abstract_CR.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// URL pública del sitio para enlaces en correos. En Azure, si AppSettings:BaseUrl sigue siendo localhost,
        /// se usa el host de la petición actual (con ForwardedHeaders el esquema será https).
        /// </summary>
        private string GetPublicBaseUrl()
        {
            var configured = _configuration["AppSettings:BaseUrl"]?.TrimEnd('/');
            var configuredIsLocal = string.IsNullOrEmpty(configured)
                || configured.Contains("localhost", StringComparison.OrdinalIgnoreCase)
                || configured.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase);

            var req = _httpContextAccessor.HttpContext?.Request;
            if (req != null && configuredIsLocal)
                return $"{req.Scheme}://{req.Host.Value}";

            if (!string.IsNullOrEmpty(configured) && !configuredIsLocal)
                return configured;

            if (req != null)
                return $"{req.Scheme}://{req.Host.Value}";

            if (!string.IsNullOrEmpty(configured))
                return configured;

            _logger.LogWarning("No se pudo determinar la URL pública (AppSettings:BaseUrl y HttpContext vacíos).");
            return string.Empty;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string userName)
        {
            var baseUrl = GetPublicBaseUrl().TrimEnd('/');
            if (string.IsNullOrEmpty(baseUrl))
            {
                _logger.LogError("No se puede enviar el correo de recuperación: falta la URL base del sitio.");
                return false;
            }

            var tokenEncoded = Uri.EscapeDataString(resetToken);
            var resetUrl = $"{baseUrl}/PasswordReset/ResetPassword?token={tokenEncoded}";
            
            var subject = "Recuperación de Contraseña - Abstract Healthy Food";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; border-radius: 5px; }}
                        .content {{ padding: 20px; }}
                        .button {{ 
                            display: inline-block; 
                            padding: 12px 24px; 
                            background-color: #007bff; 
                            color: white; 
                            text-decoration: none; 
                            border-radius: 5px; 
                            margin: 20px 0;
                        }}
                        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; border-radius: 5px; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>🍎 Abstract Healthy Food</h2>
                            <h3>Recuperación de Contraseña</h3>
                        </div>
                        <div class='content'>
                            <p>Hola <strong>{userName}</strong>,</p>
                            <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta en Abstract Healthy Food.</p>
                            <p>Para crear una nueva contraseña, haz clic en el siguiente botón:</p>
                            <div style='text-align: center;'>
                                <a href='{resetUrl}' class='button'>Restablecer Contraseña</a>
                            </div>
                            <p><strong>Importante:</strong></p>
                            <ul>
                                <li>Este enlace expirará en 24 horas</li>
                                <li>Si no solicitaste este cambio, puedes ignorar este correo</li>
                                <li>Tu contraseña actual seguirá funcionando hasta que la cambies</li>
                            </ul>
                            <p>Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
                            <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 3px;'>{resetUrl}</p>
                        </div>
                        <div class='footer'>
                            <p>Este correo fue enviado desde Abstract Healthy Food</p>
                            <p>Si tienes alguna pregunta, no dudes en contactarnos</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:Username"];
                var smtpPassword = _configuration["EmailSettings:Password"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogError("Configuración de email incompleta en appsettings.json");
                    return false;
                }

                _logger.LogInformation($"Intentando enviar email a {toEmail} desde {smtpUsername}");

                using var client = new SmtpClient(smtpServer, smtpPort);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.Timeout = 30000; // 30 segundos timeout

                var message = new MailMessage();
                message.From = new MailAddress(fromEmail ?? smtpUsername, fromName);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                await client.SendMailAsync(message);
                
                _logger.LogInformation($"Email enviado exitosamente a {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enviando email a {toEmail}: {ex.Message}");
                
                // Log específico para diferentes tipos de errores
                if (ex.Message.Contains("Authentication failed"))
                {
                    _logger.LogError("Error de autenticación. Verifica la contraseña de aplicación de Gmail.");
                }
                else if (ex.Message.Contains("Connection refused"))
                {
                    _logger.LogError("Error de conexión. Verifica la configuración de SMTP.");
                }
                
                return false;
            }
        }
    }
}
