namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Sends push notifications to registered devices.
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Sends a push notification to the specified device token.
    /// </summary>
    Task SendAsync(string token, string subject, string body, CancellationToken cancellationToken);
}
