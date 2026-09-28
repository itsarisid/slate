// Entity inferred from CreateReminderRequest/CreateReminderFromEntityRequest
// and SnoozeReminderRequest in swagger.json.

using System;
using System.Collections.Generic;

namespace Alphabet.Domain.Entities
{
    public class Reminder
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime ReminderTime { get; set; }
        public ReminderType ReminderType { get; set; }
        public ReminderStatus Status { get; set; }
        public int? RepeatInterval { get; set; }
        public int? RepeatCount { get; set; }
        public DateTime? EndDate { get; set; }
        public bool SoundEnabled { get; set; } = true;
        public bool VibrationEnabled { get; set; } = true;
        public bool SnoozeEnabled { get; set; } = true;
        public int? SnoozeMinutes { get; set; }
        public string? LinkedEntityType { get; set; } // e.g. "Todo", "Event", "LeaveRequest"
        public Guid? LinkedEntityId { get; set; }
        public List<string> NotificationMethods { get; set; } = new(); // e.g. "Push", "Email", "SMS"
        public ReminderRecurrencePattern? RecurrencePattern { get; set; }
        public DateTime? SnoozedUntil { get; set; }
        public DateTime? DismissedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Owned type — same shape as Todo's/Event's RecurrencePattern, kept
    // separate per-owner as with Module 11.
    public class ReminderRecurrencePattern
    {
        public string Pattern { get; set; } = default!;
        public int Interval { get; set; } = 1;
        public List<string> DaysOfWeek { get; set; } = new();
        public DateTime? EndDate { get; set; }
        public int? MaxOccurrences { get; set; }
        public string? CustomExpression { get; set; }
    }
}
