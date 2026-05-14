using Alphabet.Domain.Enums;
using ProductivityTaskStatus = Alphabet.Domain.Enums.TaskStatus;

namespace Alphabet.Application.Features.Productivity.Dtos;

/// <summary>
/// Represents a Kanban board column.
/// </summary>
public sealed record TaskBoardColumnDto(
    ProductivityTaskStatus Status,
    IReadOnlyList<TaskDto> Tasks);
