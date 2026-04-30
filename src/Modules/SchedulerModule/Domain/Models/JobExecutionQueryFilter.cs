using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Models;

/// <summary>
/// Query parameters for filtering job executions.
/// </summary>
public sealed record JobExecutionQueryFilter(
    Guid JobId,
    int PageNumber = 1,
    int PageSize = 20,
    ExecutionStatus? Status = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null);
