namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Sends in-application notifications to users.
/// </summary>
public interface IInAppNotificationService
{
    /// <summary>
    /// Sends an in-application notification to the specified user.
    /// </summary>
    Task SendAsync(string userId, string subject, string body, CancellationToken cancellationToken);
}
