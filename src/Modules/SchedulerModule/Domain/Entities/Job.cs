using System.Text.Json;
using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a scheduled job definition.
/// </summary>
public sealed class Job : BaseEntity
{
    private Job()
    {
    }

    private Job(
        string name,
        string description,
        JobType jobType,
        ScheduleType scheduleType,
        string? scheduleExpression,
        int? intervalSeconds,
        DateTimeOffset? runAt,
        string jobConfiguration,
        string retryPolicy,
        string timezone,
        bool isEnabled,
        string createdBy,
        string tagsJson)
    {
        Name = name.Trim();
        Description = description.Trim();
        JobType = jobType;
        ScheduleType = scheduleType;
        ScheduleExpression = scheduleExpression;
        IntervalSeconds = intervalSeconds;
        RunAt = runAt?.ToUniversalTime();
        JobConfiguration = jobConfiguration;
        RetryPolicy = retryPolicy;
        Timezone = timezone;
        IsEnabled = isEnabled;
        CreatedBy = createdBy;
        LastModifiedBy = createdBy;
        Tags = tagsJson;
    }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public JobType JobType { get; private set; }

    public ScheduleType ScheduleType { get; private set; }

    public string? ScheduleExpression { get; private set; }

    public int? IntervalSeconds { get; private set; }

    public DateTimeOffset? RunAt { get; private set; }

    public string JobConfiguration { get; private set; } = "{}";

    public string RetryPolicy { get; private set; } = "{}";

    public string Timezone { get; private set; } = "UTC";

    public bool IsEnabled { get; private set; }

    public bool IsDeleted { get; private set; }

    public bool IsPaused { get; private set; }

    public string CreatedBy { get; private set; } = string.Empty;

    public string LastModifiedBy { get; private set; } = string.Empty;

    public string Tags { get; private set; } = "[]";

    public string? SchedulerJobId { get; private set; }

    public string? ExclusionsJson { get; private set; }

    public string? DependenciesJson { get; private set; }

    public DateTimeOffset? LastExecutedAt { get; private set; }

    public ExecutionStatus? LastExecutionStatus { get; private set; }

    public int ConsecutiveFailures { get; private set; }

    /// <summary>
    /// Creates a new scheduler job.
    /// </summary>
    public static Job Create(
        string name,
        string description,
        JobType jobType,
        ScheduleType scheduleType,
        string? scheduleExpression,
        int? intervalSeconds,
        DateTimeOffset? runAt,
        JsonDocument jobConfiguration,
        JsonDocument retryPolicy,
        string timezone,
        bool isEnabled,
        string createdBy,
        IReadOnlyCollection<string> tags)
    {
        return new Job(
            name,
            description,
            jobType,
            scheduleType,
            scheduleExpression,
            intervalSeconds,
            runAt,
            jobConfiguration.RootElement.GetRawText(),
            retryPolicy.RootElement.GetRawText(),
            string.IsNullOrWhiteSpace(timezone) ? "UTC" : timezone,
            isEnabled,
            createdBy,
            JsonSerializer.Serialize(tags));
    }

    /// <summary>
    /// Updates core metadata for the job.
    /// </summary>
    public void UpdateDetails(
        string? name,
        string? description,
        string? jobConfiguration,
        string? retryPolicy,
        string? timezone,
        bool? isEnabled,
        IReadOnlyCollection<string>? tags,
        string performedBy)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name.Trim();
        }

        if (description is not null)
        {
            Description = description.Trim();
        }

        if (!string.IsNullOrWhiteSpace(jobConfiguration))
        {
            JobConfiguration = jobConfiguration;
        }

        if (!string.IsNullOrWhiteSpace(retryPolicy))
        {
            RetryPolicy = retryPolicy;
        }

        if (!string.IsNullOrWhiteSpace(timezone))
        {
            Timezone = timezone;
        }

        if (isEnabled.HasValue)
        {
            IsEnabled = isEnabled.Value;
            if (!isEnabled.Value)
            {
                IsPaused = true;
            }
        }

        if (tags is not null)
        {
            Tags = JsonSerializer.Serialize(tags);
        }

        LastModifiedBy = performedBy;
        Touch();
    }

    /// <summary>
    /// Updates the scheduling strategy for the job.
    /// </summary>
    public void UpdateSchedule(
        ScheduleType scheduleType,
        string? scheduleExpression,
        int? intervalSeconds,
        DateTimeOffset? runAt,
        string performedBy)
    {
        ScheduleType = scheduleType;
        ScheduleExpression = scheduleExpression;
        IntervalSeconds = intervalSeconds;
        RunAt = runAt?.ToUniversalTime();
        LastModifiedBy = performedBy;
        Touch();
    }

    /// <summary>
    /// Enables a disabled job.
    /// </summary>
    public void Enable(string performedBy)
    {
        IsEnabled = true;
        IsPaused = false;
        LastModifiedBy = performedBy;
        Touch();
    }

    /// <summary>
    /// Disables a job.
    /// </summary>
    public void Disable(string performedBy)
    {
        IsEnabled = false;
        IsPaused = true;
        LastModifiedBy = performedBy;
        Touch();
    }

    /// <summary>
    /// Pauses a job without deleting it.
    /// </summary>
    public void Pause(string performedBy)
    {
        IsPaused = true;
        LastModifiedBy = performedBy;
        Touch();
    }

    /// <summary>
    /// Resumes a paused job.
    /// </summary>
    public void Resume(string performedBy)
    {
        IsPaused = false;
        IsEnabled = true;
        LastModifiedBy = performedBy;
        Touch();
    }

    /// <summary>
    /// Marks a job as softly deleted.
    /// </summary>
    public void SoftDelete(string performedBy)
    {
        IsDeleted = true;
        IsEnabled = false;
        IsPaused = true;
        LastModifiedBy = performedBy;
        Touch();
    }

    /// <summary>
    /// Stores the backend scheduler identifier.
    /// </summary>
    public void SetSchedulerJobId(string? schedulerJobId, string performedBy)
    {
        SchedulerJobId = schedulerJobId;
        LastModifiedBy = performedBy;
        Touch();
    }

    /// <summary>
    /// Stores exclusion rules as JSON.
    /// </summary>
    public void SetExclusions(string exclusionsJson, string performedBy)
    {
        ExclusionsJson = exclusionsJson;
        LastModifiedBy = performedBy;
        Touch();
    }

    /// <summary>
    /// Stores dependency rules as JSON.
    /// </summary>
    public void SetDependencies(string dependenciesJson, string performedBy)
    {
        DependenciesJson = dependenciesJson;
        LastModifiedBy = performedBy;
        Touch();
    }

    /// <summary>
    /// Records the outcome of the latest execution.
    /// </summary>
    public void RecordExecution(ExecutionStatus status)
    {
        LastExecutedAt = DateTimeOffset.UtcNow;
        LastExecutionStatus = status;
        ConsecutiveFailures = status == ExecutionStatus.Failed ? ConsecutiveFailures + 1 : 0;
        Touch();
    }
}
