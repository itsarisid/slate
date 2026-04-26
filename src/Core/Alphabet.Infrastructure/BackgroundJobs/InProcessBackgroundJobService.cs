using Alphabet.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alphabet.Infrastructure.BackgroundJobs;

/// <summary>
/// Runs background work items in-process.
/// </summary>
public sealed class InProcessBackgroundJobService(ILogger<InProcessBackgroundJobService> logger) : IBackgroundJobService
{
    public async Task EnqueueAsync(string jobName, Func<CancellationToken, Task> workItem, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing background job {JobName}", jobName);
        await workItem(cancellationToken);
    }
}
