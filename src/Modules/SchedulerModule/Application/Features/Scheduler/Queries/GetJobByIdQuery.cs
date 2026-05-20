using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Scheduler.Queries;

/// <summary>
/// Returns a single job by identifier.
/// </summary>
public sealed record GetJobByIdQuery(Guid JobId) : IRequest<Result<JobDto>>;
/// <summary>
/// Get job by id query handler.
/// </summary>

public sealed class GetJobByIdQueryHandler(IJobRepository jobRepository, ISchedulerService schedulerService)
    : IRequestHandler<GetJobByIdQuery, Result<JobDto>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<JobDto>> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            return Result<JobDto>.Failure("Job was not found.");
        }

        var status = await schedulerService.GetJobStatusAsync(job, cancellationToken);
        return job.ToDto(status);
    }
}
