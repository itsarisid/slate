namespace Alphabet.Application.Common.Interfaces.Productivity;

/// <summary>
/// Sends productivity notifications through configured channels.
/// </summary>
public interface INotificationService
{
    Task SendReminderAsync(Guid userId, string title, string body, IReadOnlyCollection<string> channels, CancellationToken cancellationToken);

    Task SendAssignmentAsync(Guid userId, string title, string body, CancellationToken cancellationToken);
}
