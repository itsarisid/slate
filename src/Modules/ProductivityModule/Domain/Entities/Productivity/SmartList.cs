namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a saved productivity filter.
/// </summary>
public sealed class SmartList : BaseEntity
{
    public Guid OwnerUserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string EntityType { get; private set; } = string.Empty;

    public string CriteriaJson { get; private set; } = "{}";

    public bool IsShared { get; private set; }

    private SmartList()
    {
    }

    private SmartList(Guid ownerUserId, string name, string entityType, string criteriaJson, bool isShared)
    {
        OwnerUserId = ownerUserId;
        Name = name.Trim();
        EntityType = entityType.Trim();
        CriteriaJson = string.IsNullOrWhiteSpace(criteriaJson) ? "{}" : criteriaJson;
        IsShared = isShared;
    }

    public static SmartList Create(Guid ownerUserId, string name, string entityType, string criteriaJson, bool isShared)
        => new(ownerUserId, name, entityType, criteriaJson, isShared);
}
