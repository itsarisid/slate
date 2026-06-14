namespace Alphabet.Application.Results;

/// <summary>
/// Represents a paged operation result.
/// </summary>
public sealed class PagedResult<T> : Result<IReadOnlyList<T>>
{
    private PagedResult(
        bool isSuccess,
        IReadOnlyList<T>? value,
        string? error,
        int page,
        int pageSize,
        int totalCount)
        : base(isSuccess, value, error)
    {
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public int Page { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public static PagedResult<T> Success(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        return new(true, items, null, page, pageSize, totalCount);
    }

    public new static PagedResult<T> Failure(string error)
    {
        return new(false, null, error, 0, 0, 0);
    }
}
