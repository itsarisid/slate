using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Models;

/// <summary>
/// Represents filtering options for reminder queries.
/// </summary>
public sealed record ReminderQueryFilter(
    DateTimeOffset? From,
    DateTimeOffset? To,
    ReminderType? Type,
    ReminderStatus? Status,
    Guid? OwnerUserId);
