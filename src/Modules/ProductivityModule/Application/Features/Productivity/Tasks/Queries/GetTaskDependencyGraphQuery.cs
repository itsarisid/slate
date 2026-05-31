using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Tasks.Queries;

/// <summary>
/// Gets task dependencies.
/// </summary>
public sealed record GetTaskDependencyGraphQuery(Guid TaskId) : IRequest<Result<IReadOnlyList<Guid>>>;
/// <summary>
/// Get task dependency graph query handler.
/// </summary>

public sealed class GetTaskDependencyGraphQueryHandler(ITaskRepository taskRepository)
    : IRequestHandler<GetTaskDependencyGraphQuery, Result<IReadOnlyList<Guid>>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<IReadOnlyList<Guid>>> Handle(GetTaskDependencyGraphQuery request, CancellationToken cancellationToken)
    {
        var items = await taskRepository.GetDependenciesAsync(request.TaskId, cancellationToken);
        return items.Select(x => x.DependsOnTaskId).ToArray();
    }
}
