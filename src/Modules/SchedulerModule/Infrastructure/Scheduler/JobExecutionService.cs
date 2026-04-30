using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;

namespace Alphabet.Infrastructure.Scheduler;

/// <summary>
/// Persists job execution lifecycle state.
/// </summary>
public sealed class JobExecutionService(IJobExecutionRepository executionRepository, IUnitOfWork unitOfWork) : IJobExecutionService
{
    public async Task<JobExecution> CreatePendingExecutionAsync(Guid jobId, Guid? triggeredBy, Guid? retryParentId, CancellationToken cancellationToken)
    {
        var execution = JobExecution.Create(jobId, triggeredBy, retryParentId);
        await executionRepository.AddAsync(execution, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return execution;
    }

    public async Task<JobExecution> CreateAndStartExecutionAsync(Guid jobId, Guid? triggeredBy, Guid? retryParentId, CancellationToken cancellationToken)
    {
        var execution = JobExecution.Create(jobId, triggeredBy, retryParentId);
        execution.Start();
        await executionRepository.AddAsync(execution, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return execution;
    }

    public Task<JobExecution?> GetExecutionAsync(Guid executionId, CancellationToken cancellationToken)
        => executionRepository.GetByIdAsync(executionId, cancellationToken);

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

    public Task<bool> HasSuccessfulExecutionAsync(Guid jobId, CancellationToken cancellationToken)
        => executionRepository.HasSuccessfulExecutionAsync(jobId, cancellationToken);
}
