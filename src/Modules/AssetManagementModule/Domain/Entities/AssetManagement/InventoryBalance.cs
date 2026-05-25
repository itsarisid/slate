namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents the current inventory balance for a stock-tracked asset and location.
/// </summary>
public sealed class InventoryBalance : BaseEntity
{
    private InventoryBalance()
    {
    }

    private InventoryBalance(Guid assetId, Guid locationId, int quantityOnHand, int minimumThreshold)
    {
        AssetId = assetId;
        LocationId = locationId;
        QuantityOnHand = quantityOnHand;
        MinimumThreshold = minimumThreshold;
    }

    public Guid AssetId { get; private set; }

    public Guid LocationId { get; private set; }

    public int QuantityOnHand { get; private set; }

    public int MinimumThreshold { get; private set; }

    public DateTimeOffset? LastCountedAt { get; private set; }

    /// <summary>
    /// Creates a new inventory balance row.
    /// </summary>
    public static InventoryBalance Create(Guid assetId, Guid locationId, int quantityOnHand, int minimumThreshold)
    {
        return new InventoryBalance(assetId, locationId, quantityOnHand, minimumThreshold);
    }

    /// <summary>
    /// Applies a stock adjustment.
    /// </summary>
    public void Apply(int quantityOnHand)
    {
        QuantityOnHand = quantityOnHand;
        Touch();
    }

    /// <summary>
    /// Marks the inventory balance as physically counted.
    /// </summary>
    public void MarkCounted()
    {
        LastCountedAt = DateTimeOffset.UtcNow;
        Touch();
    }
}
