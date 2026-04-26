namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a security-relevant audit log record.
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? UserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;
}
