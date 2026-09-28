// Entities inferred from CreateSchedulerJobRequest/UpdateSchedulerJobRequest,
// JobDto, JobConfigurationDto, JobExecutionDto, RetryPolicyDto,
// AddJobDependencyRequest, and AddJobExclusionRequest in swagger.json.
//
// JobConfiguration / RetryPolicy are modeled as EF Core owned types (single
// table, prefixed columns) since they're small value-object-ish blobs
// attached 1:1 to a job. Swap to a JSON column if you'd rather.

using System;
using System.Collections.Generic;

namespace Alphabet.Domain.Entities
{
    public class SchedulerJob
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public JobType JobType { get; set; }
        public ScheduleType ScheduleType { get; set; }
        public string? ScheduleExpression { get; set; } // cron expression when ScheduleType == Cron
        public int? IntervalSeconds { get; set; }        // when ScheduleType == Interval
        public DateTime? RunAt { get; set; }              // when ScheduleType == OneTime
        public int? TimeoutSeconds { get; set; }
        public string? Timezone { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsPaused { get; set; }
        public bool IsDeleted { get; set; }
        public List<string> Tags { get; set; } = new();
        public string? CreatedBy { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastExecutedAt { get; set; }
        public ExecutionStatus? LastExecutionStatus { get; set; }
        public int ConsecutiveFailures { get; set; }

        public JobConfiguration Configuration { get; set; } = new();
        public JobRetryPolicy RetryPolicy { get; set; } = new();
    }

    // Owned type — superset of JobConfigurationDto's fields; only the ones
    // relevant to a given JobType are populated by the seeder.
    public class JobConfiguration
    {
        public string? Url { get; set; }
        public string? Method { get; set; }
        public string? Body { get; set; }
        public List<int> RetryOnStatusCodes { get; set; } = new();
        public List<int> SuccessStatusCodes { get; set; } = new();

        public string? StoredProcedureName { get; set; }
        public string? DatabaseConnectionStringName { get; set; }
        public int? CommandTimeoutSeconds { get; set; }

        public string? HandlerType { get; set; }
        public string? MethodName { get; set; }
        public string? Operation { get; set; }

        public string? SourcePath { get; set; }
        public string? DestinationPath { get; set; }
        public string? ArchivePath { get; set; }
        public string? Compression { get; set; }
        public int? DeleteAfterDays { get; set; }
        public int? OlderThanHours { get; set; }
        public bool? DeleteEmptyDirectories { get; set; }
    }

    // Owned type — mirrors RetryPolicyDto.
    public class JobRetryPolicy
    {
        public int MaxRetryAttempts { get; set; }
        public int RetryDelaySeconds { get; set; }
        public RetryBackoffType RetryBackoffType { get; set; }
        public List<string> RetryOnExceptions { get; set; } = new();
    }

    public class JobExecution
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid JobId { get; set; }
        public Guid? TriggeredBy { get; set; } // null = triggered by scheduler, else a UserId
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public ExecutionStatus Status { get; set; }
        public long? DurationMs { get; set; }
        public string? Output { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public Guid? RetryParentId { get; set; } // points back at the original execution being retried
        public DateTime CreatedAt { get; set; }
    }

    public class JobDependency
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid JobId { get; set; }
        public Guid DependsOnJobId { get; set; }
        public string? Condition { get; set; } // e.g. "OnSuccess", "OnCompletion"
    }

    public class JobExclusion
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid JobId { get; set; }
        public List<DateOnly> ExcludedDates { get; set; } = new();
        public List<System.DayOfWeek> ExcludedDaysOfWeek { get; set; } = new();
        public string? TimeRangeStart { get; set; } // "HH:mm"
        public string? TimeRangeEnd { get; set; }
    }
}
