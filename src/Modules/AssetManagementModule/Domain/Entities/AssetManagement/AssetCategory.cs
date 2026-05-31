using Alphabet.Domain.Exceptions;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents an asset category and optional parent hierarchy.
/// </summary>
public sealed class AssetCategory : BaseEntity
{
    private AssetCategory()
    {
    }

    private AssetCategory(
        string name,
        string description,
        Guid? parentCategoryId,
        string customFieldsSchemaJson,
        decimal? depreciationRate,
        Guid? defaultLocationId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required.");
        }

        Name = name.Trim();
        Description = description.Trim();
        ParentCategoryId = parentCategoryId;
        CustomFieldsSchemaJson = customFieldsSchemaJson;
        DepreciationRate = depreciationRate;
        DefaultLocationId = defaultLocationId;
    }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public Guid? ParentCategoryId { get; private set; }

    public string CustomFieldsSchemaJson { get; private set; } = "{}";

    public decimal? DepreciationRate { get; private set; }

    public Guid? DefaultLocationId { get; private set; }

    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Creates a new asset category.
    /// </summary>
    public static AssetCategory Create(
        string name,
        string description,
        Guid? parentCategoryId,
        string customFieldsSchemaJson,
        decimal? depreciationRate,
        Guid? defaultLocationId)
    {
        return new AssetCategory(name, description, parentCategoryId, customFieldsSchemaJson, depreciationRate, defaultLocationId);
    }
}
