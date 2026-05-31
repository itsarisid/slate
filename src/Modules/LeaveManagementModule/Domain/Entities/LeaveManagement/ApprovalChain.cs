using System.ComponentModel.DataAnnotations.Schema;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a configurable N-level leave approval chain.
/// </summary>
public sealed class ApprovalChain : BaseEntity
{
    private ApprovalChain()
    {
    }

    private ApprovalChain(string name, string code, string description, ApprovalChainApplicability applicableTo, IReadOnlyCollection<ApprovalLevelDefinition> levels, int finalApprovalLevel, bool allowSkipLevels, bool parallelApproval, bool isActive)
    {
        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        Description = description.Trim();
        ApplicableToJson = LeaveManagementJson.Serialize(applicableTo);
        ApprovalLevelsJson = LeaveManagementJson.Serialize(levels.OrderBy(x => x.Level).ToArray());
        FinalApprovalLevel = finalApprovalLevel;
        AllowSkipLevels = allowSkipLevels;
        ParallelApproval = parallelApproval;
        IsActive = isActive;
    }

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string ApplicableToJson { get; private set; } = "{}";

    public string ApprovalLevelsJson { get; private set; } = "[]";

    public int FinalApprovalLevel { get; private set; }

    public bool AllowSkipLevels { get; private set; }

    public bool ParallelApproval { get; private set; }

    public bool IsActive { get; private set; }

    [NotMapped]
    public ApprovalChainApplicability ApplicableTo => LeaveManagementJson.Deserialize<ApprovalChainApplicability>(ApplicableToJson)
        ?? new ApprovalChainApplicability([], [], [], [], 0, null);

    [NotMapped]
    public IReadOnlyList<ApprovalLevelDefinition> ApprovalLevels => LeaveManagementJson.DeserializeList<ApprovalLevelDefinition>(ApprovalLevelsJson);

    /// <summary>
    /// Creates an approval chain.
    /// </summary>
    public static ApprovalChain Create(string name, string code, string description, ApprovalChainApplicability applicableTo, IReadOnlyCollection<ApprovalLevelDefinition> levels, int finalApprovalLevel, bool allowSkipLevels, bool parallelApproval, bool isActive)
    {
        return new ApprovalChain(name, code, description, applicableTo, levels, finalApprovalLevel, allowSkipLevels, parallelApproval, isActive);
    }
}
