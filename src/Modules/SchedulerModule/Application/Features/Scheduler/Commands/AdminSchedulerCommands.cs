using System.Text.Json;
using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Scheduler.Commands;

public sealed record PauseAllJobsCommand : IRequest<Result>;

public sealed record ResumeAllJobsCommand : IRequest<Result>;

public sealed record ClearOldExecutionLogsCommand(int OlderThanDays) : IRequest<Result<int>>;

public sealed record ImportJobConfigurationsCommand(string JsonPayload) : IRequest<Result<int>>;

public sealed class PauseAllJobsCommandHandler(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    ISchedulerService schedulerService,
    ICurrentUserService currentUserService) : IRequestHandler<PauseAllJobsCommand, Result>
{
    public async Task<Result> Handle(PauseAllJobsCommand request, CancellationToken cancellationToken)
    {
        var jobs = await jobRepository.GetAllAsync(cancellationToken);
        foreach (var job in jobs.Where(x => !x.IsDeleted && x.IsEnabled))
        {
            job.Pause(currentUserService.Email ?? "system@alphabet.local");
            jobRepository.Update(job);
            await schedulerService.PauseJobAsync(job, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class ResumeAllJobsCommandHandler(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    ISchedulerService schedulerService,
    ICurrentUserService currentUserService) : IRequestHandler<ResumeAllJobsCommand, Result>
{
    public async Task<Result> Handle(ResumeAllJobsCommand request, CancellationToken cancellationToken)
    {
        var jobs = await jobRepository.GetAllAsync(cancellationToken);
        foreach (var job in jobs.Where(x => !x.IsDeleted))
        {
            job.Resume(currentUserService.Email ?? "system@alphabet.local");
            jobRepository.Update(job);
            var schedulerJobId = await schedulerService.UpdateScheduleAsync(job, cancellationToken);
            job.SetSchedulerJobId(schedulerJobId, currentUserService.Email ?? "system@alphabet.local");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class ClearOldExecutionLogsCommandHandler(IJobExecutionRepository executionRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<ClearOldExecutionLogsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ClearOldExecutionLogsCommand request, CancellationToken cancellationToken)
    {
        var deleted = await executionRepository.ClearOlderThanAsync(DateTimeOffset.UtcNow.AddDays(-request.OlderThanDays), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return deleted;
    }
}

public sealed class ImportJobConfigurationsCommandHandler(ISender sender)
    : IRequestHandler<ImportJobConfigurationsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ImportJobConfigurationsCommand request, CancellationToken cancellationToken)
    {
        var jobs = JsonSerializer.Deserialize<IReadOnlyList<CreateJobCommand>>(request.JsonPayload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        if (jobs is null || jobs.Count == 0)
        {
            return Result<int>.Failure("No jobs were found in the import payload.");
        }

        var created = 0;
        foreach (var job in jobs)
        {
            var result = await sender.Send(job, cancellationToken);
            if (result.IsSuccess)
            {
                created++;
            }
        }

        return created;
    }
}
