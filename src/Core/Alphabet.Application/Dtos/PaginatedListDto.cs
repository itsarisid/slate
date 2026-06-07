namespace Alphabet.Application.Dtos;

/// <summary>
/// Transport DTO for paged data.
/// </summary>
public sealed record PaginatedListDto<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
