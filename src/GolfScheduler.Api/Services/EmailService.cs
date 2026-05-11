using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;
using GolfScheduler.Api.Configuration;

namespace GolfScheduler.Api.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private EmailClient? _emailClient;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    private EmailClient GetClient() =>
        _emailClient ??= new EmailClient(_emailSettings.ConnectionString);

    public async Task SendPasswordResetEmailAsync(string toEmail, string displayName, string resetLink)
    {
        var subject = "Golf Scheduler - Password Reset Request";

        var htmlBody = $"""
            <html>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
              <h2 style="color: #2e7d32;">Password Reset Request</h2>
              <p>Hi {System.Web.HttpUtility.HtmlEncode(displayName)},</p>
              <p>We received a request to reset your Golf Scheduler password. Click the button below to set a new password:</p>
              <p style="margin: 30px 0;">
                <a href="{resetLink}" style="background-color: #2e7d32; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold;">
                  Reset Password
                </a>
              </p>
              <p>This link will expire in <strong>1 hour</strong>.</p>
              <p>If you did not request a password reset, you can ignore this email — your password will remain unchanged.</p>
              <hr style="border: none; border-top: 1px solid #ddd; margin: 20px 0;" />
              <p style="color: #666; font-size: 12px;">If the button above doesn't work, copy and paste this link into your browser:<br />{resetLink}</p>
            </body>
            </html>
            """;

        var plainText = $"""
            Password Reset Request

            Hi {displayName},

            We received a request to reset your Golf Scheduler password.

            To reset your password, visit the following link:
            {resetLink}

            This link will expire in 1 hour.

            If you did not request a password reset, you can ignore this email.
            """;

        var emailContent = new EmailContent(subject)
        {
            Html = htmlBody,
            PlainText = plainText
        };

        var recipients = new EmailRecipients([new EmailAddress(toEmail, displayName)]);
        var message = new EmailMessage(_emailSettings.FromAddress, recipients, emailContent);

        await GetClient().SendAsync(WaitUntil.Completed, message);
    }
}
