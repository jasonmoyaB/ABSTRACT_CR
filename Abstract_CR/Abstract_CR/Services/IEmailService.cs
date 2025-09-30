namespace Abstract_CR.Services
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string userName);
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    }
}
