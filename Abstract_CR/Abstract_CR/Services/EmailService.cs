using System.Net.Http.Headers;
using System.Text.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using MimeKit;

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
            var resendKey = _configuration["EmailSettings:ResendApiKey"];
            if (!string.IsNullOrWhiteSpace(resendKey))
            {
                _logger.LogInformation("Enviando correo vía Resend API a {To}", toEmail);
                return await SendViaResendAsync(toEmail, subject, body, resendKey.Trim());
            }

            var sendGridKey = _configuration["EmailSettings:SendGridApiKey"];
            if (!string.IsNullOrWhiteSpace(sendGridKey))
            {
                _logger.LogInformation("Enviando correo vía SendGrid API a {To}", toEmail);
                return await SendViaSendGridAsync(toEmail, subject, body, sendGridKey.Trim());
            }

            return await SendViaSmtpAsync(toEmail, subject, body);
        }

        /// <summary>Resend: plan gratuito; requiere dominio verificado (o dominio de prueba según su política).</summary>
        private async Task<bool> SendViaResendAsync(string toEmail, string subject, string body, string apiKey)
        {
            try
            {
                var fromEmail = _configuration["EmailSettings:FromEmail"]?.Trim();
                var fromName = _configuration["EmailSettings:FromName"]?.Trim() ?? "Abstract Healthy Food";
                if (string.IsNullOrEmpty(fromEmail))
                    fromEmail = _configuration["EmailSettings:Username"]?.Trim();

                if (string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogError("Resend: falta EmailSettings:FromEmail para el remitente.");
                    return false;
                }

                var fromHeader = $"{fromName} <{fromEmail}>";
                var payload = new
                {
                    from = fromHeader,
                    to = new[] { toEmail },
                    subject,
                    html = body
                };

                var json = JsonSerializer.Serialize(payload);

                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Resend: correo enviado a {To}", toEmail);
                    return true;
                }

                var errBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Resend rechazó el envío. Status={Status}. Respuesta={Body}. Verifica dominio/remitente en Resend.",
                    (int)response.StatusCode,
                    errBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resend: error al enviar a {To}", toEmail);
                return false;
            }
        }

        private async Task<bool> SendViaSendGridAsync(string toEmail, string subject, string body, string apiKey)
        {
            try
            {
                var fromEmail = _configuration["EmailSettings:FromEmail"]?.Trim();
                var fromName = _configuration["EmailSettings:FromName"]?.Trim() ?? "Abstract Healthy Food";
                if (string.IsNullOrEmpty(fromEmail))
                {
                    fromEmail = _configuration["EmailSettings:Username"]?.Trim();
                }

                if (string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogError("SendGrid: falta EmailSettings:FromEmail (o Username) para el remitente.");
                    return false;
                }

                var payload = new
                {
                    personalizations = new[] { new { to = new[] { new { email = toEmail } } } },
                    from = new { email = fromEmail, name = fromName },
                    subject,
                    content = new[] { new { type = "text/html", value = body } }
                };

                var json = JsonSerializer.Serialize(payload);

                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await http.SendAsync(request);
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    _logger.LogInformation("SendGrid: correo aceptado para {To}", toEmail);
                    return true;
                }

                var errBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "SendGrid rechazó el envío. Status={Status}. Respuesta={Body}. Verifica remitente verificado en SendGrid y la API key.",
                    (int)response.StatusCode,
                    errBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendGrid: error de red o configuración al enviar a {To}", toEmail);
                return false;
            }
        }

        private async Task<bool> SendViaSmtpAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = 587;
                if (int.TryParse(_configuration["EmailSettings:SmtpPort"], out var parsedPort) && parsedPort > 0)
                    smtpPort = parsedPort;

                var smtpUsername = _configuration["EmailSettings:Username"];
                var smtpPassword = _configuration["EmailSettings:Password"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogError(
                        "Configuración SMTP incompleta. Opciones: EmailSettings__ResendApiKey, EmailSettings__SendGridApiKey, o SMTP (p. ej. Brevo) en Azure.");
                    return false;
                }

                if (smtpPassword.Contains("Your_Email", StringComparison.OrdinalIgnoreCase) ||
                    smtpPassword.Contains("Here", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError("La contraseña SMTP parece un placeholder; usa credenciales reales o Resend/SendGrid (API key) en Azure.");
                    return false;
                }

                _logger.LogInformation("Enviando correo por SMTP ({Host}:{Port}) a {To} como {User}", smtpServer, smtpPort, toEmail, smtpUsername);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName ?? "Abstract Healthy Food", fromEmail ?? smtpUsername));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };

                using var client = new SmtpClient();
                var socketOptions = smtpPort == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

                await client.ConnectAsync(smtpServer, smtpPort, socketOptions);
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email enviado exitosamente a {To}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error SMTP a {To}: {Message}. En Azure Gmail suele fallar; prueba Brevo SMTP (gratis) o EmailSettings__ResendApiKey.",
                    toEmail,
                    ex.Message);

                if (ex.Message.Contains("Authentication", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("535", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError("Fallo de autenticación SMTP: contraseña de aplicación incorrecta o cuenta bloqueada para este origen.");
                }

                return false;
            }
        }
    }
}
