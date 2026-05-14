using System.Text.Json;
using Alphabet.Domain.Enums;
using Alphabet.Domain.ValueObjects;
using ProductivityTaskStatus = Alphabet.Domain.Enums.TaskStatus;

namespace Alphabet.Modules.ProductivityModule.Api.Models;

/// <summary>
/// Represents the create todo request payload.
/// </summary>
public sealed class CreateTodoRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Priority Priority { get; init; } = Priority.Medium;
    public DateTimeOffset? DueDate { get; init; }
    public int? ReminderMinutesBefore { get; init; }
    public string? Category { get; init; }
    public IReadOnlyCollection<string>? Tags { get; init; }
    public bool IsRecurring { get; init; }
    public RecurrencePattern? RecurrencePattern { get; init; }
    public Guid? AssignedTo { get; init; }
}

/// <summary>
/// Represents the update todo request payload.
/// </summary>
public sealed class UpdateTodoRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Priority Priority { get; init; } = Priority.Medium;
    public DateTimeOffset? DueDate { get; init; }
    public int? ReminderMinutesBefore { get; init; }
    public string? Category { get; init; }
    public IReadOnlyCollection<string>? Tags { get; init; }
    public bool IsRecurring { get; init; }
    public RecurrencePattern? RecurrencePattern { get; init; }
    public Guid? AssignedTo { get; init; }
}

/// <summary>
/// Represents a bulk todo operation payload.
/// </summary>
public sealed class BulkTodoOperationRequest
{
    public IReadOnlyCollection<Guid> TodoIds { get; init; } = Array.Empty<Guid>();
}

/// <summary>
/// Represents a reminder creation payload.
/// </summary>
public sealed class CreateReminderRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset ReminderTime { get; init; }
    public ReminderType ReminderType { get; init; } = ReminderType.Once;
    public int? RepeatInterval { get; init; }
    public int? RepeatCount { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public bool SoundEnabled { get; init; } = true;
    public bool VibrationEnabled { get; init; }
    public bool SnoozeEnabled { get; init; } = true;
    public int? SnoozeMinutes { get; init; } = 10;
    public string? LinkedEntityType { get; init; }
    public Guid? LinkedEntityId { get; init; }
    public IReadOnlyCollection<string> NotificationMethods { get; init; } = ["Email", "InApp"];
    public RecurrencePattern? RecurrencePattern { get; init; }
}

/// <summary>
/// Represents a reminder snooze payload.
/// </summary>
public sealed class SnoozeReminderRequest
{
    public int Minutes { get; init; }
}

/// <summary>
/// Represents a note creation payload.
/// </summary>
public sealed class CreateNoteRequest
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public NoteFormat Format { get; init; } = NoteFormat.Markdown;
    public string? Category { get; init; }
    public IReadOnlyCollection<string>? Tags { get; init; }
    public string? Color { get; init; }
    public bool IsPinned { get; init; }
    public bool IsFavorite { get; init; }
    public Guid? NotebookId { get; init; }
    public IReadOnlyCollection<string>? Collaborators { get; init; }
}

/// <summary>
/// Represents a note update payload.
/// </summary>
public sealed class UpdateNoteRequest
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public NoteFormat Format { get; init; } = NoteFormat.Markdown;
    public string? Category { get; init; }
    public string? Color { get; init; }
    public bool IsPinned { get; init; }
    public bool IsFavorite { get; init; }
    public Guid? NotebookId { get; init; }
}

/// <summary>
/// Represents a share note payload.
/// </summary>
public sealed class ShareNoteRequest
{
    public string Email { get; init; } = string.Empty;
    public string Permission { get; init; } = "View";
}

/// <summary>
/// Represents a notebook creation payload.
/// </summary>
public sealed class CreateNotebookRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Color { get; init; }
    public Guid? ParentNotebookId { get; init; }
}

