// Two distinct concepts share the "workflow" name in swagger.json:
//
//  A) Approval workflows — CreateWorkflowDefinitionRequest/
//     CreateWorkflowDefinitionStepRequest define a reusable, multi-step
//     approval template (e.g. "started" against an asset via
//     /workflows/assets/{assetId}/start). StartWorkflowRequest and
//     WorkflowStepActionRequest drive runtime instances of it.
//     => WorkflowDefinition, WorkflowDefinitionStep, WorkflowInstance,
//        WorkflowStepInstance
//
//  B) Scheduler DAG workflows — CreateWorkflowRequest/CreateWorkflowJobRequest
//     (posted to /scheduler/workflows) bundle existing SchedulerJobs (see
//     Module 5) into a dependency graph with per-job onFailure behavior.
//     => SchedulerWorkflow, SchedulerWorkflowJob

using System;
using System.Collections.Generic;

namespace Alphabet.Domain.Entities
{
    // ---- A) Approval workflows ----

    public class WorkflowDefinition
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int Version { get; set; } = 1;
        public bool IsActive { get; set; } = true;
    }

    public class WorkflowDefinitionStep
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkflowDefinitionId { get; set; }
        public string Name { get; set; } = default!;
        public int Order { get; set; }
        public string? AssignedToRole { get; set; }
        public int RequiredApprovals { get; set; } = 1;
        public int TimeoutHours { get; set; }
        public List<string> Actions { get; set; } = new(); // e.g. "Approve", "Reject", "RequestChanges"
    }

    public class WorkflowInstance
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkflowDefinitionId { get; set; }
        public string EntityType { get; set; } = default!; // e.g. "Asset"
        public Guid EntityId { get; set; }                 // e.g. the assetId that started it
        public Dictionary<string, string?> Context { get; set; } = new();
        public WorkflowInstanceStatus Status { get; set; }
        public int CurrentStepOrder { get; set; }
        public DateTime StartedAt { get; set; }
        public Guid? StartedByUserId { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class WorkflowStepInstance
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkflowInstanceId { get; set; }
        public Guid WorkflowDefinitionStepId { get; set; }
        public int Order { get; set; }
        public WorkflowStepInstanceStatus Status { get; set; }
        public Guid? ActionTakenByUserId { get; set; }
        public DateTime? ActionTakenAt { get; set; }
        public string? Action { get; set; } // free-text, mirrors WorkflowStepActionRequest.Action
        public string? Comment { get; set; }
    }

    // ---- B) Scheduler DAG workflows ----

    public class SchedulerWorkflow
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class SchedulerWorkflowJob
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SchedulerWorkflowId { get; set; }
        public Guid JobId { get; set; } // FK to SchedulerJob (Module 5)
        public List<Guid> DependsOn { get; set; } = new(); // other JobIds within the same workflow
        public string? OnFailure { get; set; } // e.g. "Stop", "Continue", "Retry"
    }
}
