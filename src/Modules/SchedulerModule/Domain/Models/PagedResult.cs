namespace Alphabet.Domain.Models;

/// <summary>
/// Represents a paged collection.
/// </summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int PageNumber, int PageSize);
