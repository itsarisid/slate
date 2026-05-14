using Alphabet.Domain.Enums;
using Alphabet.Domain.Models;
using Alphabet.Domain.ValueObjects;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a calendar event.
/// </summary>
public sealed class CalendarEvent : BaseEntity
{
    public Guid OwnerUserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string? Location { get; private set; }

    public bool IsAllDay { get; private set; }

    public DateTimeOffset StartTime { get; private set; }

    public DateTimeOffset EndTime { get; private set; }

    public string Timezone { get; private set; } = "UTC";

    public string? ConferenceLink { get; private set; }

    public EventVisibility Visibility { get; private set; }

    public string? Color { get; private set; }

    public string AttendeesJson { get; private set; } = "[]";

    public string ReminderMinutesJson { get; private set; } = "[]";

    public string? RecurrencePatternJson { get; private set; }

    public string? ResponsesJson { get; private set; }

    private CalendarEvent()
    {
    }

    private CalendarEvent(
        Guid ownerUserId,
        string title,
        string description,
        string? location,
        bool isAllDay,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        string timezone,
        RecurrencePattern? recurrence,
        IReadOnlyCollection<string>? attendees,
        EventVisibility visibility,
        string? color,
        IReadOnlyCollection<int>? reminderMinutesBefore,
        string? conferenceLink)
    {
        OwnerUserId = ownerUserId;
        Title = title.Trim();
        Description = description.Trim();
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        IsAllDay = isAllDay;
        StartTime = startTime.ToUniversalTime();
        EndTime = endTime.ToUniversalTime();
        Timezone = string.IsNullOrWhiteSpace(timezone) ? "UTC" : timezone.Trim();
        RecurrencePatternJson = recurrence is null ? null : ProductivityJson.Serialize(recurrence);
        AttendeesJson = ProductivityJson.Serialize(attendees);
        Visibility = visibility;
        Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
        ReminderMinutesJson = ProductivityJson.Serialize(reminderMinutesBefore);
        ConferenceLink = string.IsNullOrWhiteSpace(conferenceLink) ? null : conferenceLink.Trim();
        ResponsesJson = ProductivityJson.Serialize<IReadOnlyCollection<CalendarEventResponse>>(Array.Empty<CalendarEventResponse>());
    }

    public static CalendarEvent Create(
        Guid ownerUserId,
        string title,
        string description,
        string? location,
        bool isAllDay,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        string timezone,
        RecurrencePattern? recurrence,
        IReadOnlyCollection<string>? attendees,
        EventVisibility visibility,
        string? color,
        IReadOnlyCollection<int>? reminderMinutesBefore,
        string? conferenceLink)
        => new(ownerUserId, title, description, location, isAllDay, startTime, endTime, timezone, recurrence, attendees, visibility, color, reminderMinutesBefore, conferenceLink);

    public IReadOnlyList<string> Attendees => ProductivityJson.DeserializeList<string>(AttendeesJson);

    public IReadOnlyList<int> ReminderMinutesBefore => ProductivityJson.DeserializeList<int>(ReminderMinutesJson);

    public IReadOnlyList<CalendarEventResponse> Responses => ProductivityJson.DeserializeList<CalendarEventResponse>(ResponsesJson);

    public void Respond(string email, string response)
    {
        var responses = Responses.Where(x => !string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)).ToList();
        responses.Add(new CalendarEventResponse(email.Trim(), response.Trim(), DateTimeOffset.UtcNow));
        ResponsesJson = ProductivityJson.Serialize<IReadOnlyCollection<CalendarEventResponse>>(responses);
        Touch();
    }
}
