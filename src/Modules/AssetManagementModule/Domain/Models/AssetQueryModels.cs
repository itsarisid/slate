using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Models;

/// <summary>
/// Represents an asset query filter.
/// </summary>
public sealed record AssetQueryFilter(
    string? Status,
    Guid? CategoryId,
    Guid? LocationId,
    Guid? AssignedToUserId,
    string? Search,
    DateOnly? PurchaseDateFrom,
    DateOnly? PurchaseDateTo,
    decimal? CostMin,
    decimal? CostMax,
    string? Condition,
    int? WarrantyExpiringInDays,
    bool IncludeRetired,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize);

/// <summary>
/// Represents an activity log filter.
/// </summary>
public sealed record AssetActivityFilter(
    Guid? AssetId,
    string? Action,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    Guid? UserId,
    int Take,
    int Skip);

/// <summary>
/// Represents an asset stock take line item.
/// </summary>
public sealed record StockTakeCountedItem(
    Guid AssetId,
    int CountedQuantity,
    int ExpectedQuantity,
    int Discrepancy);

/// <summary>
/// Represents a paged result for asset queries.
/// </summary>
public sealed record AssetPagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
