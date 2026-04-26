namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Sends email messages through an external provider.
/// </summary>
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken);

    Task SendTemplateAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken);
}
