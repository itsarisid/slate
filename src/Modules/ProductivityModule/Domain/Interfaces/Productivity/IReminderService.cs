using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces.Productivity;

/// <summary>
/// Schedules and maintains reminder execution.
/// </summary>
public interface IReminderService
{
    Task ScheduleAsync(Reminder reminder, CancellationToken cancellationToken);

    Task RescheduleAsync(Reminder reminder, CancellationToken cancellationToken);

    Task CancelAsync(Guid reminderId, CancellationToken cancellationToken);
}
