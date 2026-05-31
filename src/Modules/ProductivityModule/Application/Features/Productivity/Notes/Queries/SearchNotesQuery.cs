using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.Productivity;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Notes.Queries;

/// <summary>
/// Gets or searches notes.
/// </summary>
public sealed record SearchNotesQuery(
    string? Category,
    string? Tag,
    Guid? NotebookId,
    bool? IsPinned,
    bool? IsFavorite,
    string? Search,
    int Page,
    int PageSize) : IRequest<Result<PagedResponseDto<NoteDto>>>;
/// <summary>
/// Search notes query handler.
/// </summary>

public sealed class SearchNotesQueryHandler(
    INoteRepository noteRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<SearchNotesQuery, Result<PagedResponseDto<NoteDto>>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<PagedResponseDto<NoteDto>>> Handle(SearchNotesQuery request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var result = await noteRepository.SearchAsync(
            new NoteQueryFilter(
                request.Category,
                request.Tag,
                request.NotebookId,
                request.IsPinned,
                request.IsFavorite,
                request.Search,
                userId,
                request.Page <= 0 ? 1 : request.Page,
                request.PageSize <= 0 ? 20 : request.PageSize),
            cancellationToken);

        return new PagedResponseDto<NoteDto>(result.Items.Select(x => x.ToDto()).ToArray(), result.Page, result.PageSize, result.TotalCount);
    }
}
