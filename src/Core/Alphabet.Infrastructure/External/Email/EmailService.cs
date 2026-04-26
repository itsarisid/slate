using Alphabet.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alphabet.Infrastructure.External.Email;

/// <summary>
/// Provides an email sender implementation.
/// </summary>
public sealed class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);
        return Task.CompletedTask;
    }

    public Task SendTemplateAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Sending HTML email to {To} with subject {Subject}", to, subject);
        return Task.CompletedTask;
    }
}
