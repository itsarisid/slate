using Microsoft.Extensions.Logging;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Synchronizes external calendar providers.
/// </summary>
public sealed class CalendarSyncJob(ILogger<CalendarSyncJob> logger)
{
    /// <summary>
    /// Execute async.
    /// </summary>
    public Task ExecuteAsync()
    {
        logger.LogInformation("Calendar sync job executed.");
        return Task.CompletedTask;
    }
}
