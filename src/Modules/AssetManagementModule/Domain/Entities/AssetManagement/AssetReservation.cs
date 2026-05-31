namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a future reservation for an asset.
/// </summary>
public sealed class AssetReservation : BaseEntity
{
    private AssetReservation()
    {
    }

    private AssetReservation(Guid assetId, Guid reservedByUserId, DateTimeOffset startAt, DateTimeOffset endAt, string? purpose)
    {
        AssetId = assetId;
        ReservedByUserId = reservedByUserId;
        StartAt = startAt;
        EndAt = endAt;
        Purpose = string.IsNullOrWhiteSpace(purpose) ? null : purpose.Trim();
    }

    public Guid AssetId { get; private set; }

    public Guid ReservedByUserId { get; private set; }

    public DateTimeOffset StartAt { get; private set; }

    public DateTimeOffset EndAt { get; private set; }

    public string? Purpose { get; private set; }

    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Creates a reservation.
    /// </summary>
    public static AssetReservation Create(Guid assetId, Guid reservedByUserId, DateTimeOffset startAt, DateTimeOffset endAt, string? purpose)
    {
        return new AssetReservation(assetId, reservedByUserId, startAt, endAt, purpose);
    }
}
