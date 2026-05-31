namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a normalized tag bound to a productivity entity.
/// </summary>
public sealed class Tag : BaseEntity
{
    public string EntityType { get; private set; } = string.Empty;

    public Guid EntityId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string? Color { get; private set; }

    private Tag()
    {
    }

    private Tag(string entityType, Guid entityId, string name, string? color)
    {
        EntityType = entityType.Trim();
        EntityId = entityId;
        Name = name.Trim();
        NormalizedName = name.Trim().ToLowerInvariant();
        Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
    }
    /// <summary>
    /// Create.
    /// </summary>

    public static Tag Create(string entityType, Guid entityId, string name, string? color = null)
        => new(entityType, entityId, name, color);
}
