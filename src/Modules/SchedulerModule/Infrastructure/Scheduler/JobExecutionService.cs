using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;

namespace Alphabet.Infrastructure.Scheduler;

/// <summary>
/// Persists job execution lifecycle state.
/// </summary>
public sealed class JobExecutionService(IJobExecutionRepository executionRepository, IUnitOfWork unitOfWork) : IJobExecutionService
{
    /// <summary>
    /// Create pending execution async.
    /// </summary>
    public async Task<JobExecution> CreatePendingExecutionAsync(Guid jobId, Guid? triggeredBy, Guid? retryParentId, CancellationToken cancellationToken)
    {
        var execution = JobExecution.Create(jobId, triggeredBy, retryParentId);
        await executionRepository.AddAsync(execution, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return execution;
    }
    /// <summary>
    /// Create and start execution async.
    /// </summary>

    public async Task<JobExecution> CreateAndStartExecutionAsync(Guid jobId, Guid? triggeredBy, Guid? retryParentId, CancellationToken cancellationToken)
    {
        var execution = JobExecution.Create(jobId, triggeredBy, retryParentId);
        execution.Start();
        await executionRepository.AddAsync(execution, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return execution;
    }
    /// <summary>
    /// Get execution async.
    /// </summary>

    public Task<JobExecution?> GetExecutionAsync(Guid executionId, CancellationToken cancellationToken)
        => executionRepository.GetByIdAsync(executionId, cancellationToken);
    /// <summary>
    /// Mark success async.
    /// </summary>

    public async Task MarkSuccessAsync(Guid executionId, string? output, CancellationToken cancellationToken)
    {
        var execution = await executionRepository.GetByIdAsync(executionId, cancellationToken);
        if (execution is null)
        {
            return;
        }

        execution.CompleteSuccess(output);
        executionRepository.Update(execution);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
    /// <summary>
    /// Mark failure async.
    /// </summary>

    public async Task MarkFailureAsync(Guid executionId, string? output, string? errorMessage, int retryCount, CancellationToken cancellationToken)
    {
        var execution = await executionRepository.GetByIdAsync(executionId, cancellationToken);
        if (execution is null)
        {
            return;
        }

        execution.CompleteFailure(output, errorMessage, retryCount);
        executionRepository.Update(execution);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
    /// <summary>
    /// Mark timeout async.
    /// </summary>

    public async Task MarkTimeoutAsync(Guid executionId, string? output, string? errorMessage, int retryCount, CancellationToken cancellationToken)
    {
        var execution = await executionRepository.GetByIdAsync(executionId, cancellationToken);
        if (execution is null)
        {
            return;
        }

        execution.Timeout(output, errorMessage, retryCount);
        executionRepository.Update(execution);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
    /// <summary>
    /// Cancel async.
    /// </summary>

    public async Task<bool> CancelAsync(Guid executionId, CancellationToken cancellationToken)
    {
        var execution = await executionRepository.GetByIdAsync(executionId, cancellationToken);
        if (execution is null || execution.EndedAt is not null)
        {
            return false;
        }

        execution.Cancel("Cancelled by operator.");
        executionRepository.Update(execution);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
    /// <summary>
    /// Has successful execution async.
    /// </summary>

    public Task<bool> HasSuccessfulExecutionAsync(Guid jobId, CancellationToken cancellationToken)
        => executionRepository.HasSuccessfulExecutionAsync(jobId, cancellationToken);
}
