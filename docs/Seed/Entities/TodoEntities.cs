// Entities inferred from CreateTodoRequest/UpdateTodoRequest,
// TodoChecklistItem, and RecurrencePattern in swagger.json.

using System;
using System.Collections.Generic;

namespace Alphabet.Domain.Entities
{
    public class Todo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } // owner
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public Priority Priority { get; set; }
        public TodoStatus Status { get; set; }
        public DateTime? DueDate { get; set; }
        public int? ReminderMinutesBefore { get; set; }
        public string? Category { get; set; }
        public List<string> Tags { get; set; } = new();
        public bool IsRecurring { get; set; }
        public TodoRecurrencePattern? RecurrencePattern { get; set; }
        public Guid? AssignedTo { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // Owned type — mirrors RecurrencePattern from swagger.
    public class TodoRecurrencePattern
    {
        public string Pattern { get; set; } = default!; // e.g. "Daily", "Weekly", "Monthly", "Custom"
        public int Interval { get; set; } = 1;
        public List<string> DaysOfWeek { get; set; } = new(); // e.g. ["Monday","Wednesday"]
        public DateTime? EndDate { get; set; }
        public int? MaxOccurrences { get; set; }
        public string? CustomExpression { get; set; }
    }

    public class TodoChecklistItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TodoId { get; set; }
        public string Text { get; set; } = default!;
        public bool Completed { get; set; }
        public int Order { get; set; }
    }
}
