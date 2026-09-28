// Requires: dotnet add package Bogus
//
// Seeds, in order: WorkTasks -> WorkTaskChecklistItems ->
// WorkTaskDependencies -> WorkTaskTimeEntries -> WorkTaskStatusHistory.
//
// USER REFERENCES: AssigneeId/ReviewerId/etc. are random Guids unless you
// pass real ones via ExistingUserIds.

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
    public class WorkTaskSeedOptions
    {
        public int TaskCount { get; set; } = 40;
        public double SubtaskRatio { get; set; } = 0.25; // fraction of tasks that get a ParentTaskId
        public int DependencyCount { get; set; } = 15;
        public bool WipeExisting { get; set; } = false;
        public IReadOnlyList<Guid> ExistingUserIds { get; set; } = Array.Empty<Guid>();
    }

    public class WorkTaskSeedResult
    {
        public int Tasks { get; set; }
        public int ChecklistItems { get; set; }
        public int Dependencies { get; set; }
        public int TimeEntries { get; set; }
        public int StatusHistoryEntries { get; set; }
    }

    public interface IWorkTaskSeeder
    {
        Task<WorkTaskSeedResult> SeedAsync(WorkTaskSeedOptions options, CancellationToken ct = default);
    }

    public class WorkTaskSeeder : IWorkTaskSeeder
    {
        private readonly DbContext _context; // replace with your actual AppDbContext type

        // The natural forward order through the board, used to build a
        // realistic status history rather than a random walk.
        private static readonly WorkTaskStatus[] StatusOrder =
        {
            WorkTaskStatus.Backlog, WorkTaskStatus.ToDo, WorkTaskStatus.InProgress,
            WorkTaskStatus.InReview, WorkTaskStatus.Done
        };

        private static readonly string[] TitlePool =
        {
            "Implement asset check-in flow", "Fix flaky leave balance calculation",
            "Add pagination to privilege list", "Investigate scheduler job timeout",
            "Write integration tests for workflows", "Refactor notification service",
            "Design onboarding email templates", "Set up staging environment",
            "Optimize dashboard query performance", "Add audit log export",
            "Migrate legacy user import script", "Build asset depreciation report",
            "Fix timezone bug in calendar view", "Add rate limiting to public API",
            "Update API documentation", "Review third-party auth library upgrade"
        };

        public WorkTaskSeeder(DbContext context)
        {
            _context = context;
        }

        public async Task<WorkTaskSeedResult> SeedAsync(WorkTaskSeedOptions options, CancellationToken ct = default)
        {
            if (options.WipeExisting)
                await WipeExistingAsync(ct);

            var random = new Random();
            var faker = new Faker();

            var userIds = options.ExistingUserIds.Count > 0
                ? options.ExistingUserIds
                : Enumerable.Range(0, 15).Select(_ => Guid.NewGuid()).ToList();

            var projectIds = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToList(); // loose grouping only, see entity file

            // ---- Tasks (create parents before children so ParentTaskId is always valid) ----
            var tasks = new List<WorkTask>();
            for (int i = 0; i < options.TaskCount; i++)
            {
                var statusIndex = faker.Random.WeightedRandom(
                    new[] { 0, 1, 2, 3, 4 },
                    new[] { 0.15f, 0.25f, 0.25f, 0.15f, 0.20f });
                var status = StatusOrder[statusIndex];
                var createdAt = faker.Date.Past(1);

                var task = new WorkTask
                {
                    Id = Guid.NewGuid(),
                    Title = faker.PickRandom(TitlePool) + (i >= TitlePool.Length ? $" #{i / TitlePool.Length + 1}" : ""),
                    Description = random.NextDouble() < 0.6 ? faker.Lorem.Paragraph() : null,
                    Priority = faker.PickRandom<Priority>(),
                    Status = status,
                    DueDate = random.NextDouble() < 0.6 ? faker.Date.Between(createdAt, DateTime.UtcNow.AddDays(45)) : null,
                    EstimatedHours = random.NextDouble() < 0.7 ? faker.PickRandom(1, 2, 4, 8, 16, 24) : null,
                    AssigneeId = random.NextDouble() < 0.85 ? userIds[random.Next(userIds.Count)] : null,
                    ReviewerId = status is WorkTaskStatus.InReview or WorkTaskStatus.Done
                        ? userIds[random.Next(userIds.Count)]
                        : (random.NextDouble() < 0.3 ? userIds[random.Next(userIds.Count)] : null),
                    ParentTaskId = null, // filled in below, after all tasks exist
                    ProjectId = random.NextDouble() < 0.8 ? faker.PickRandom(projectIds) : null,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt,
                    CompletedAt = status == WorkTaskStatus.Done ? createdAt.AddDays(random.Next(1, 30)) : null
                };
                tasks.Add(task);
            }

            // Assign ~SubtaskRatio of tasks a parent from among the other tasks
            // (never itself, and never creating a parent/child cycle since
            // every candidate parent was created independently above).
            var subtaskCandidates = tasks.OrderBy(_ => random.Next())
                .Take((int)(tasks.Count * options.SubtaskRatio)).ToList();
            foreach (var task in subtaskCandidates)
            {
                var parent = tasks.Where(t => t.Id != task.Id).OrderBy(_ => random.Next()).First();
                task.ParentTaskId = parent.Id;
                // keep the project consistent with the parent for realism
                task.ProjectId = parent.ProjectId;
            }

            await _context.Set<WorkTask>().AddRangeAsync(tasks, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Checklist items (~40% of tasks get 1-5 items) ----
            var checklistItems = new List<WorkTaskChecklistItem>();
            foreach (var task in tasks)
            {
                if (random.NextDouble() >= 0.4) continue;
                var itemCount = random.Next(1, 6);
                for (int c = 0; c < itemCount; c++)
                {
                    checklistItems.Add(new WorkTaskChecklistItem
                    {
                        Id = Guid.NewGuid(),
                        TaskId = task.Id,
                        Text = faker.Lorem.Sentence(4),
                        Completed = task.Status == WorkTaskStatus.Done || random.NextDouble() < 0.4,
                        Order = c
                    });
                }
            }
            await _context.Set<WorkTaskChecklistItem>().AddRangeAsync(checklistItems, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Dependencies (task depends on an earlier-indexed task — no cycles) ----
            var dependencies = new List<WorkTaskDependency>();
            for (int i = 0; i < options.DependencyCount && tasks.Count >= 2; i++)
            {
                var laterIndex = random.Next(1, tasks.Count);
                var earlierIndex = random.Next(0, laterIndex);
                var laterTask = tasks[laterIndex];
                var earlierTask = tasks[earlierIndex];
                if (laterTask.Id == earlierTask.Id) continue;

                dependencies.Add(new WorkTaskDependency
                {
                    Id = Guid.NewGuid(),
                    TaskId = laterTask.Id,
                    DependsOnTaskId = earlierTask.Id
                });
            }
            await _context.Set<WorkTaskDependency>().AddRangeAsync(dependencies, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Time entries (only for tasks that have moved past ToDo) ----
            var timeEntries = new List<WorkTaskTimeEntry>();
            foreach (var task in tasks.Where(t => t.Status is WorkTaskStatus.InProgress or WorkTaskStatus.InReview or WorkTaskStatus.Done))
            {
                var entryCount = random.Next(1, 6);
                var loggedBy = task.AssigneeId ?? userIds[random.Next(userIds.Count)];
                double totalHours = 0;
                for (int e = 0; e < entryCount; e++)
                {
                    var hours = Math.Round(faker.Random.Double(0.5, 6), 1);
                    totalHours += hours;
                    timeEntries.Add(new WorkTaskTimeEntry
                    {
                        Id = Guid.NewGuid(),
                        TaskId = task.Id,
                        UserId = loggedBy,
                        Hours = hours,
                        Description = random.NextDouble() < 0.5 ? faker.Lorem.Sentence(6) : null,
                        EntryDate = DateOnly.FromDateTime(task.CreatedAt.AddDays(random.Next(0, 20))),
                        CreatedAt = task.CreatedAt.AddDays(random.Next(0, 20))
                    });
                }
                task.ActualHours = Math.Round(totalHours, 1);
            }
            await _context.Set<WorkTaskTimeEntry>().AddRangeAsync(timeEntries, ct);
            await _context.SaveChangesAsync(ct); // also persists task.ActualHours updates

            // ---- Status history (walk forward through StatusOrder up to the task's current status) ----
            var statusHistory = new List<WorkTaskStatusHistory>();
            foreach (var task in tasks)
            {
                var targetIndex = Array.IndexOf(StatusOrder, task.Status);
                if (targetIndex <= 0) continue; // still Backlog, nothing happened yet

                var cursor = task.CreatedAt;
                for (int s = 1; s <= targetIndex; s++)
                {
                    cursor = cursor.AddDays(random.Next(1, 7));
                    statusHistory.Add(new WorkTaskStatusHistory
                    {
                        Id = Guid.NewGuid(),
                        TaskId = task.Id,
                        FromStatus = StatusOrder[s - 1],
                        ToStatus = StatusOrder[s],
                        Comment = random.NextDouble() < 0.3 ? faker.Lorem.Sentence() : null,
                        ChangedByUserId = task.AssigneeId ?? userIds[random.Next(userIds.Count)],
                        ChangedAt = cursor
                    });
                }
            }
            await _context.Set<WorkTaskStatusHistory>().AddRangeAsync(statusHistory, ct);
            await _context.SaveChangesAsync(ct);

            return new WorkTaskSeedResult
            {
                Tasks = tasks.Count,
                ChecklistItems = checklistItems.Count,
                Dependencies = dependencies.Count,
                TimeEntries = timeEntries.Count,
                StatusHistoryEntries = statusHistory.Count
            };
        }

        private async Task WipeExistingAsync(CancellationToken ct)
        {
            _context.Set<WorkTaskStatusHistory>().RemoveRange(_context.Set<WorkTaskStatusHistory>());
            _context.Set<WorkTaskTimeEntry>().RemoveRange(_context.Set<WorkTaskTimeEntry>());
            _context.Set<WorkTaskDependency>().RemoveRange(_context.Set<WorkTaskDependency>());
            _context.Set<WorkTaskChecklistItem>().RemoveRange(_context.Set<WorkTaskChecklistItem>());
            _context.Set<WorkTask>().RemoveRange(_context.Set<WorkTask>());
            await _context.SaveChangesAsync(ct);
        }
    }
}
