// Requires: dotnet add package Bogus
//
// Seeds, in dependency order:
//   SchedulerJobs (with embedded Configuration/RetryPolicy) -> JobExecutions
//   -> JobDependencies -> JobExclusions
//
// NOTE on owned types: JobConfiguration/JobRetryPolicy are modeled as EF Core
// owned types in SchedulerJobEntities.cs. If your DbContext configures them
// with OwnsOne(...), you don't need anything extra here — just assign them
// on the SchedulerJob instance as normal.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Alphabet.Domain.Entities;

namespace Alphabet.Infrastructure.Data.Seed
{
    public class SchedulerJobSeedOptions
    {
        public int JobCount { get; set; } = 25;
        public int MinExecutionsPerJob { get; set; } = 3;
        public int MaxExecutionsPerJob { get; set; } = 15;
        public int DependencyCount { get; set; } = 8;
        public int ExclusionCount { get; set; } = 5;
        public bool WipeExisting { get; set; } = false;
        public IReadOnlyList<Guid> ExistingUserIds { get; set; } = Array.Empty<Guid>();
    }

    public class SchedulerJobSeedResult
    {
        public int Jobs { get; set; }
        public int Executions { get; set; }
        public int Dependencies { get; set; }
        public int Exclusions { get; set; }
    }

    public interface ISchedulerJobSeeder
    {
        Task<SchedulerJobSeedResult> SeedAsync(SchedulerJobSeedOptions options, CancellationToken ct = default);
    }

    public class SchedulerJobSeeder : ISchedulerJobSeeder
    {
        private readonly DbContext _context; // replace with your actual AppDbContext type

        private static readonly string[] CronPresets =
        {
            "0 0 * * *",      // daily midnight
            "0 */6 * * *",    // every 6 hours
            "*/15 * * * *",   // every 15 minutes
            "0 9 * * 1-5",    // weekdays 9am
            "0 0 1 * *",      // monthly
        };

        private static readonly string[] NamePool =
        {
            "Sync Asset Depreciation", "Send Leave Reminder Emails", "Recalculate Leave Balances",
            "Purge Old Notifications", "Nightly Report Export", "Sync Users from HRIS",
            "Archive Completed Workflows", "Cleanup Temp Files", "Refresh Privilege Cache",
            "Send Maintenance Due Alerts", "Generate Payroll Export", "Backup Database",
            "Retry Failed Communications", "Recompute Analytics Snapshots", "Expire Stale Access Requests",
            "Sync Calendar Holidays", "Rebuild Search Index", "Process Stock Take Reconciliation",
            "Send Scheduler Health Report", "Compress Old Execution Logs"
        };

        public SchedulerJobSeeder(DbContext context)
        {
            _context = context;
        }

