using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Models;

/// <summary>
/// Represents advanced asset filtering.
/// </summary>
public sealed record AssetFilter(
    AssetStatus? Status,
    Guid? CategoryId,
    Guid? LocationId,
    Guid? AssignedToUserId,
    string? Search,
    DateOnly? PurchaseDateFrom,
    DateOnly? PurchaseDateTo,
    decimal? CostMin,
    decimal? CostMax,
    AssetCondition? Condition,
    int? WarrantyExpiringInDays,
    bool IncludeRetired,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize);
