using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.Scheduler.Queries;

/// <summary>
/// Returns paged executions for a job.
/// </summary>
public sealed record GetJobExecutionsQuery(
    Guid JobId,
    int PageNumber = 1,
    int PageSize = 20,
    ExecutionStatus? Status = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null) : IRequest<PagedResponseDto<JobExecutionDto>>;

public sealed class GetJobExecutionsQueryHandler(IJobExecutionRepository executionRepository)
    : IRequestHandler<GetJobExecutionsQuery, PagedResponseDto<JobExecutionDto>>
{
    public async Task<PagedResponseDto<JobExecutionDto>> Handle(GetJobExecutionsQuery request, CancellationToken cancellationToken)
    {
        var page = await executionRepository.GetPagedByJobIdAsync(
            new JobExecutionQueryFilter(request.JobId, request.PageNumber, request.PageSize, request.Status, request.FromDate, request.ToDate),
            cancellationToken);

        return page.ToDto();
    }
}
