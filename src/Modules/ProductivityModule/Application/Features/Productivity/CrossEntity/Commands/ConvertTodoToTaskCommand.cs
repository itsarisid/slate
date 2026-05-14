using Alphabet.Application.Features.Productivity.Common;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.CrossEntity.Commands;

/// <summary>
/// Converts a todo into a task.
/// </summary>
public sealed record ConvertTodoToTaskCommand(Guid TodoId) : IRequest<Result<TaskDto>>;

public sealed class ConvertTodoToTaskCommandHandler(
    ITodoRepository todoRepository,
    ITaskRepository taskRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<ConvertTodoToTaskCommand, Result<TaskDto>>
{
    public async Task<Result<TaskDto>> Handle(ConvertTodoToTaskCommand request, CancellationToken cancellationToken)
    {
        var todo = await todoRepository.GetByIdAsync(request.TodoId, cancellationToken);
        if (todo is null)
        {
            return Result<TaskDto>.Failure("Todo was not found.");
        }

        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var task = ProductivityTask.Create(
            userId,
            todo.Title,
            todo.Description,
            todo.Priority,
            Alphabet.Domain.Enums.TaskStatus.NotStarted,
            todo.DueDate,
            null,
            todo.AssignedToUserId,
            null,
            null,
            null,
            null);

        await taskRepository.AddAsync(task, cancellationToken);
        todo.MarkConverted(task.Id);
        todoRepository.Update(todo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return task.ToDto();
    }
}
