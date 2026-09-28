// Requires: dotnet add package Bogus
//
// Seeds two independent groups, each in FK-safe order:
//
//  A) WorkflowDefinitions -> WorkflowDefinitionSteps -> WorkflowInstances ->
//     WorkflowStepInstances
//  B) SchedulerWorkflows -> SchedulerWorkflowJobs
//
// USER REFERENCES: StartedByUserId / ActionTakenByUserId are random Guids
// unless you pass real ones via ExistingUserIds.
//
// SCHEDULER JOB REFERENCES: SchedulerWorkflowJob.JobId points at SchedulerJob
// rows from Module 5. Pass real job IDs via ExistingSchedulerJobIds (e.g. by
// running the SchedulerJob seeder first and querying its results) — falls
// back to random Guids otherwise.

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
    public class WorkflowSeedOptions
    {
        public int DefinitionCount { get; set; } = 6;
        public int InstancesPerDefinition { get; set; } = 5;
        public int SchedulerWorkflowCount { get; set; } = 4;
        public int MinJobsPerSchedulerWorkflow { get; set; } = 2;
        public int MaxJobsPerSchedulerWorkflow { get; set; } = 6;
        public bool WipeExisting { get; set; } = false;
        public IReadOnlyList<Guid> ExistingUserIds { get; set; } = Array.Empty<Guid>();
        public IReadOnlyList<Guid> ExistingSchedulerJobIds { get; set; } = Array.Empty<Guid>();
    }

    public class WorkflowSeedResult
    {
        public int Definitions { get; set; }
        public int DefinitionSteps { get; set; }
        public int Instances { get; set; }
        public int StepInstances { get; set; }
        public int SchedulerWorkflows { get; set; }
        public int SchedulerWorkflowJobs { get; set; }
    }

    public interface IWorkflowSeeder
    {
        Task<WorkflowSeedResult> SeedAsync(WorkflowSeedOptions options, CancellationToken ct = default);
    }

    public class WorkflowSeeder : IWorkflowSeeder
    {
        private readonly DbContext _context; // replace with your actual AppDbContext type

        private static readonly (string Name, string[] StepNames, string[] Roles)[] DefinitionTemplates =
        {
            ("Asset Retirement Approval",
                new[] { "Manager Review", "Finance Sign-off", "IT Confirmation" },
                new[] { "Manager", "Finance", "IT" }),
            ("High-Value Asset Purchase",
                new[] { "Budget Check", "Manager Approval", "Procurement Approval" },
                new[] { "Finance", "Manager", "Procurement" }),
            ("Leave Escalation",
                new[] { "Team Lead Review", "HR Review" },
                new[] { "TeamLead", "HR" }),
            ("Privilege Elevation Request",
                new[] { "Security Review", "Manager Approval", "Final Grant" },
                new[] { "Security", "Manager", "Admin" }),
            ("New Employee Onboarding",
                new[] { "HR Setup", "IT Provisioning", "Manager Welcome" },
                new[] { "HR", "IT", "Manager" }),
            ("Vendor Contract Renewal",
                new[] { "Legal Review", "Finance Review", "Executive Approval" },
                new[] { "Legal", "Finance", "Executive" }),
        };

        public WorkflowSeeder(DbContext context)
        {
            _context = context;
        }

        public async Task<WorkflowSeedResult> SeedAsync(WorkflowSeedOptions options, CancellationToken ct = default)
        {
            if (options.WipeExisting)
                await WipeExistingAsync(ct);

            var random = new Random();
            var faker = new Faker();

            var userIds = options.ExistingUserIds.Count > 0
                ? options.ExistingUserIds
                : Enumerable.Range(0, 15).Select(_ => Guid.NewGuid()).ToList();

            var jobIds = options.ExistingSchedulerJobIds.Count > 0
                ? options.ExistingSchedulerJobIds
                : Enumerable.Range(0, 20).Select(_ => Guid.NewGuid()).ToList();

            // ======== A) Approval workflows ========

            var definitions = new List<WorkflowDefinition>();
            var steps = new List<WorkflowDefinitionStep>();

            var templateCount = Math.Min(options.DefinitionCount, DefinitionTemplates.Length);
            for (int i = 0; i < templateCount; i++)
            {
                var template = DefinitionTemplates[i];
                var definition = new WorkflowDefinition
                {
                    Id = Guid.NewGuid(),
                    Name = template.Name,
                    Description = $"Approval workflow for {template.Name.ToLowerInvariant()}.",
                    Version = 1,
                    IsActive = true
                };
                definitions.Add(definition);

                for (int s = 0; s < template.StepNames.Length; s++)
                {
                    steps.Add(new WorkflowDefinitionStep
                    {
                        Id = Guid.NewGuid(),
                        WorkflowDefinitionId = definition.Id,
                        Name = template.StepNames[s],
                        Order = s + 1,
                        AssignedToRole = template.Roles[s],
                        RequiredApprovals = 1,
                        TimeoutHours = faker.PickRandom(24, 48, 72),
                        Actions = new List<string> { "Approve", "Reject", "RequestChanges" }
                    });
                }
            }
            await _context.Set<WorkflowDefinition>().AddRangeAsync(definitions, ct);
            await _context.Set<WorkflowDefinitionStep>().AddRangeAsync(steps, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Instances + step instances ----
            var instances = new List<WorkflowInstance>();
            var stepInstances = new List<WorkflowStepInstance>();

            foreach (var definition in definitions)
            {
                var definitionSteps = steps.Where(s => s.WorkflowDefinitionId == definition.Id)
                    .OrderBy(s => s.Order).ToList();

                for (int i = 0; i < options.InstancesPerDefinition; i++)
                {
                    var startedAt = faker.Date.Past(1);
                    var startedBy = userIds[random.Next(userIds.Count)];

                    // Decide how far this instance progressed.
                    var outcome = random.NextDouble();
                    int stepsCompleted;
                    WorkflowInstanceStatus finalStatus;

                    if (outcome < 0.6) // fully approved, completed
                    {
                        stepsCompleted = definitionSteps.Count;
                        finalStatus = WorkflowInstanceStatus.Completed;
                    }
                    else if (outcome < 0.8) // rejected partway through
                    {
                        stepsCompleted = random.Next(1, definitionSteps.Count + 1);
                        finalStatus = WorkflowInstanceStatus.Rejected;
                    }
                    else if (outcome < 0.9) // cancelled partway through
                    {
                        stepsCompleted = random.Next(0, definitionSteps.Count);
                        finalStatus = WorkflowInstanceStatus.Cancelled;
                    }
                    else // still in progress
                    {
                        stepsCompleted = random.Next(0, definitionSteps.Count);
                        finalStatus = WorkflowInstanceStatus.InProgress;
                    }

                    var instance = new WorkflowInstance
                    {
                        Id = Guid.NewGuid(),
                        WorkflowDefinitionId = definition.Id,
                        EntityType = faker.PickRandom("Asset", "PrivilegeAccessRequest", "LeaveRequest", "User"),
                        EntityId = Guid.NewGuid(), // seed placeholder; wire to a real entity ID if you have one
                        Context = new Dictionary<string, string?>
                        {
                            ["initiatedBy"] = "seed",
                            ["reason"] = faker.Lorem.Sentence(5)
                        },
                        Status = finalStatus,
                        CurrentStepOrder = finalStatus == WorkflowInstanceStatus.Completed
                            ? definitionSteps.Count
                            : Math.Min(stepsCompleted + 1, definitionSteps.Count),
                        StartedAt = startedAt,
                        StartedByUserId = startedBy,
                        CompletedAt = finalStatus is WorkflowInstanceStatus.Completed or WorkflowInstanceStatus.Rejected or WorkflowInstanceStatus.Cancelled
                            ? startedAt.AddHours(random.Next(2, 96))
                            : null
                    };
                    instances.Add(instance);

                    var cursor = startedAt;
                    for (int s = 0; s < definitionSteps.Count; s++)
                    {
                        var defStep = definitionSteps[s];
                        WorkflowStepInstanceStatus stepStatus;
                        string? action = null;

                        if (s < stepsCompleted)
                        {
                            // approved unless this is the exact step where a rejection happened
                            var isRejectionStep = finalStatus == WorkflowInstanceStatus.Rejected && s == stepsCompleted - 1;
                            stepStatus = isRejectionStep ? WorkflowStepInstanceStatus.Rejected : WorkflowStepInstanceStatus.Approved;
                            action = isRejectionStep ? "Reject" : "Approve";
                        }
                        else if (s == stepsCompleted && finalStatus == WorkflowInstanceStatus.InProgress)
                        {
                            stepStatus = WorkflowStepInstanceStatus.Pending;
                        }
                        else if (finalStatus == WorkflowInstanceStatus.Cancelled && s >= stepsCompleted)
                        {
                            stepStatus = WorkflowStepInstanceStatus.Skipped;
                        }
                        else
                        {
                            stepStatus = WorkflowStepInstanceStatus.Pending;
                        }

                        cursor = cursor.AddHours(random.Next(1, 36));
                        stepInstances.Add(new WorkflowStepInstance
                        {
                            Id = Guid.NewGuid(),
                            WorkflowInstanceId = instance.Id,
                            WorkflowDefinitionStepId = defStep.Id,
                            Order = defStep.Order,
                            Status = stepStatus,
                            ActionTakenByUserId = stepStatus is WorkflowStepInstanceStatus.Approved or WorkflowStepInstanceStatus.Rejected
                                ? userIds[random.Next(userIds.Count)]
                                : null,
                            ActionTakenAt = stepStatus is WorkflowStepInstanceStatus.Approved or WorkflowStepInstanceStatus.Rejected
                                ? cursor
                                : null,
                            Action = action,
                            Comment = action != null && random.NextDouble() < 0.4 ? faker.Lorem.Sentence() : null
                        });
                    }
                }
            }
            await _context.Set<WorkflowInstance>().AddRangeAsync(instances, ct);
            await _context.Set<WorkflowStepInstance>().AddRangeAsync(stepInstances, ct);
            await _context.SaveChangesAsync(ct);

            // ======== B) Scheduler DAG workflows ========

            var schedulerWorkflows = new List<SchedulerWorkflow>();
            var schedulerWorkflowJobs = new List<SchedulerWorkflowJob>();

            var workflowNamePool = new[]
            {
                "Nightly Data Pipeline", "Month-End Close Pipeline", "Onboarding Automation",
                "Report Distribution Pipeline", "Asset Lifecycle Pipeline", "Payroll Export Pipeline"
            };

            for (int i = 0; i < options.SchedulerWorkflowCount; i++)
            {
                var workflow = new SchedulerWorkflow
                {
                    Id = Guid.NewGuid(),
                    Name = workflowNamePool[i % workflowNamePool.Length] + (i >= workflowNamePool.Length ? $" #{i + 1}" : ""),
                    CreatedAt = faker.Date.Past(1)
                };
                schedulerWorkflows.Add(workflow);

                var jobCount = random.Next(options.MinJobsPerSchedulerWorkflow, options.MaxJobsPerSchedulerWorkflow + 1);
                var chosenJobIds = jobIds.OrderBy(_ => random.Next()).Take(Math.Min(jobCount, jobIds.Count)).ToList();

                // Build a simple linear-ish chain with occasional fan-in, so
                // DependsOn always points at an earlier job in the list (no cycles).
                for (int j = 0; j < chosenJobIds.Count; j++)
                {
                    var dependsOn = new List<Guid>();
                    if (j > 0 && random.NextDouble() < 0.8)
                    {
                        dependsOn.Add(chosenJobIds[j - 1]);
                        if (j > 1 && random.NextDouble() < 0.3)
                            dependsOn.Add(chosenJobIds[random.Next(0, j - 1)]);
                    }

                    schedulerWorkflowJobs.Add(new SchedulerWorkflowJob
                    {
                        Id = Guid.NewGuid(),
                        SchedulerWorkflowId = workflow.Id,
                        JobId = chosenJobIds[j],
                        DependsOn = dependsOn,
                        OnFailure = faker.PickRandom("Stop", "Continue", "Retry")
                    });
                }
            }
            await _context.Set<SchedulerWorkflow>().AddRangeAsync(schedulerWorkflows, ct);
            await _context.Set<SchedulerWorkflowJob>().AddRangeAsync(schedulerWorkflowJobs, ct);
            await _context.SaveChangesAsync(ct);

            return new WorkflowSeedResult
            {
                Definitions = definitions.Count,
                DefinitionSteps = steps.Count,
                Instances = instances.Count,
                StepInstances = stepInstances.Count,
                SchedulerWorkflows = schedulerWorkflows.Count,
                SchedulerWorkflowJobs = schedulerWorkflowJobs.Count
            };
        }

        private async Task WipeExistingAsync(CancellationToken ct)
        {
            _context.Set<SchedulerWorkflowJob>().RemoveRange(_context.Set<SchedulerWorkflowJob>());
            _context.Set<SchedulerWorkflow>().RemoveRange(_context.Set<SchedulerWorkflow>());
            _context.Set<WorkflowStepInstance>().RemoveRange(_context.Set<WorkflowStepInstance>());
            _context.Set<WorkflowInstance>().RemoveRange(_context.Set<WorkflowInstance>());
            _context.Set<WorkflowDefinitionStep>().RemoveRange(_context.Set<WorkflowDefinitionStep>());
            _context.Set<WorkflowDefinition>().RemoveRange(_context.Set<WorkflowDefinition>());
            await _context.SaveChangesAsync(ct);
        }
    }
}
