using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Scheduler.Commands;

public sealed record CancelExecutionCommand(Guid ExecutionId) : IRequest<Result>;

public sealed record RetryExecutionCommand(Guid ExecutionId) : IRequest<Result<Guid>>;
/// <summary>
/// Cancel execution command handler.
/// </summary>

public sealed class CancelExecutionCommandHandler(IJobExecutionService jobExecutionService)
    : IRequestHandler<CancelExecutionCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result> Handle(CancelExecutionCommand request, CancellationToken cancellationToken)
    {
        var cancelled = await jobExecutionService.CancelAsync(request.ExecutionId, cancellationToken);
        return cancelled ? Result.Success() : Result.Failure("Execution could not be cancelled.");
    }
}
/// <summary>
/// Retry execution command handler.
/// </summary>

public sealed class RetryExecutionCommandHandler(
    IJobExecutionRepository jobExecutionRepository,
    IJobRepository jobRepository,
    ISchedulerService schedulerService)
    : IRequestHandler<RetryExecutionCommand, Result<Guid>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<Guid>> Handle(RetryExecutionCommand request, CancellationToken cancellationToken)
    {
        var execution = await jobExecutionRepository.GetByIdAsync(request.ExecutionId, cancellationToken);
        if (execution is null)
        {
            return Result<Guid>.Failure("Execution was not found.");
        }

        var job = await jobRepository.GetByIdAsync(execution.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            return Result<Guid>.Failure("Job was not found.");
        }

        var executionId = await schedulerService.TriggerJobAsync(job, execution.TriggeredBy, execution.Id, cancellationToken);
        return executionId;
    }
}
