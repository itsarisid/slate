using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents an audit trail entry for a scheduled job.
/// </summary>
public sealed class JobHistory : BaseEntity
{
    private JobHistory()
    {
    }

    private JobHistory(Guid jobId, JobHistoryAction action, string changes, string performedBy, string? ipAddress)
    {
        JobId = jobId;
        Action = action;
        Changes = changes;
        PerformedBy = performedBy;
        PerformedAt = DateTimeOffset.UtcNow;
        IpAddress = ipAddress;
    }

    public Guid JobId { get; private set; }

    public JobHistoryAction Action { get; private set; }

    public string Changes { get; private set; } = "{}";

    public string PerformedBy { get; private set; } = string.Empty;

    public DateTimeOffset PerformedAt { get; private set; }

    public string? IpAddress { get; private set; }

    /// <summary>
    /// Creates a new history entry.
    /// </summary>
    public static JobHistory Create(Guid jobId, JobHistoryAction action, string changes, string performedBy, string? ipAddress)
        => new(jobId, action, changes, performedBy, ipAddress);
}
