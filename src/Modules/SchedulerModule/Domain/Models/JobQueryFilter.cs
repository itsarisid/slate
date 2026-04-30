using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Models;

/// <summary>
/// Query parameters for filtering jobs.
/// </summary>
public sealed record JobQueryFilter(
    int PageNumber = 1,
    int PageSize = 20,
    JobType? JobType = null,
    string? Status = null,
    string? Tag = null,
    string? Search = null,
    string? SortBy = null,
    string? SortDirection = null,
    string? CreatedBy = null);
