using Alphabet.Domain.Exceptions;

namespace Alphabet.Domain.Entities.Privilege;

/// <summary>
/// Represents a privilege definition.
/// </summary>
public sealed class Privilege : BaseEntity
{
    private readonly HashSet<string> _allowedActions = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _dependsOn = new(StringComparer.OrdinalIgnoreCase);

    private Privilege()
    {
    }

    private Privilege(
        string name,
        string displayName,
        string? description,
        Guid? categoryId,
        string? resourceType,
        IEnumerable<string>? actions,
        bool isGlobal,
        IReadOnlyDictionary<string, string?>? attributes,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Privilege name is required.");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainException("Privilege display name is required.");
        }

        Name = name.Trim().ToLowerInvariant();
        DisplayName = displayName.Trim();
        Description = description?.Trim();
        CategoryId = categoryId;
        ResourceType = resourceType?.Trim();
        IsGlobal = isGlobal;
        CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "system" : createdBy.Trim();

        if (actions is not null)
        {
            foreach (var action in actions.Where(static value => !string.IsNullOrWhiteSpace(value)))
            {
                _allowedActions.Add(action.Trim());
            }
        }

        Attributes = attributes is null
            ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string?>(attributes, StringComparer.OrdinalIgnoreCase);
    }

    public string Name { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public Guid? CategoryId { get; private set; }

    public string? ResourceType { get; private set; }

    public IReadOnlyCollection<string> AllowedActions => _allowedActions.ToArray();

    public bool IsGlobal { get; private set; }

    public bool IsDeprecated { get; private set; }

    public IReadOnlyCollection<string> DependsOn => _dependsOn.ToArray();

    public IDictionary<string, string?> Attributes { get; private set; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    public string CreatedBy { get; private set; } = "system";

    public string? UpdatedBy { get; private set; }

    public static Privilege Create(
        string name,
        string displayName,
        string? description,
        Guid? categoryId,
        string? resourceType,
        IEnumerable<string>? actions,
        bool isGlobal,
        IReadOnlyDictionary<string, string?>? attributes,
        string createdBy)
    {
        return new Privilege(name, displayName, description, categoryId, resourceType, actions, isGlobal, attributes, createdBy);
    }

    /// <summary>
    /// Updates privilege metadata except for its immutable name.
    /// </summary>
    public void UpdateDetails(
        string displayName,
        string? description,
        Guid? categoryId,
        string? resourceType,
        IEnumerable<string>? actions,
        IReadOnlyDictionary<string, string?>? attributes,
        string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainException("Privilege display name is required.");
        }

        DisplayName = displayName.Trim();
        Description = description?.Trim();
        CategoryId = categoryId;
        ResourceType = resourceType?.Trim();
        UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? CreatedBy : updatedBy.Trim();

        _allowedActions.Clear();
        if (actions is not null)
        {
            foreach (var action in actions.Where(static value => !string.IsNullOrWhiteSpace(value)))
            {
                _allowedActions.Add(action.Trim());
            }
        }

        Attributes = attributes is null
            ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string?>(attributes, StringComparer.OrdinalIgnoreCase);

        Touch();
    }

    /// <summary>
    /// Adds a dependency to another privilege name.
    /// </summary>
    public void AddDependency(string privilegeName)
    {
        if (string.IsNullOrWhiteSpace(privilegeName))
        {
            throw new DomainException("Dependency privilege name is required.");
        }

        if (string.Equals(Name, privilegeName.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("A privilege cannot depend on itself.");
        }

        _dependsOn.Add(privilegeName.Trim().ToLowerInvariant());
        Touch();
    }

    /// <summary>
    /// Removes a dependency from the privilege.
    /// </summary>
    public void RemoveDependency(string privilegeName)
    {
        if (string.IsNullOrWhiteSpace(privilegeName))
        {
            return;
        }

        _dependsOn.Remove(privilegeName.Trim().ToLowerInvariant());
        Touch();
    }

    /// <summary>
    /// Replaces the current dependency set.
    /// </summary>
    public void ReplaceDependencies(IEnumerable<string>? privilegeNames)
    {
        _dependsOn.Clear();
        if (privilegeNames is not null)
        {
            foreach (var privilegeName in privilegeNames.Where(static value => !string.IsNullOrWhiteSpace(value)))
            {
                AddDependency(privilegeName);
            }
        }
    }

    /// <summary>
    /// Marks the privilege as deprecated.
    /// </summary>
    public void Deprecate(string updatedBy)
    {
        IsDeprecated = true;
        UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? CreatedBy : updatedBy.Trim();
        Touch();
    }

    /// <summary>
    /// Moves the privilege to a different category.
    /// </summary>
    public void MoveToCategory(Guid? categoryId, string updatedBy)
    {
        CategoryId = categoryId;
        UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? CreatedBy : updatedBy.Trim();
        Touch();
    }
}
