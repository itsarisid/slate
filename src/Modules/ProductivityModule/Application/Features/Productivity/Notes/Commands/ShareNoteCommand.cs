using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Notes.Commands;

/// <summary>
/// Shares a note with a collaborator.
/// </summary>
public sealed record ShareNoteCommand(Guid NoteId, string Email, string Permission) : IRequest<Result>;
/// <summary>
/// Share note command handler.
/// </summary>

public sealed class ShareNoteCommandHandler(INoteRepository noteRepository)
    : IRequestHandler<ShareNoteCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result> Handle(ShareNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await noteRepository.GetByIdAsync(request.NoteId, cancellationToken);
        if (note is null)
        {
            return Result.Failure("Note was not found.");
        }

        note.ShareWith(request.Email);
        noteRepository.Update(note);
        return Result.Success();
    }
}
