namespace Alphabet.Domain.Models;

/// <summary>
/// Represents filtering options for note queries.
/// </summary>
public sealed record NoteQueryFilter(
    string? Category,
    string? Tag,
    Guid? NotebookId,
    bool? IsPinned,
    bool? IsFavorite,
    string? Search,
    Guid? OwnerUserId,
    int Page,
    int PageSize);
