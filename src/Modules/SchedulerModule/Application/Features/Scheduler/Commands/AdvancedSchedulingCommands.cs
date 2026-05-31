using System.Text.Json;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Scheduler.Commands;

public sealed record AddJobExclusionCommand(Guid JobId, IReadOnlyList<DateOnly>? ExcludedDates, IReadOnlyList<DayOfWeek>? ExcludedDaysOfWeek, string? Start, string? End)
    : IRequest<Result>;

public sealed record AddJobDependencyCommand(Guid JobId, IReadOnlyList<Guid> DependsOnJobIds, string Condition)
    : IRequest<Result>;

public sealed record CreateWorkflowJobItem(Guid JobId, IReadOnlyList<Guid> DependsOn, string OnFailure);

public sealed record CreateWorkflowCommand(string Name, IReadOnlyList<CreateWorkflowJobItem> Jobs) : IRequest<Result<string>>;
/// <summary>
/// Add job exclusion command handler.
/// </summary>

public sealed class AddJobExclusionCommandHandler(
    IJobRepository jobRepository,
    IRepository<JobHistory> jobHistoryRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<AddJobExclusionCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result> Handle(AddJobExclusionCommand request, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            return Result.Failure("Job was not found.");
        }

        var payload = JsonSerializer.Serialize(new
        {
            request.ExcludedDates,
            request.ExcludedDaysOfWeek,
            TimeRange = new { request.Start, request.End }
        });

        job.SetExclusions(payload, currentUserService.Email ?? "system@alphabet.local");
        jobRepository.Update(job);
        await jobHistoryRepository.AddAsync(JobHistory.Create(job.Id, JobHistoryAction.Updated, payload, currentUserService.Email ?? "system@alphabet.local", currentUserService.IpAddress), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
/// <summary>
/// Add job dependency command handler.
/// </summary>

public sealed class AddJobDependencyCommandHandler(
    IJobRepository jobRepository,
    IRepository<JobHistory> jobHistoryRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<AddJobDependencyCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result> Handle(AddJobDependencyCommand request, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            return Result.Failure("Job was not found.");
        }

        var payload = JsonSerializer.Serialize(new { request.DependsOnJobIds, request.Condition });
        job.SetDependencies(payload, currentUserService.Email ?? "system@alphabet.local");
        jobRepository.Update(job);
        await jobHistoryRepository.AddAsync(JobHistory.Create(job.Id, JobHistoryAction.Updated, payload, currentUserService.Email ?? "system@alphabet.local", currentUserService.IpAddress), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
/// <summary>
/// Create workflow command handler.
/// </summary>

public sealed class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, Result<string>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public Task<Result<string>> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    => Task.FromResult(Result<string>.Success(JsonSerializer.Serialize(request)));
}
