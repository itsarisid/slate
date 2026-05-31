using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.CrossEntity.Commands;

/// <summary>
/// Converts a note to a todo.
/// </summary>
public sealed record ConvertNoteToTodoCommand(Guid NoteId) : IRequest<Result<TodoDto>>;
/// <summary>
/// Convert note to todo command handler.
/// </summary>

public sealed class ConvertNoteToTodoCommandHandler(
    INoteRepository noteRepository,
    ITodoRepository todoRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<ConvertNoteToTodoCommand, Result<TodoDto>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<TodoDto>> Handle(ConvertNoteToTodoCommand request, CancellationToken cancellationToken)
    {
        var note = await noteRepository.GetByIdAsync(request.NoteId, cancellationToken);
        if (note is null)
        {
            return Result<TodoDto>.Failure("Note was not found.");
        }

        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var todo = Todo.Create(userId, userId, note.Title, note.Content, Alphabet.Domain.Enums.Priority.Medium, null, note.Category, null, false, null);
        await todoRepository.AddAsync(todo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return todo.ToDto();
    }
}
