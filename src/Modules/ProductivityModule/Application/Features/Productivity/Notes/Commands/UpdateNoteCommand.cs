using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Notes.Commands;

/// <summary>
/// Updates an existing note.
/// </summary>
public sealed record UpdateNoteCommand(
    Guid NoteId,
    string Title,
    string Content,
    Alphabet.Domain.Enums.NoteFormat Format,
    string? Category,
    string? Color,
    bool IsPinned,
    bool IsFavorite,
    Guid? NotebookId) : IRequest<Result<NoteDto>>;
/// <summary>
/// Update note command handler.
/// </summary>

public sealed class UpdateNoteCommandHandler(INoteRepository noteRepository)
    : IRequestHandler<UpdateNoteCommand, Result<NoteDto>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<NoteDto>> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await noteRepository.GetByIdAsync(request.NoteId, cancellationToken);
        if (note is null)
        {
            return Result<NoteDto>.Failure("Note was not found.");
        }

        note.Update(request.Title, request.Content, request.Format, request.Category, request.Color, request.IsPinned, request.IsFavorite, request.NotebookId);
        noteRepository.Update(note);
        return note.ToDto();
    }
}
