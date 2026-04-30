using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.Scheduler.Queries;

/// <summary>
/// Returns paged scheduler jobs.
/// </summary>
public sealed record GetJobsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    JobType? JobType = null,
    string? Status = null,
    string? Tag = null,
    string? Search = null,
    string? SortBy = null,
    string? SortDirection = null,
    string? CreatedBy = null) : IRequest<PagedResponseDto<JobDto>>;

public sealed class GetJobsQueryHandler(IJobRepository jobRepository, ISchedulerService schedulerService)
    : IRequestHandler<GetJobsQuery, PagedResponseDto<JobDto>>
{
    public async Task<PagedResponseDto<JobDto>> Handle(GetJobsQuery request, CancellationToken cancellationToken)
    {
        var page = await jobRepository.GetPagedAsync(
            new JobQueryFilter(
                request.PageNumber,
                request.PageSize,
                request.JobType,
                request.Status,
                request.Tag,
                request.Search,
                request.SortBy,
                request.SortDirection,
                request.CreatedBy),
            cancellationToken);

        var statuses = new Dictionary<Guid, string>();
        foreach (var job in page.Items)
        {
            statuses[job.Id] = await schedulerService.GetJobStatusAsync(job, cancellationToken);
        }

        return page.ToDto(statuses);
    }
}
