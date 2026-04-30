namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Scheduler module configuration.
/// </summary>
public sealed class SchedulerSettings
{
    public const string SectionName = "Scheduler";

    public string Provider { get; init; } = "Hangfire";

    public HangfireSettings Hangfire { get; init; } = new();

    public QuartzSettings Quartz { get; init; } = new();

    public SchedulerJobsSettings Jobs { get; init; } = new();

    public SchedulerRetrySettings Retry { get; init; } = new();

    public SchedulerMonitoringSettings Monitoring { get; init; } = new();
}

public sealed class HangfireSettings
{
    public string? ConnectionStringName { get; init; }

    public bool DashboardEnabled { get; init; } = true;

    public string DashboardPath { get; init; } = "/hangfire";

    public int JobExpirationDays { get; init; } = 30;

    public int WorkerCount { get; init; } = 5;

    public string[] Queues { get; init; } = ["default", "critical", "background"];
}

public sealed class QuartzSettings
{
    public string InstanceName { get; init; } = "AlphabetScheduler";

    public bool Clustered { get; init; }

    public string TablePrefix { get; init; } = "QRTZ_";
}

public sealed class SchedulerJobsSettings
{
    public int DefaultTimeoutSeconds { get; init; } = 300;

    public int MaxConcurrentJobs { get; init; } = 10;

    public int HeartbeatIntervalSeconds { get; init; } = 30;

    public int DeadJobCleanupDays { get; init; } = 90;

    public string[] AllowedFileRoots { get; init; } = ["C:\\Users\\moinc\\OneDrive\\Documents\\New project"];
}

public sealed class SchedulerRetrySettings
{
    public int DefaultMaxAttempts { get; init; } = 3;

    public int DefaultDelaySeconds { get; init; } = 60;
}

public sealed class SchedulerMonitoringSettings
{
    public string FailedJobsAlertEmail { get; init; } = "admin@alphabet.local";

    public int AlertOnConsecutiveFailures { get; init; } = 3;
}
