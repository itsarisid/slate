using System.Text.Json;
using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Application.Results;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Scheduler.Commands;

public sealed record PauseJobCommand(Guid JobId) : IRequest<Result>;

public sealed record ResumeJobCommand(Guid JobId) : IRequest<Result>;

public sealed record TriggerJobCommand(Guid JobId) : IRequest<Result<Guid>>;

public sealed class PauseJobCommandHandler(
    IJobRepository jobRepository,
    IRepository<JobHistory> jobHistoryRepository,
    IUnitOfWork unitOfWork,
    ISchedulerService schedulerService,
    ICurrentUserService currentUserService) : IRequestHandler<PauseJobCommand, Result>
{
    public async Task<Result> Handle(PauseJobCommand request, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            return Result.Failure("Job was not found.");
        }

        var performedBy = currentUserService.Email ?? "system@alphabet.local";
        job.Pause(performedBy);
        jobRepository.Update(job);
        await schedulerService.PauseJobAsync(job, cancellationToken);
        await jobHistoryRepository.AddAsync(JobHistory.Create(job.Id, JobHistoryAction.Paused, JsonSerializer.Serialize(request), performedBy, currentUserService.IpAddress), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class ResumeJobCommandHandler(
    IJobRepository jobRepository,
    IRepository<JobHistory> jobHistoryRepository,
    IUnitOfWork unitOfWork,
    ISchedulerService schedulerService,
    ICurrentUserService currentUserService) : IRequestHandler<ResumeJobCommand, Result>
{
    public async Task<Result> Handle(ResumeJobCommand request, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            return Result.Failure("Job was not found.");
        }

        var performedBy = currentUserService.Email ?? "system@alphabet.local";
        job.Resume(performedBy);
        var schedulerJobId = await schedulerService.UpdateScheduleAsync(job, cancellationToken);
        job.SetSchedulerJobId(schedulerJobId, performedBy);
        jobRepository.Update(job);
        await jobHistoryRepository.AddAsync(JobHistory.Create(job.Id, JobHistoryAction.Resumed, JsonSerializer.Serialize(request), performedBy, currentUserService.IpAddress), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class TriggerJobCommandHandler(
    IJobRepository jobRepository,
    IRepository<JobHistory> jobHistoryRepository,
    IUnitOfWork unitOfWork,
    ISchedulerService schedulerService,
    ICurrentUserService currentUserService) : IRequestHandler<TriggerJobCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(TriggerJobCommand request, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            return Result<Guid>.Failure("Job was not found.");
        }

        var executionId = await schedulerService.TriggerJobAsync(job, currentUserService.UserId, null, cancellationToken);
        await jobHistoryRepository.AddAsync(JobHistory.Create(job.Id, JobHistoryAction.Triggered, JsonSerializer.Serialize(request), currentUserService.Email ?? "system@alphabet.local", currentUserService.IpAddress), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return executionId;
    }
}
