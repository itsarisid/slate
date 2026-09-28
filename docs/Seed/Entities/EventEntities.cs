// Entities inferred from CreateEventRequest, RespondToEventRequest, and
// EventVisibility in swagger.json.

using System;
using System.Collections.Generic;

namespace Alphabet.Domain.Entities
{
    public class Event
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrganizerUserId { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public bool IsAllDay { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Timezone { get; set; }
        public EventRecurrencePattern? Recurrence { get; set; }
        public EventVisibility Visibility { get; set; }
        public string? Color { get; set; }
        public List<int> ReminderMinutesBefore { get; set; } = new();
        public string? ConferenceLink { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Owned type — same shape as RecurrencePattern used by Todos (Module 9),
    // duplicated here as its own class since Event and Todo may not want to
    // share a table-mapped owned type across two different owners.
    public class EventRecurrencePattern
    {
        public string Pattern { get; set; } = default!;
        public int Interval { get; set; } = 1;
        public List<string> DaysOfWeek { get; set; } = new();
        public DateTime? EndDate { get; set; }
        public int? MaxOccurrences { get; set; }
        public string? CustomExpression { get; set; }
    }

    public class EventAttendee
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EventId { get; set; }
        public string Email { get; set; } = default!;
        public Guid? UserId { get; set; } // set when the email matches a known seeded user
        public string Response { get; set; } = "NoResponse"; // "Accepted" | "Declined" | "Tentative" | "NoResponse"
        public DateTime? RespondedAt { get; set; }
    }
}
