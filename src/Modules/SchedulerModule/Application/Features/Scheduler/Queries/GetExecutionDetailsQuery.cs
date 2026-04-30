using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Scheduler.Queries;

/// <summary>
/// Returns a single execution by identifier.
/// </summary>
public sealed record GetExecutionDetailsQuery(Guid ExecutionId) : IRequest<Result<JobExecutionDto>>;

public sealed class GetExecutionDetailsQueryHandler(IJobExecutionRepository executionRepository)
    : IRequestHandler<GetExecutionDetailsQuery, Result<JobExecutionDto>>
{
    public async Task<Result<JobExecutionDto>> Handle(GetExecutionDetailsQuery request, CancellationToken cancellationToken)
    {
        var execution = await executionRepository.GetByIdAsync(request.ExecutionId, cancellationToken);
        return execution is null
            ? Result<JobExecutionDto>.Failure("Execution was not found.")
            : execution.ToDto();
    }
}
