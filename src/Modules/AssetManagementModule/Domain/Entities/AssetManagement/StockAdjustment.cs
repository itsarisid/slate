using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a stock adjustment event.
/// </summary>
public sealed class StockAdjustment : BaseEntity
{
    private StockAdjustment()
    {
    }

    private StockAdjustment(
        Guid assetId,
        Guid locationId,
        StockAdjustmentType adjustmentType,
        int quantity,
        string reason,
        Guid performedByUserId,
        string? referenceNumber)
    {
        AssetId = assetId;
        LocationId = locationId;
        AdjustmentType = adjustmentType;
        Quantity = quantity;
        Reason = reason.Trim();
        PerformedByUserId = performedByUserId;
        ReferenceNumber = string.IsNullOrWhiteSpace(referenceNumber) ? null : referenceNumber.Trim();
        PerformedAt = DateTimeOffset.UtcNow;
    }

    public Guid AssetId { get; private set; }

    public Guid LocationId { get; private set; }

    public StockAdjustmentType AdjustmentType { get; private set; }

    public int Quantity { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public Guid PerformedByUserId { get; private set; }

    public string? ReferenceNumber { get; private set; }

    public DateTimeOffset PerformedAt { get; private set; }

    /// <summary>
    /// Creates a stock adjustment event.
    /// </summary>
    public static StockAdjustment Create(
        Guid assetId,
        Guid locationId,
        StockAdjustmentType adjustmentType,
        int quantity,
        string reason,
        Guid performedByUserId,
        string? referenceNumber)
    {
        return new StockAdjustment(assetId, locationId, adjustmentType, quantity, reason, performedByUserId, referenceNumber);
    }
}
