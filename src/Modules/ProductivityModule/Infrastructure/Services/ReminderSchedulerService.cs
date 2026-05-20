using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces.Productivity;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Schedules reminder triggers through Hangfire.
/// </summary>
public sealed class ReminderSchedulerService(
    IBackgroundJobClient backgroundJobClient,
    ILogger<ReminderSchedulerService> logger) : IReminderService
{
    /// <summary>
    /// Schedule async.
    /// </summary>
    public Task ScheduleAsync(Reminder reminder, CancellationToken cancellationToken)
    {
        var delay = reminder.ReminderTime.UtcDateTime - DateTime.UtcNow;
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        backgroundJobClient.Schedule<ReminderTriggerJob>(job => job.ExecuteAsync(reminder.Id), delay);
        logger.LogInformation("Scheduled reminder {ReminderId} to run in {Delay}.", reminder.Id, delay);
        return Task.CompletedTask;
    }
    /// <summary>
    /// Reschedule async.
    /// </summary>

    public Task RescheduleAsync(Reminder reminder, CancellationToken cancellationToken) => ScheduleAsync(reminder, cancellationToken);
    /// <summary>
    /// Cancel async.
    /// </summary>

    public Task CancelAsync(Guid reminderId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Reminder cancel requested for {ReminderId}; queued Hangfire jobs are allowed to no-op if already persisted.", reminderId);
        return Task.CompletedTask;
    }
}
