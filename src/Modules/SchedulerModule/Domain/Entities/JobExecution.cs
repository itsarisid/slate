using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a single execution attempt for a job.
/// </summary>
public sealed class JobExecution : BaseEntity
{
    public Guid JobId { get; private set; }

    public Guid? TriggeredBy { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    public DateTimeOffset? EndedAt { get; private set; }

    public ExecutionStatus Status { get; private set; }

    public long? DurationMs { get; private set; }

    public string? Output { get; private set; }

    public string? ErrorMessage { get; private set; }

    public int RetryCount { get; private set; }

    public Guid? RetryParentId { get; private set; }

    private JobExecution()
    {
    }

    private JobExecution(Guid jobId, Guid? triggeredBy, Guid? retryParentId)
    {
        JobId = jobId;
        TriggeredBy = triggeredBy;
        RetryParentId = retryParentId;
        Status = ExecutionStatus.Pending;
        StartedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Creates a new pending execution.
    /// </summary>
    public static JobExecution Create(Guid jobId, Guid? triggeredBy, Guid? retryParentId = null)
        => new(jobId, triggeredBy, retryParentId);

    /// <summary>
    /// Marks the execution as running.
    /// </summary>
    public void Start()
    {
        Status = ExecutionStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>
    /// Marks the execution as successful.
    /// </summary>
    public void CompleteSuccess(string? output)
    {
        Status = ExecutionStatus.Success;
        EndedAt = DateTimeOffset.UtcNow;
        DurationMs = (long)(EndedAt.Value - StartedAt).TotalMilliseconds;
        Output = Truncate(output);
        ErrorMessage = null;
        Touch();
    }

    /// <summary>
    /// Marks the execution as failed.
    /// </summary>
    public void CompleteFailure(string? output, string? errorMessage, int retryCount)
    {
        Status = ExecutionStatus.Failed;
        EndedAt = DateTimeOffset.UtcNow;
        DurationMs = (long)(EndedAt.Value - StartedAt).TotalMilliseconds;
        Output = Truncate(output);
        ErrorMessage = Truncate(errorMessage);
        RetryCount = retryCount;
        Touch();
    }

    /// <summary>
    /// Marks the execution as cancelled.
    /// </summary>
    public void Cancel(string? output = null)
    {
        Status = ExecutionStatus.Cancelled;
        EndedAt = DateTimeOffset.UtcNow;
        DurationMs = (long)(EndedAt.Value - StartedAt).TotalMilliseconds;
        Output = Truncate(output);
        Touch();
    }

    /// <summary>
    /// Marks the execution as timed out.
    /// </summary>
    public void Timeout(string? output, string? errorMessage, int retryCount)
    {
        Status = ExecutionStatus.TimedOut;
        EndedAt = DateTimeOffset.UtcNow;
        DurationMs = (long)(EndedAt.Value - StartedAt).TotalMilliseconds;
        Output = Truncate(output);
        ErrorMessage = Truncate(errorMessage);
        RetryCount = retryCount;
        Touch();
    }

    private static string? Truncate(string? value)
        => string.IsNullOrWhiteSpace(value) ? value : value.Length > 4000 ? value[..4000] : value;
}
