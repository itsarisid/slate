using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Tasks.Commands;

/// <summary>
/// Updates a task status.
/// </summary>
public sealed record UpdateTaskStatusCommand(Guid TaskId, Alphabet.Domain.Enums.TaskStatus Status, string? Comment) : IRequest<Result>;

public sealed class UpdateTaskStatusCommandHandler(ITaskRepository taskRepository)
    : IRequestHandler<UpdateTaskStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
        {
            return Result.Failure("Task was not found.");
        }

        task.UpdateStatus(request.Status, request.Comment);
        taskRepository.Update(task);
        return Result.Success();
    }
}
