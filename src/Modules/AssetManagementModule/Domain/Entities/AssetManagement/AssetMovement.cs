namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a location movement event for an asset.
/// </summary>
public sealed class AssetMovement : BaseEntity
{
    private AssetMovement()
    {
    }

    private AssetMovement(Guid assetId, Guid? fromLocationId, Guid toLocationId, string reason, Guid movedByUserId)
    {
        AssetId = assetId;
        FromLocationId = fromLocationId;
        ToLocationId = toLocationId;
        Reason = reason.Trim();
        MovedByUserId = movedByUserId;
        MovedAt = DateTimeOffset.UtcNow;
    }

    public Guid AssetId { get; private set; }

    public Guid? FromLocationId { get; private set; }

    public Guid ToLocationId { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public Guid MovedByUserId { get; private set; }

    public DateTimeOffset MovedAt { get; private set; }

    /// <summary>
    /// Creates a movement entry.
    /// </summary>
    public static AssetMovement Create(Guid assetId, Guid? fromLocationId, Guid toLocationId, string reason, Guid movedByUserId)
    {
        return new AssetMovement(assetId, fromLocationId, toLocationId, reason, movedByUserId);
    }
}
