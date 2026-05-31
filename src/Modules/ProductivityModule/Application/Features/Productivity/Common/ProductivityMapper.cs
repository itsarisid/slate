using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Domain.Models;
using Alphabet.Domain.ValueObjects;

namespace Alphabet.Application.Features.Productivity.Common;
/// <summary>
/// Productivity mapper.
/// </summary>

public static class ProductivityMapper
{
    /// <summary>
    /// To dto.
    /// </summary>
    public static TodoDto ToDto(this Todo todo)
    => new(
        todo.Id,
        todo.Title,
        todo.Description,
        todo.Priority,
        todo.Status,
        todo.DueDate,
        todo.Category,
        todo.AssignedToUserId,
        todo.ReminderMinutesBefore,
        todo.IsRecurring,
        ProductivityJson.Deserialize<RecurrencePattern>(todo.RecurrencePatternJson),
        todo.CreatedAt,
        todo.CompletedAt);
    /// <summary>
    /// To dto.
    /// </summary>

    public static ReminderDto ToDto(this Reminder reminder)
        => new(
            reminder.Id,
            reminder.Title,
            reminder.Description,
            reminder.ReminderTime,
            reminder.ReminderType,
            reminder.Status,
            reminder.NotificationMethods,
            reminder.LinkedEntityType,
            reminder.LinkedEntityId,
            ProductivityJson.Deserialize<RecurrencePattern>(reminder.RecurrencePatternJson));
    /// <summary>
    /// To dto.
    /// </summary>

    public static NoteDto ToDto(this Note note)
        => new(
            note.Id,
            note.Title,
            note.Content,
            note.Format,
            note.Category,
            note.Color,
            note.IsPinned,
            note.IsFavorite,
            note.NotebookId,
            note.Collaborators,
            note.VersionNumber,
            note.CreatedAt);
    /// <summary>
    /// To dto.
    /// </summary>

    public static TaskDto ToDto(this ProductivityTask task)
        => new(
            task.Id,
            task.Title,
            task.Description,
            task.Priority,
            task.Status,
            task.DueDate,
            task.EstimatedHours,
            task.ActualHours,
            task.AssigneeId,
            task.ReviewerId,
            task.ParentTaskId,
            task.ProjectId,
            task.Checklist,
            task.CreatedAt);
    /// <summary>
    /// To dto.
    /// </summary>

    public static CalendarEventDto ToDto(this CalendarEvent calendarEvent)
        => new(
            calendarEvent.Id,
            calendarEvent.Title,
            calendarEvent.Description,
            calendarEvent.Location,
            calendarEvent.IsAllDay,
            calendarEvent.StartTime,
            calendarEvent.EndTime,
            calendarEvent.Timezone,
            calendarEvent.Attendees,
            calendarEvent.Visibility,
            calendarEvent.Color,
            calendarEvent.ReminderMinutesBefore,
            calendarEvent.ConferenceLink,
            ProductivityJson.Deserialize<RecurrencePattern>(calendarEvent.RecurrencePatternJson));
    /// <summary>
    /// To dto.
    /// </summary>

    public static NoteVersionDto ToDto(this NoteVersionSnapshot snapshot)
        => new(snapshot.VersionNumber, snapshot.Content, snapshot.SavedAt);
}
