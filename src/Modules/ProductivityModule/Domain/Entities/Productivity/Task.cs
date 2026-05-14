using Alphabet.Domain.Enums;
using Alphabet.Domain.Models;
using Alphabet.Domain.ValueObjects;
using ProductivityTaskStatus = Alphabet.Domain.Enums.TaskStatus;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents an advanced work task with status, checklist, and tracking support.
/// </summary>
public sealed class ProductivityTask : BaseEntity
{
    public Guid OwnerUserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public Priority Priority { get; private set; }

    public ProductivityTaskStatus Status { get; private set; }

    public DateTimeOffset? DueDate { get; private set; }

    public decimal? EstimatedHours { get; private set; }

    public decimal? ActualHours { get; private set; }

    public Guid? AssigneeId { get; private set; }

    public Guid? ReviewerId { get; private set; }

    public Guid? ParentTaskId { get; private set; }

    public Guid? ProjectId { get; private set; }

    public string ChecklistJson { get; private set; } = "[]";

    public string CommentsJson { get; private set; } = "[]";

    private ProductivityTask()
    {
    }

    private ProductivityTask(
        Guid ownerUserId,
        string title,
        string description,
        Priority priority,
        ProductivityTaskStatus status,
        DateTimeOffset? dueDate,
        decimal? estimatedHours,
        Guid? assigneeId,
        Guid? reviewerId,
        Guid? parentTaskId,
        Guid? projectId,
        IReadOnlyCollection<TodoChecklistItem>? checklist)
    {
        OwnerUserId = ownerUserId;
        Title = title.Trim();
        Description = description.Trim();
        Priority = priority;
        Status = status;
        DueDate = dueDate?.ToUniversalTime();
        EstimatedHours = estimatedHours;
        AssigneeId = assigneeId;
        ReviewerId = reviewerId;
        ParentTaskId = parentTaskId;
        ProjectId = projectId;
        ChecklistJson = ProductivityJson.Serialize(checklist);
    }

    public static ProductivityTask Create(
        Guid ownerUserId,
        string title,
        string description,
        Priority priority,
        ProductivityTaskStatus status,
        DateTimeOffset? dueDate,
        decimal? estimatedHours,
        Guid? assigneeId,
        Guid? reviewerId,
        Guid? parentTaskId,
        Guid? projectId,
        IReadOnlyCollection<TodoChecklistItem>? checklist)
        => new(ownerUserId, title, description, priority, status, dueDate, estimatedHours, assigneeId, reviewerId, parentTaskId, projectId, checklist);

    public IReadOnlyList<TodoChecklistItem> Checklist => ProductivityJson.DeserializeList<TodoChecklistItem>(ChecklistJson);

    public void UpdateStatus(ProductivityTaskStatus status, string? comment)
    {
        Status = status;
        if (!string.IsNullOrWhiteSpace(comment))
        {
            var comments = ProductivityJson.DeserializeList<string>(CommentsJson).ToList();
            comments.Add(comment.Trim());
            CommentsJson = ProductivityJson.Serialize<IReadOnlyCollection<string>>(comments);
        }

        Touch();
    }

    public void AddTime(decimal hours)
    {
        ActualHours = (ActualHours ?? 0m) + hours;
        Touch();
    }
}
