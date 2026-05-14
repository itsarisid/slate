namespace Alphabet.Application.Features.Productivity.Dtos;

/// <summary>
/// Represents a paginated API response.
/// </summary>
public sealed record PagedResponseDto<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
