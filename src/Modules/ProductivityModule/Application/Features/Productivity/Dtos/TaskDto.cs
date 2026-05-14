using Alphabet.Domain.Enums;
using Alphabet.Domain.ValueObjects;
using ProductivityTaskStatus = Alphabet.Domain.Enums.TaskStatus;

namespace Alphabet.Application.Features.Productivity.Dtos;

/// <summary>
/// Represents a task read model.
/// </summary>
public sealed record TaskDto(
    Guid Id,
    string Title,
    string Description,
    Priority Priority,
    ProductivityTaskStatus Status,
    DateTimeOffset? DueDate,
    decimal? EstimatedHours,
    decimal? ActualHours,
    Guid? AssigneeId,
    Guid? ReviewerId,
    Guid? ParentTaskId,
    Guid? ProjectId,
    IReadOnlyList<TodoChecklistItem> Checklist,
    DateTimeOffset CreatedAt);
