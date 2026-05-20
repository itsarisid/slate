using Alphabet.Domain.Exceptions;

namespace Alphabet.Domain.Entities.Privilege;

/// <summary>
/// Represents a privilege category in a hierarchy.
/// </summary>
public sealed class PrivilegeCategory : BaseEntity
{
    private PrivilegeCategory()
    {
    }

    private PrivilegeCategory(string name, string? description, Guid? parentCategoryId, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Privilege category name is required.");
        }

        Name = name.Trim();
        Description = description?.Trim();
        ParentCategoryId = parentCategoryId;
        SortOrder = sortOrder;
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public Guid? ParentCategoryId { get; private set; }

    public int SortOrder { get; private set; }
    /// <summary>
    /// Create.
    /// </summary>

    public static PrivilegeCategory Create(string name, string? description, Guid? parentCategoryId, int sortOrder)
    {
        return new PrivilegeCategory(name, description, parentCategoryId, sortOrder);
    }
}