        public async Task<SchedulerJobSeedResult> SeedAsync(SchedulerJobSeedOptions options, CancellationToken ct = default)
        {
            if (options.WipeExisting)
                await WipeExistingAsync(ct);

            var random = new Random();
            var faker = new Faker();

            var userIds = options.ExistingUserIds.Count > 0
                ? options.ExistingUserIds
                : Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();

            // ---- Scheduler Jobs ----
            var jobs = new List<SchedulerJob>();
            for (int i = 0; i < options.JobCount; i++)
            {
                var jobType = (JobType)random.Next(1, 5);
                var scheduleType = (ScheduleType)random.Next(1, 4);
                var name = NamePool[i % NamePool.Length] + (i >= NamePool.Length ? $" #{i / NamePool.Length + 1}" : "");

                var job = new SchedulerJob
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = faker.Lorem.Sentence(8),
                    JobType = jobType,
                    ScheduleType = scheduleType,
                    TimeoutSeconds = faker.PickRandom(30, 60, 120, 300, 600),
                    Timezone = faker.PickRandom("UTC", "Asia/Qatar", "Europe/London", "America/New_York"),
                    IsEnabled = random.NextDouble() < 0.9,
                    IsPaused = random.NextDouble() < 0.1,
                    IsDeleted = false,
                    Tags = faker.Random.Bool(0.6f)
                        ? new List<string> { faker.PickRandom("nightly", "reporting", "cleanup", "hr", "finance", "critical") }
                        : new List<string>(),
                    CreatedBy = "seed",
                    CreatedAt = faker.Date.Past(1),
                    UpdatedAt = DateTime.UtcNow,
                    RetryPolicy = new JobRetryPolicy
                    {
                        MaxRetryAttempts = faker.Random.Int(0, 5),
                        RetryDelaySeconds = faker.PickRandom(10, 30, 60, 120),
                        RetryBackoffType = faker.PickRandom<RetryBackoffType>(),
                        RetryOnExceptions = jobType == JobType.HttpCallback
                            ? new List<string> { "System.Net.Http.HttpRequestException", "System.TimeoutException" }
                            : new List<string> { "System.Exception" }
                    }
                };

                switch (scheduleType)
                {
                    case ScheduleType.Cron:
                        job.ScheduleExpression = faker.PickRandom(CronPresets);
                        break;
                    case ScheduleType.Interval:
                        job.IntervalSeconds = faker.PickRandom(60, 300, 900, 3600, 21600);
                        break;
                    case ScheduleType.OneTime:
                        job.RunAt = faker.Date.Soon(14);
                        break;
                }

                job.Configuration = jobType switch
                {
                    JobType.HttpCallback => new JobConfiguration
                    {
                        Url = faker.Internet.Url() + "/api/internal/" + faker.Lorem.Word(),
                        Method = faker.PickRandom("GET", "POST"),
                        Body = null,
                        RetryOnStatusCodes = new List<int> { 500, 502, 503, 504 },
                        SuccessStatusCodes = new List<int> { 200, 201, 202 }
                    },
                    JobType.StoredProcedure => new JobConfiguration
                    {
                        StoredProcedureName = "sp_" + faker.Lorem.Word() + "_" + faker.Lorem.Word(),
                        DatabaseConnectionStringName = "DefaultConnection",
                        CommandTimeoutSeconds = faker.PickRandom(30, 60, 120)
                    },
                    JobType.InternalHandler => new JobConfiguration
                    {
                        HandlerType = "Alphabet.Application.Jobs." + faker.Lorem.Word().Transform(),
                        MethodName = "ExecuteAsync",
                        Operation = faker.PickRandom("Sync", "Recalculate", "Notify", "Cleanup")
                    },
                    JobType.FileOperation => new JobConfiguration
                    {
                        SourcePath = "/data/exports/" + faker.Lorem.Word(),
                        DestinationPath = "/data/archive/" + faker.Lorem.Word(),
                        ArchivePath = random.NextDouble() < 0.5 ? "/data/archive/zipped" : null,
                        Compression = random.NextDouble() < 0.5 ? "zip" : null,
                        DeleteAfterDays = faker.PickRandom<int?>(7, 30, 90, null),
                        OlderThanHours = faker.PickRandom<int?>(24, 72, null),
                        DeleteEmptyDirectories = faker.Random.Bool()
                    },
                    _ => new JobConfiguration()
                };

                jobs.Add(job);
            }
            await _context.Set<SchedulerJob>().AddRangeAsync(jobs, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Job Executions ----
            var executions = new List<JobExecution>();
            foreach (var job in jobs)
            {
                var execCount = random.Next(options.MinExecutionsPerJob, options.MaxExecutionsPerJob + 1);
                JobExecution? previous = null;
                var consecutiveFailures = 0;

                for (int i = 0; i < execCount; i++)
                {
                    var startedAt = faker.Date.Past(1).AddMinutes(i * random.Next(30, 720));
                    if (startedAt > DateTime.UtcNow) startedAt = DateTime.UtcNow.AddMinutes(-random.Next(1, 1000));

                    var succeeded = random.NextDouble() < 0.85;
                    var status = succeeded ? ExecutionStatus.Succeeded : faker.PickRandom(
                        ExecutionStatus.Failed, ExecutionStatus.TimedOut, ExecutionStatus.Cancelled);
                    var durationMs = (long)random.Next(200, 45000);

                    var execution = new JobExecution
                    {
                        Id = Guid.NewGuid(),
                        JobId = job.Id,
                        TriggeredBy = random.NextDouble() < 0.15 ? userIds[random.Next(userIds.Count)] : null,
                        StartedAt = startedAt,
                        EndedAt = startedAt.AddMilliseconds(durationMs),
                        Status = status,
                        DurationMs = durationMs,
                        Output = status == ExecutionStatus.Succeeded ? "Completed successfully." : null,
                        ErrorMessage = status != ExecutionStatus.Succeeded ? faker.Lorem.Sentence() : null,
                        RetryCount = 0,
                        RetryParentId = null,
                        CreatedAt = startedAt
                    };
                    executions.Add(execution);

                    // occasionally add a retry attempt for a failed execution
                    if (status != ExecutionStatus.Succeeded && job.RetryPolicy.MaxRetryAttempts > 0 && random.NextDouble() < 0.6)
                    {
                        var retrySucceeded = random.NextDouble() < 0.7;
                        var retryStart = execution.EndedAt!.Value.AddSeconds(job.RetryPolicy.RetryDelaySeconds);
                        executions.Add(new JobExecution
                        {
                            Id = Guid.NewGuid(),
                            JobId = job.Id,
                            TriggeredBy = null,
                            StartedAt = retryStart,
                            EndedAt = retryStart.AddMilliseconds(durationMs),
                            Status = retrySucceeded ? ExecutionStatus.Succeeded : ExecutionStatus.Failed,
                            DurationMs = durationMs,
                            Output = retrySucceeded ? "Completed successfully on retry." : null,
                            ErrorMessage = retrySucceeded ? null : faker.Lorem.Sentence(),
                            RetryCount = 1,
                            RetryParentId = execution.Id,
                            CreatedAt = retryStart
                        });
                    }

                    previous = execution;
                    consecutiveFailures = status == ExecutionStatus.Succeeded ? 0 : consecutiveFailures + 1;
                }

                if (previous != null)
                {
                    job.LastExecutedAt = previous.StartedAt;
                    job.LastExecutionStatus = previous.Status;
                    job.ConsecutiveFailures = consecutiveFailures;
                }
            }
            await _context.Set<JobExecution>().AddRangeAsync(executions, ct);
            await _context.SaveChangesAsync(ct); // also persists the LastExecutedAt/etc. updates on jobs

            // ---- Job Dependencies (job A depends on job B completing first) ----
            var dependencies = new List<JobDependency>();
            for (int i = 0; i < options.DependencyCount && jobs.Count >= 2; i++)
            {
                var job = jobs[random.Next(jobs.Count)];
                Guid dependsOn;
                do { dependsOn = jobs[random.Next(jobs.Count)].Id; } while (dependsOn == job.Id);

                dependencies.Add(new JobDependency
                {
                    Id = Guid.NewGuid(),
                    JobId = job.Id,
                    DependsOnJobId = dependsOn,
                    Condition = faker.PickRandom("OnSuccess", "OnCompletion")
                });
            }
            await _context.Set<JobDependency>().AddRangeAsync(dependencies, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Job Exclusions (blackout windows for a job's schedule) ----
            var exclusions = new List<JobExclusion>();
            for (int i = 0; i < options.ExclusionCount && jobs.Count > 0; i++)
            {
                var job = jobs[random.Next(jobs.Count)];
                exclusions.Add(new JobExclusion
                {
                    Id = Guid.NewGuid(),
                    JobId = job.Id,
                    ExcludedDates = random.NextDouble() < 0.4
                        ? new List<DateOnly> { DateOnly.FromDateTime(faker.Date.Soon(60)) }
                        : new List<DateOnly>(),
                    ExcludedDaysOfWeek = random.NextDouble() < 0.5
                        ? new List<System.DayOfWeek> { System.DayOfWeek.Saturday, System.DayOfWeek.Sunday }
                        : new List<System.DayOfWeek>(),
                    TimeRangeStart = random.NextDouble() < 0.3 ? "22:00" : null,
                    TimeRangeEnd = random.NextDouble() < 0.3 ? "06:00" : null
                });
            }
            await _context.Set<JobExclusion>().AddRangeAsync(exclusions, ct);
            await _context.SaveChangesAsync(ct);

            return new SchedulerJobSeedResult
            {
                Jobs = jobs.Count,
                Executions = executions.Count,
                Dependencies = dependencies.Count,
                Exclusions = exclusions.Count
            };
        }

        private async Task WipeExistingAsync(CancellationToken ct)
        {
            _context.Set<JobExclusion>().RemoveRange(_context.Set<JobExclusion>());
            _context.Set<JobDependency>().RemoveRange(_context.Set<JobDependency>());
            _context.Set<JobExecution>().RemoveRange(_context.Set<JobExecution>());
            _context.Set<SchedulerJob>().RemoveRange(_context.Set<SchedulerJob>());
            await _context.SaveChangesAsync(ct);
        }
    }

    // Small helper used above to make a Title Case handler class name from a
    // random lorem word (e.g. "recalculate" -> "Recalculate").
    internal static class StringExtensions
    {
        public static string Transform(this string s) =>
            string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..] + "JobHandler";
    }
}
