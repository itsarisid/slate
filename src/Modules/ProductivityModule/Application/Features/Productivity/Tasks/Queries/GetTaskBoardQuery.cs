using Alphabet.Application.Features.Productivity.Common;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces.Productivity;
using Alphabet.Domain.Models;
using MediatR;
using ProductivityTaskStatus = Alphabet.Domain.Enums.TaskStatus;

namespace Alphabet.Application.Features.Productivity.Tasks.Queries;

/// <summary>
/// Gets a Kanban board view of tasks.
/// </summary>
public sealed record GetTaskBoardQuery(Guid? ProjectId) : IRequest<Result<IReadOnlyList<TaskBoardColumnDto>>>;

public sealed class GetTaskBoardQueryHandler(
    ITaskRepository taskRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetTaskBoardQuery, Result<IReadOnlyList<TaskBoardColumnDto>>>
{
    public async Task<Result<IReadOnlyList<TaskBoardColumnDto>>> Handle(GetTaskBoardQuery request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var tasks = await taskRepository.GetBoardAsync(new TaskBoardFilter(request.ProjectId, null, userId), cancellationToken);
        var board = Enum.GetValues<ProductivityTaskStatus>()
            .Select(status => new TaskBoardColumnDto(status, tasks.Where(task => task.Status == status).Select(x => x.ToDto()).ToArray()))
            .ToArray();

        return board;
    }
}
