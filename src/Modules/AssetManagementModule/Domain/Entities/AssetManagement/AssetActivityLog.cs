namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents an auditable activity entry for asset management operations.
/// </summary>
public sealed class AssetActivityLog : BaseEntity
{
    private AssetActivityLog()
    {
    }

    private AssetActivityLog(
        Guid? assetId,
        Guid? userId,
        string action,
        string? oldValueJson,
        string? newValueJson,
        string? ipAddress,
        string? userAgent,
        string? reason)
    {
        AssetId = assetId;
        UserId = userId;
        Action = action.Trim();
        OldValueJson = oldValueJson;
        NewValueJson = newValueJson;
        Timestamp = DateTimeOffset.UtcNow;
        IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim();
        UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim();
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }

    public Guid? AssetId { get; private set; }

    public Guid? UserId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string? OldValueJson { get; private set; }

    public string? NewValueJson { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public string? Reason { get; private set; }

    /// <summary>
    /// Creates an activity log entry.
    /// </summary>
    public static AssetActivityLog Create(
        Guid? assetId,
        Guid? userId,
        string action,
        string? oldValueJson,
        string? newValueJson,
        string? ipAddress,
        string? userAgent,
        string? reason)
    {
        return new AssetActivityLog(assetId, userId, action, oldValueJson, newValueJson, ipAddress, userAgent, reason);
    }
}
