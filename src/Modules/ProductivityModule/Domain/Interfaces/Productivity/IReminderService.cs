using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces.Productivity;

/// <summary>
/// Schedules and maintains reminder execution.
/// </summary>
public interface IReminderService
{
    /// <summary>
    /// Schedule async.
    /// </summary>
    Task ScheduleAsync(Reminder reminder, CancellationToken cancellationToken);
    /// <summary>
    /// Reschedule async.
    /// </summary>

    Task RescheduleAsync(Reminder reminder, CancellationToken cancellationToken);
    /// <summary>
    /// Cancel async.
    /// </summary>

    Task CancelAsync(Guid reminderId, CancellationToken cancellationToken);
}
