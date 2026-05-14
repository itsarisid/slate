namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a comment thread entry for a productivity item.
/// </summary>
public sealed class Comment : BaseEntity
{
    public string EntityType { get; private set; } = string.Empty;

    public Guid EntityId { get; private set; }

    public Guid AuthorUserId { get; private set; }

    public Guid? ParentCommentId { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public bool IsEdited { get; private set; }

    public DateTimeOffset? EditedAt { get; private set; }

    private Comment()
    {
    }

    private Comment(string entityType, Guid entityId, Guid authorUserId, string content, Guid? parentCommentId)
    {
        EntityType = entityType.Trim();
        EntityId = entityId;
        AuthorUserId = authorUserId;
        Content = content.Trim();
        ParentCommentId = parentCommentId;
    }

    public static Comment Create(string entityType, Guid entityId, Guid authorUserId, string content, Guid? parentCommentId = null)
        => new(entityType, entityId, authorUserId, content, parentCommentId);

    public void Edit(string content)
    {
        Content = content.Trim();
        IsEdited = true;
        EditedAt = DateTimeOffset.UtcNow;
        Touch();
    }
}
