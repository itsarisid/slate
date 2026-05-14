using Alphabet.Domain.Enums;
using Alphabet.Domain.Models;
using Alphabet.Domain.ValueObjects;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a personal or collaborative todo item.
/// </summary>
public sealed class Todo : BaseEntity
{
    public Guid CreatedByUserId { get; private set; }

    public Guid? AssignedToUserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public Priority Priority { get; private set; }

    public DateTimeOffset? DueDate { get; private set; }

    public TodoStatus Status { get; private set; } = TodoStatus.Pending;

    public DateTimeOffset? CompletedAt { get; private set; }

    public string? Category { get; private set; }

    public int? ReminderMinutesBefore { get; private set; }

    public bool IsRecurring { get; private set; }

    public string? RecurrencePatternJson { get; private set; }

    public Guid? ConvertedTaskId { get; private set; }

    public bool IsDeleted { get; private set; }

    private Todo()
    {
    }

    private Todo(
        Guid createdByUserId,
        Guid? assignedToUserId,
        string title,
        string description,
        Priority priority,
        DateTimeOffset? dueDate,
        string? category,
        int? reminderMinutesBefore,
        bool isRecurring,
        RecurrencePattern? recurrencePattern)
    {
        CreatedByUserId = createdByUserId;
        AssignedToUserId = assignedToUserId ?? createdByUserId;
        Title = title.Trim();
        Description = description.Trim();
        Priority = priority;
        DueDate = dueDate?.ToUniversalTime();
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        ReminderMinutesBefore = reminderMinutesBefore;
        IsRecurring = isRecurring;
        RecurrencePatternJson = recurrencePattern is null ? null : ProductivityJson.Serialize(recurrencePattern);
    }

    public static Todo Create(
        Guid createdByUserId,
        Guid? assignedToUserId,
        string title,
        string description,
        Priority priority,
        DateTimeOffset? dueDate,
        string? category,
        int? reminderMinutesBefore,
        bool isRecurring,
        RecurrencePattern? recurrencePattern)
        => new(createdByUserId, assignedToUserId, title, description, priority, dueDate, category, reminderMinutesBefore, isRecurring, recurrencePattern);

    public void UpdateDetails(
        string title,
        string description,
        Priority priority,
        DateTimeOffset? dueDate,
        string? category,
        Guid? assignedToUserId,
        int? reminderMinutesBefore,
        bool isRecurring,
        RecurrencePattern? recurrencePattern)
    {
        Title = title.Trim();
        Description = description.Trim();
        Priority = priority;
        DueDate = dueDate?.ToUniversalTime();
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        AssignedToUserId = assignedToUserId;
        ReminderMinutesBefore = reminderMinutesBefore;
        IsRecurring = isRecurring;
        RecurrencePatternJson = recurrencePattern is null ? null : ProductivityJson.Serialize(recurrencePattern);
        Touch();
    }

    public void Complete()
    {
        Status = TodoStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        Touch();
    }

    public void Uncomplete()
    {
        Status = TodoStatus.Pending;
        CompletedAt = null;
        Touch();
    }

    public void Archive()
    {
        Status = TodoStatus.Archived;
        Touch();
    }

    public void MoveToTrash()
    {
        IsDeleted = true;
        Status = TodoStatus.Trash;
        Touch();
    }

    public void Restore()
    {
        IsDeleted = false;
        Status = TodoStatus.Pending;
        Touch();
    }

    public void MarkConverted(Guid taskId)
    {
        ConvertedTaskId = taskId;
        Touch();
    }
}
