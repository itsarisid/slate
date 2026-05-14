namespace Alphabet.Domain.Models;

/// <summary>
/// Represents a paged query result.
/// </summary>
public sealed record ProductivityPagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
