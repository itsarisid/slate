using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Tasks.Queries;

/// <summary>
/// Gets task dependencies.
/// </summary>
public sealed record GetTaskDependencyGraphQuery(Guid TaskId) : IRequest<Result<IReadOnlyList<Guid>>>;

public sealed class GetTaskDependencyGraphQueryHandler(ITaskRepository taskRepository)
    : IRequestHandler<GetTaskDependencyGraphQuery, Result<IReadOnlyList<Guid>>>
{
    public async Task<Result<IReadOnlyList<Guid>>> Handle(GetTaskDependencyGraphQuery request, CancellationToken cancellationToken)
    {
        var items = await taskRepository.GetDependenciesAsync(request.TaskId, cancellationToken);
        return items.Select(x => x.DependsOnTaskId).ToArray();
    }
}
