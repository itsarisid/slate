using Microsoft.Extensions.Logging;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Placeholder recurring task generation job.
/// </summary>
public sealed class RecurringTaskGeneratorJob(ILogger<RecurringTaskGeneratorJob> logger)
{
    /// <summary>
    /// Execute async.
    /// </summary>
    public Task ExecuteAsync()
    {
        logger.LogInformation("Recurring task generation job executed.");
        return Task.CompletedTask;
    }
}
