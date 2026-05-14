using Alphabet.Application.Features.Productivity.Common;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Todos.Queries;

/// <summary>
/// Gets a todo by identifier.
/// </summary>
public sealed record GetTodoByIdQuery(Guid TodoId) : IRequest<Result<TodoDto>>;

public sealed class GetTodoByIdQueryHandler(ITodoRepository todoRepository)
    : IRequestHandler<GetTodoByIdQuery, Result<TodoDto>>
{
    public async Task<Result<TodoDto>> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
    {
        var todo = await todoRepository.GetByIdAsync(request.TodoId, cancellationToken);
        return todo is null ? Result<TodoDto>.Failure("Todo was not found.") : todo.ToDto();
    }
}
