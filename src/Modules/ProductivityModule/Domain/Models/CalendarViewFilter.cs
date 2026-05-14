namespace Alphabet.Domain.Models;

/// <summary>
/// Represents a calendar view query.
/// </summary>
public sealed record CalendarViewFilter(
    string View,
    DateTimeOffset? Date,
    DateTimeOffset? Start,
    DateTimeOffset? End,
    Guid? OwnerUserId);
