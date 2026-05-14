namespace Alphabet.Domain.Models;

/// <summary>
/// Represents a task board query.
/// </summary>
public sealed record TaskBoardFilter(
    Guid? ProjectId,
    Guid? AssigneeId,
    Guid? OwnerUserId);
