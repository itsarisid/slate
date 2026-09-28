// Entities inferred from CreateTaskRequest, UpdateTaskStatusRequest, and the
// re-used TodoChecklistItem shape (via CreateTaskRequest.checklist) in
// swagger.json. See TaskEnums.cs for why this is WorkTask, not Task.
//
// ProjectId exists on CreateTaskRequest but there's no CreateProjectRequest
// or Project DTO anywhere in swagger.json — Projects aren't a module this
// API exposes (yet?), so ProjectId is just a loose grouping Guid here with
// no seeded Project entity behind it.

using System;
using System.Collections.Generic;

namespace Alphabet.Domain.Entities
{
    public class WorkTask
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public Priority Priority { get; set; }
        public WorkTaskStatus Status { get; set; }
        public DateTime? DueDate { get; set; }
        public double? EstimatedHours { get; set; }
        public double? ActualHours { get; set; } // derived from WorkTaskTimeEntry rows
        public Guid? AssigneeId { get; set; }
        public Guid? ReviewerId { get; set; }
        public Guid? ParentTaskId { get; set; }
        public Guid? ProjectId { get; set; } // loose grouping only — see file header
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }

    public class WorkTaskChecklistItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TaskId { get; set; }
        public string Text { get; set; } = default!;
        public bool Completed { get; set; }
        public int Order { get; set; }
    }

    public class WorkTaskDependency
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TaskId { get; set; }
        public Guid DependsOnTaskId { get; set; }
    }

    public class WorkTaskTimeEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public double Hours { get; set; }
        public string? Description { get; set; }
        public DateOnly EntryDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Not backed by a distinct swagger DTO — added so status changes driven
    // by UpdateTaskStatusRequest leave an audit trail, since the board view
    // (GET /tasks/board) implies status history is meaningful to show.
    public class WorkTaskStatusHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TaskId { get; set; }
        public WorkTaskStatus FromStatus { get; set; }
        public WorkTaskStatus ToStatus { get; set; }
        public string? Comment { get; set; }
        public Guid? ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
