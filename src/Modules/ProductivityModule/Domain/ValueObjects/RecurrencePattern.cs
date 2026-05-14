using System.ComponentModel.DataAnnotations.Schema;

namespace Alphabet.Domain.ValueObjects;

/// <summary>
/// Represents recurrence metadata for reminders, todos, and calendar events.
/// </summary>
[NotMapped]
public sealed record RecurrencePattern(
    string Pattern,
    int Interval,
    IReadOnlyCollection<string>? DaysOfWeek,
    DateTimeOffset? EndDate,
    int? MaxOccurrences,
    string? CustomExpression);
