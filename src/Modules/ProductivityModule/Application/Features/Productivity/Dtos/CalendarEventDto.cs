using Alphabet.Domain.Enums;
using Alphabet.Domain.ValueObjects;

namespace Alphabet.Application.Features.Productivity.Dtos;

/// <summary>
/// Represents a calendar event read model.
/// </summary>
public sealed record CalendarEventDto(
    Guid Id,
    string Title,
    string Description,
    string? Location,
    bool IsAllDay,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Timezone,
    IReadOnlyList<string> Attendees,
    EventVisibility Visibility,
    string? Color,
    IReadOnlyList<int> ReminderMinutesBefore,
    string? ConferenceLink,
    RecurrencePattern? RecurrencePattern);
