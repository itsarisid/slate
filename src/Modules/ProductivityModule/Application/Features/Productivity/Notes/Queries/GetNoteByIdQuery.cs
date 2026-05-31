using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Notes.Queries;

/// <summary>
/// Gets a note by identifier.
/// </summary>
public sealed record GetNoteByIdQuery(Guid NoteId) : IRequest<Result<NoteDto>>;
/// <summary>
/// Get note by id query handler.
/// </summary>

public sealed class GetNoteByIdQueryHandler(INoteRepository noteRepository)
    : IRequestHandler<GetNoteByIdQuery, Result<NoteDto>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<NoteDto>> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
    {
        var note = await noteRepository.GetByIdAsync(request.NoteId, cancellationToken);
        return note is null ? Result<NoteDto>.Failure("Note was not found.") : note.ToDto();
    }
}
