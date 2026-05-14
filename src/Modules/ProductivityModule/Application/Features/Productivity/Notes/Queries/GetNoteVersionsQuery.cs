using Alphabet.Application.Features.Productivity.Common;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Notes.Queries;

/// <summary>
/// Gets note version history.
/// </summary>
public sealed record GetNoteVersionsQuery(Guid NoteId) : IRequest<Result<IReadOnlyList<NoteVersionDto>>>;

public sealed class GetNoteVersionsQueryHandler(INoteRepository noteRepository)
    : IRequestHandler<GetNoteVersionsQuery, Result<IReadOnlyList<NoteVersionDto>>>
{
    public async Task<Result<IReadOnlyList<NoteVersionDto>>> Handle(GetNoteVersionsQuery request, CancellationToken cancellationToken)
    {
        var note = await noteRepository.GetByIdAsync(request.NoteId, cancellationToken);
        return note is null
            ? Result<IReadOnlyList<NoteVersionDto>>.Failure("Note was not found.")
            : Result<IReadOnlyList<NoteVersionDto>>.Success(note.Versions.Select(x => x.ToDto()).ToArray());
    }
}
