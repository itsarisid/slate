using Alphabet.Application.Features.Productivity.Common;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.Productivity;
using Alphabet.Domain.ValueObjects;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Todos.Commands;

/// <summary>
/// Updates an existing todo item.
/// </summary>
public sealed record UpdateTodoCommand(
    Guid TodoId,
    string Title,
    string Description,
    Alphabet.Domain.Enums.Priority Priority,
    DateTimeOffset? DueDate,
    string? Category,
    Guid? AssignedTo,
    int? ReminderMinutesBefore,
    bool IsRecurring,
    RecurrencePattern? RecurrencePattern) : IRequest<Result<TodoDto>>;

public sealed class UpdateTodoCommandHandler(ITodoRepository todoRepository)
    : IRequestHandler<UpdateTodoCommand, Result<TodoDto>>
{
    public async Task<Result<TodoDto>> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await todoRepository.GetByIdAsync(request.TodoId, cancellationToken);
        if (todo is null)
        {
            return Result<TodoDto>.Failure("Todo was not found.");
        }

        todo.UpdateDetails(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            request.Category,
            request.AssignedTo,
            request.ReminderMinutesBefore,
            request.IsRecurring,
            request.RecurrencePattern);

        todoRepository.Update(todo);
        return todo.ToDto();
    }
}
