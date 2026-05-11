namespace GolfScheduler.Api.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string displayName, string resetLink);
}
