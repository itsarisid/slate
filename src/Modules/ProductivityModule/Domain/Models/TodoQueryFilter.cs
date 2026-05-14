using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Models;

/// <summary>
/// Represents filtering options for todo queries.
/// </summary>
public sealed record TodoQueryFilter(
    TodoStatus? Status,
    Priority? Priority,
    string? Category,
    string? Tag,
    DateTimeOffset? DueDateFrom,
    DateTimeOffset? DueDateTo,
    Guid? AssignedTo,
    string? Search,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize,
    Guid? OwnerUserId);
