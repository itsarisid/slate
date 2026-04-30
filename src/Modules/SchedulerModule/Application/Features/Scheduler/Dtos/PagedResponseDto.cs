namespace Alphabet.Application.Features.Scheduler.Dtos;

/// <summary>
/// Generic paged API response.
/// </summary>
public sealed record PagedResponseDto<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
