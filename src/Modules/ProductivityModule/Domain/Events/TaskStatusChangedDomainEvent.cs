using ProductivityTaskStatus = Alphabet.Domain.Enums.TaskStatus;

namespace Alphabet.Domain.Events;

/// <summary>
/// Raised when a task status changes.
/// </summary>
public sealed record TaskStatusChangedDomainEvent(Guid TaskId, ProductivityTaskStatus Status, Guid? AssigneeId);
