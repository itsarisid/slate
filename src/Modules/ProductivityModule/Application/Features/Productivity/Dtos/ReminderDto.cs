using Alphabet.Domain.Enums;
using Alphabet.Domain.ValueObjects;

namespace Alphabet.Application.Features.Productivity.Dtos;

/// <summary>
/// Represents a reminder read model.
/// </summary>
public sealed record ReminderDto(
    Guid Id,
    string Title,
    string Description,
    DateTimeOffset ReminderTime,
    ReminderType ReminderType,
    ReminderStatus Status,
    IReadOnlyList<string> NotificationMethods,
    string? LinkedEntityType,
    Guid? LinkedEntityId,
    RecurrencePattern? RecurrencePattern);
