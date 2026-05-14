using Alphabet.Domain.Enums;
using Alphabet.Domain.ValueObjects;

namespace Alphabet.Application.Features.Productivity.Dtos;

/// <summary>
/// Represents a todo read model.
/// </summary>
public sealed record TodoDto(
    Guid Id,
    string Title,
    string Description,
    Priority Priority,
    TodoStatus Status,
    DateTimeOffset? DueDate,
    string? Category,
    Guid? AssignedTo,
    int? ReminderMinutesBefore,
    bool IsRecurring,
    RecurrencePattern? RecurrencePattern,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);
