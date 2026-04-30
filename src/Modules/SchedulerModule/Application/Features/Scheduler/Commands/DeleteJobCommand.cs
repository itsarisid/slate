using System.Text.Json;
using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Application.Results;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Scheduler.Commands;

/// <summary>
/// Deletes a scheduler job.
/// </summary>
public sealed record DeleteJobCommand(Guid JobId, bool HardDelete = false) : IRequest<Result>;

public sealed class DeleteJobCommandHandler(
    IJobRepository jobRepository,
    IRepository<JobHistory> jobHistoryRepository,
    IUnitOfWork unitOfWork,
    ISchedulerService schedulerService,
    ICurrentUserService currentUserService)
    : IRequestHandler<DeleteJobCommand, Result>
{
    public async Task<Result> Handle(DeleteJobCommand request, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null)
        {
            return Result.Failure("Job was not found.");
        }

        var performedBy = currentUserService.Email ?? "system@alphabet.local";
        await schedulerService.DeleteJobAsync(job, cancellationToken);

        if (request.HardDelete)
        {
            jobRepository.Remove(job);
        }
        else
        {
            job.SoftDelete(performedBy);
            jobRepository.Update(job);
        }

        await jobHistoryRepository.AddAsync(
            JobHistory.Create(job.Id, JobHistoryAction.Deleted, JsonSerializer.Serialize(request), performedBy, currentUserService.IpAddress),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
