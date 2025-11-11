using System.Net;
using System.Net.Mail;
using System.Text;

namespace Abstract_CR.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendGridEmailService> _logger;

        public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string userName)
        {
            var resetUrl = $"{_configuration["AppSettings:BaseUrl"]}/Autenticacion/ResetPassword?token={resetToken}";
            
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
                // Configuración alternativa usando un servicio SMTP genérico
                var smtpServer = "smtp.gmail.com";
                var smtpPort = 587;
                var smtpUsername = _configuration["EmailSettings:Username"];
                var smtpPassword = _configuration["EmailSettings:Password"];

                _logger.LogInformation($"Enviando email usando SendGrid compatible a {toEmail}");

                using var client = new SmtpClient(smtpServer, smtpPort);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                var message = new MailMessage();
                message.From = new MailAddress(smtpUsername, "Abstract Healthy Food");
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
                return false;
            }
        }
    }
}
