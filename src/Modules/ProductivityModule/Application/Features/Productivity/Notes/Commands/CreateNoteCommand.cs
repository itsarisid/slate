using Alphabet.Application.Features.Productivity.Common;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Notes.Commands;

/// <summary>
/// Creates a note.
/// </summary>
public sealed record CreateNoteCommand(
    string Title,
    string Content,
    Alphabet.Domain.Enums.NoteFormat Format,
    string? Category,
    IReadOnlyCollection<string>? Tags,
    string? Color,
    bool IsPinned,
    bool IsFavorite,
    Guid? NotebookId,
    IReadOnlyCollection<string>? Collaborators) : IRequest<Result<NoteDto>>;

public sealed class CreateNoteCommandHandler(
    INoteRepository noteRepository,
    IRepository<Tag> tagRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateNoteCommand, Result<NoteDto>>
{
    public async Task<Result<NoteDto>> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var note = Note.Create(
            userId,
            request.Title,
            request.Content,
            request.Format,
            request.Category,
            request.Color,
            request.IsPinned,
            request.IsFavorite,
            request.NotebookId,
            request.Collaborators);

        await noteRepository.AddAsync(note, cancellationToken);
        foreach (var tag in request.Tags?.Where(static x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase) ?? [])
        {
            await tagRepository.AddAsync(Tag.Create("Note", note.Id, tag), cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return note.ToDto();
    }
}
