using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;

namespace Alphabet.Application.Common.Interfaces.Scheduler;

/// <summary>
/// Manages persisted job execution lifecycle records.
/// </summary>
public interface IJobExecutionService
{
    Task<JobExecution> CreatePendingExecutionAsync(Guid jobId, Guid? triggeredBy, Guid? retryParentId, CancellationToken cancellationToken);

    Task<JobExecution> CreateAndStartExecutionAsync(Guid jobId, Guid? triggeredBy, Guid? retryParentId, CancellationToken cancellationToken);

    Task<JobExecution?> GetExecutionAsync(Guid executionId, CancellationToken cancellationToken);

    Task MarkSuccessAsync(Guid executionId, string? output, CancellationToken cancellationToken);

    Task MarkFailureAsync(Guid executionId, string? output, string? errorMessage, int retryCount, CancellationToken cancellationToken);

    Task MarkTimeoutAsync(Guid executionId, string? output, string? errorMessage, int retryCount, CancellationToken cancellationToken);

    Task<bool> CancelAsync(Guid executionId, CancellationToken cancellationToken);

    Task<bool> HasSuccessfulExecutionAsync(Guid jobId, CancellationToken cancellationToken);
}
