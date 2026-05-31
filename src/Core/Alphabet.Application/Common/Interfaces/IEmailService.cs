namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Sends email messages through an external provider.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send async.
    /// </summary>
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken);
    /// <summary>
    /// Send template async.
    /// </summary>

    Task SendTemplateAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken);
}
