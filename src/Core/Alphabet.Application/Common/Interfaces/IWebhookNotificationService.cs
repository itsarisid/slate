namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Sends outbound webhook notifications.
/// </summary>
public interface IWebhookNotificationService
{
    /// <summary>
    /// Sends a webhook payload to the specified URL.
    /// </summary>
    Task SendAsync(string url, string subject, string body, CancellationToken cancellationToken);
}
