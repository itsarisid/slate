namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a reusable productivity item template.
/// </summary>
public sealed class ProductivityTemplate : BaseEntity
{
    public Guid OwnerUserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string EntityType { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string TemplateJson { get; private set; } = "{}";

    private ProductivityTemplate()
    {
    }

    private ProductivityTemplate(Guid ownerUserId, string name, string entityType, string? description, string templateJson)
    {
        OwnerUserId = ownerUserId;
        Name = name.Trim();
        EntityType = entityType.Trim();
        Description = description?.Trim();
        TemplateJson = string.IsNullOrWhiteSpace(templateJson) ? "{}" : templateJson;
    }

    public static ProductivityTemplate Create(Guid ownerUserId, string name, string entityType, string? description, string templateJson)
        => new(ownerUserId, name, entityType, description, templateJson);
}
