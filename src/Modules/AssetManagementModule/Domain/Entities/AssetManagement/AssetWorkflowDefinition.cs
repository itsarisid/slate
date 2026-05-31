using System.ComponentModel.DataAnnotations.Schema;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a workflow definition for asset-related approvals or processes.
/// </summary>
public sealed class AssetWorkflowDefinition : BaseEntity
{
    private AssetWorkflowDefinition()
    {
    }

    private AssetWorkflowDefinition(
        string name,
        string description,
        int version,
        IReadOnlyCollection<AssetWorkflowStepDefinitionModel> steps)
    {
        Name = name.Trim();
        Description = description.Trim();
        Version = version;
        StepsJson = AssetManagementJson.Serialize(steps);
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public int Version { get; private set; }

    public string StepsJson { get; private set; } = "[]";

    public bool IsActive { get; private set; }

    [NotMapped]
    public IReadOnlyList<AssetWorkflowStepDefinitionModel> Steps => AssetManagementJson.DeserializeList<AssetWorkflowStepDefinitionModel>(StepsJson);

    /// <summary>
    /// Creates a workflow definition.
    /// </summary>
    public static AssetWorkflowDefinition Create(
        string name,
        string description,
        int version,
        IReadOnlyCollection<AssetWorkflowStepDefinitionModel> steps)
    {
        return new AssetWorkflowDefinition(name, description, version, steps);
    }
}