/// <summary>
/// Represents a task creation payload.
/// </summary>
public sealed class CreateTaskRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Priority Priority { get; init; } = Priority.Medium;
    public ProductivityTaskStatus Status { get; init; } = ProductivityTaskStatus.NotStarted;
    public DateTimeOffset? DueDate { get; init; }
    public decimal? EstimatedHours { get; init; }
    public Guid? AssigneeId { get; init; }
    public Guid? ReviewerId { get; init; }
    public Guid? ParentTaskId { get; init; }
    public Guid? ProjectId { get; init; }
    public IReadOnlyCollection<Guid>? Dependencies { get; init; }
    public IReadOnlyCollection<TodoChecklistItem>? Checklist { get; init; }
}

/// <summary>
/// Represents a task status update payload.
/// </summary>
public sealed class UpdateTaskStatusRequest
{
    public ProductivityTaskStatus Status { get; init; }
    public string? Comment { get; init; }
}

/// <summary>
/// Represents a task time entry payload.
/// </summary>
public sealed class AddTimeEntryRequest
{
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// Represents a calendar event creation payload.
/// </summary>
public sealed class CreateEventRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Location { get; init; }
    public bool IsAllDay { get; init; }
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public string Timezone { get; init; } = "UTC";
    public RecurrencePattern? Recurrence { get; init; }
    public IReadOnlyCollection<string>? Attendees { get; init; }
    public EventVisibility Visibility { get; init; } = EventVisibility.Team;
    public string? Color { get; init; }
    public IReadOnlyCollection<int>? ReminderMinutesBefore { get; init; }
    public string? ConferenceLink { get; init; }
}

/// <summary>
/// Represents an availability request payload.
/// </summary>
public sealed class CheckAvailabilityRequest
{
    public IReadOnlyCollection<Guid> UserIds { get; init; } = Array.Empty<Guid>();
    public AvailabilityDateRange DateRange { get; init; } = new();
    public int DurationMinutes { get; init; } = 60;
}

/// <summary>
/// Represents a date range payload.
/// </summary>
public sealed class AvailabilityDateRange
{
    public DateTimeOffset Start { get; init; }
    public DateTimeOffset End { get; init; }
}

/// <summary>
/// Represents an event response payload.
/// </summary>
public sealed class RespondToEventRequest
{
    public string Response { get; init; } = "Accepted";
}

/// <summary>
/// Represents a meeting-time suggestion request.
/// </summary>
public sealed class SuggestMeetingTimesRequest
{
    public IReadOnlyCollection<string> Attendees { get; init; } = Array.Empty<string>();
    public int DurationMinutes { get; init; } = 60;
    public AvailabilityDateRange DateRange { get; init; } = new();
    public WorkingHoursRequest? WorkingHours { get; init; }
}

/// <summary>
/// Represents working-hour preferences.
/// </summary>
public sealed class WorkingHoursRequest
{
    public string? Start { get; init; }
    public string? End { get; init; }
    public string? Timezone { get; init; }
}

/// <summary>
/// Represents a smart-list creation payload.
/// </summary>
public sealed class CreateSmartListRequest
{
    public string Name { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public JsonDocument Criteria { get; init; } = JsonDocument.Parse("{}");
    public bool IsShared { get; init; }
}

/// <summary>
/// Represents a template creation payload.
/// </summary>
public sealed class CreateTemplateRequest
{
    public string Name { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public string? Description { get; init; }
    public JsonDocument Template { get; init; } = JsonDocument.Parse("{}");
}

/// <summary>
/// Represents a reminder-from-entity payload.
/// </summary>
public sealed class CreateReminderFromEntityRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset ReminderTime { get; init; }
}

/// <summary>
/// Represents a note collaboration delta.
/// </summary>
public sealed class NoteDelta
{
    public string Operation { get; init; } = string.Empty;
    public int Position { get; init; }
    public string Text { get; init; } = string.Empty;
}
