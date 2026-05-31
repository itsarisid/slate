namespace Alphabet.Application.Common.Interfaces.Productivity;

/// <summary>
/// Sends productivity notifications through configured channels.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send reminder async.
    /// </summary>
    Task SendReminderAsync(Guid userId, string title, string body, IReadOnlyCollection<string> channels, CancellationToken cancellationToken);
    /// <summary>
    /// Send assignment async.
    /// </summary>

    Task SendAssignmentAsync(Guid userId, string title, string body, CancellationToken cancellationToken);
}
