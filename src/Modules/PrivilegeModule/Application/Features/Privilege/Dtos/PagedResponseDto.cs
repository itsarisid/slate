namespace Alphabet.Application.Features.Privilege.Dtos;

/// <summary>
/// Represents a paged response.
/// </summary>
public sealed record PagedResponseDto<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
