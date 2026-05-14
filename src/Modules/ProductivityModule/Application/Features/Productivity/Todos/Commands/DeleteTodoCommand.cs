using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Todos.Commands;

/// <summary>
/// Soft deletes or restores a todo item.
/// </summary>
public sealed record DeleteTodoCommand(Guid TodoId, bool Restore) : IRequest<Result>;

public sealed class DeleteTodoCommandHandler(ITodoRepository todoRepository)
    : IRequestHandler<DeleteTodoCommand, Result>
{
    public async Task<Result> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await todoRepository.GetByIdAsync(request.TodoId, cancellationToken);
        if (todo is null)
        {
            return Result.Failure("Todo was not found.");
        }

        if (request.Restore)
        {
            todo.Restore();
        }
        else
        {
            todo.MoveToTrash();
        }

        todoRepository.Update(todo);
        return Result.Success();
    }
}
