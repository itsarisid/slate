namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Enqueues or schedules background jobs.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Enqueue async.
    /// </summary>
    Task EnqueueAsync(string jobName, Func<CancellationToken, Task> workItem, CancellationToken cancellationToken);
}
