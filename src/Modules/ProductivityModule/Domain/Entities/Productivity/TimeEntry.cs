namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents tracked time for a task.
/// </summary>
public sealed class TimeEntry : BaseEntity
{
    public Guid ProductivityTaskId { get; private set; }

    public Guid UserId { get; private set; }

    public DateTimeOffset StartTime { get; private set; }

    public DateTimeOffset EndTime { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public decimal DurationHours => (decimal)(EndTime - StartTime).TotalHours;

    private TimeEntry()
    {
    }

    private TimeEntry(Guid productivityTaskId, Guid userId, DateTimeOffset startTime, DateTimeOffset endTime, string description)
    {
        ProductivityTaskId = productivityTaskId;
        UserId = userId;
        StartTime = startTime.ToUniversalTime();
        EndTime = endTime.ToUniversalTime();
        Description = description.Trim();
    }

    public static TimeEntry Create(Guid productivityTaskId, Guid userId, DateTimeOffset startTime, DateTimeOffset endTime, string description)
        => new(productivityTaskId, userId, startTime, endTime, description);
}
