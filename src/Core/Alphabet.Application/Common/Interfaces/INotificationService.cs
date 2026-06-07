namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Sends cross-module notifications through shared abstractions.
/// </summary>
public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken);

    Task SendPushNotificationAsync(string userId, string title, string message, CancellationToken cancellationToken);
}
