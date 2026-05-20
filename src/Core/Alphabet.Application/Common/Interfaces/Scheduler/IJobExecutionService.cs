namespace Alphabet.Application.Common.Interfaces.Scheduler;

/// <summary>
/// Manages persisted job execution lifecycle records.
/// </summary>
public interface IJobExecutionService
{
    /// <summary>
    /// Create pending execution async.
    /// </summary>
    Task<JobExecution> CreatePendingExecutionAsync(Guid jobId, Guid? triggeredBy, Guid? retryParentId, CancellationToken cancellationToken);
    /// <summary>
    /// Create and start execution async.
    /// </summary>

    Task<JobExecution> CreateAndStartExecutionAsync(Guid jobId, Guid? triggeredBy, Guid? retryParentId, CancellationToken cancellationToken);
    /// <summary>
    /// Get execution async.
    /// </summary>

    Task<JobExecution?> GetExecutionAsync(Guid executionId, CancellationToken cancellationToken);
    /// <summary>
    /// Mark success async.
    /// </summary>

    Task MarkSuccessAsync(Guid executionId, string? output, CancellationToken cancellationToken);
    /// <summary>
    /// Mark failure async.
    /// </summary>

    Task MarkFailureAsync(Guid executionId, string? output, string? errorMessage, int retryCount, CancellationToken cancellationToken);
    /// <summary>
    /// Mark timeout async.
    /// </summary>

    Task MarkTimeoutAsync(Guid executionId, string? output, string? errorMessage, int retryCount, CancellationToken cancellationToken);
    /// <summary>
    /// Cancel async.
    /// </summary>

    Task<bool> CancelAsync(Guid executionId, CancellationToken cancellationToken);
    /// <summary>
    /// Has successful execution async.
    /// </summary>

    Task<bool> HasSuccessfulExecutionAsync(Guid jobId, CancellationToken cancellationToken);
}
