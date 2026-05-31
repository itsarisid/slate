using System.ComponentModel.DataAnnotations.Schema;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Exceptions;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents configurable leave policy rules for a leave category.
/// </summary>
public sealed class LeaveType : BaseEntity
{
    private LeaveType()
    {
    }

    private LeaveType(
        string name,
        string code,
        string description,
        string color,
        string? icon,
        bool isPaid,
        decimal defaultDaysPerYear,
        int? maxConsecutiveDays,
        decimal minDaysPerRequest,
        decimal? maxDaysPerRequest,
        bool requiresApproval,
        Guid? approvalChainId,
        bool carryForwardEnabled,
        decimal maxCarryForwardDays,
        int carryForwardExpiryMonths,
        bool encashmentEnabled,
        decimal? encashmentRate,
        bool prorationEnabled,
        LeaveEligibilityRules eligibilityRules,
        IReadOnlyCollection<LeaveBlackoutDate> blackoutDates,
        bool requiresAttachment,
        IReadOnlyCollection<string> allowedAttachmentTypes,
        LeaveAutoApproveRules autoApproveRules,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Leave type name is required.");
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Leave type code is required.");
        }

        if (defaultDaysPerYear < 0)
        {
            throw new DomainException("Default days per year cannot be negative.");
        }

        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        Description = description.Trim();
        Color = string.IsNullOrWhiteSpace(color) ? "#4CAF50" : color.Trim();
        Icon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim();
        IsPaid = isPaid;
        DefaultDaysPerYear = defaultDaysPerYear;
        MaxConsecutiveDays = maxConsecutiveDays;
        MinDaysPerRequest = minDaysPerRequest;
        MaxDaysPerRequest = maxDaysPerRequest;
        RequiresApproval = requiresApproval;
        ApprovalChainId = approvalChainId;
        CarryForwardEnabled = carryForwardEnabled;
        MaxCarryForwardDays = maxCarryForwardDays;
        CarryForwardExpiryMonths = carryForwardExpiryMonths;
        EncashmentEnabled = encashmentEnabled;
        EncashmentRate = encashmentRate;
        ProrationEnabled = prorationEnabled;
        EligibilityRulesJson = LeaveManagementJson.Serialize(eligibilityRules);
        BlackoutDatesJson = LeaveManagementJson.Serialize(blackoutDates);
        RequiresAttachment = requiresAttachment;
        AllowedAttachmentTypesJson = LeaveManagementJson.Serialize(allowedAttachmentTypes);
        AutoApproveRulesJson = LeaveManagementJson.Serialize(autoApproveRules);
        IsActive = isActive;
    }

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string Color { get; private set; } = "#4CAF50";

    public string? Icon { get; private set; }

    public bool IsPaid { get; private set; }

    public decimal DefaultDaysPerYear { get; private set; }

    public int? MaxConsecutiveDays { get; private set; }

    public decimal MinDaysPerRequest { get; private set; }

    public decimal? MaxDaysPerRequest { get; private set; }

    public bool RequiresApproval { get; private set; }

    public Guid? ApprovalChainId { get; private set; }

    public bool CarryForwardEnabled { get; private set; }

    public decimal MaxCarryForwardDays { get; private set; }

    public int CarryForwardExpiryMonths { get; private set; }

    public bool EncashmentEnabled { get; private set; }

    public decimal? EncashmentRate { get; private set; }

    public bool ProrationEnabled { get; private set; }

    public string EligibilityRulesJson { get; private set; } = "{}";

    public string BlackoutDatesJson { get; private set; } = "[]";

    public bool RequiresAttachment { get; private set; }

    public string AllowedAttachmentTypesJson { get; private set; } = "[]";

    public string AutoApproveRulesJson { get; private set; } = "{}";

    public bool IsActive { get; private set; } = true;

    [NotMapped]
    public LeaveEligibilityRules EligibilityRules => LeaveManagementJson.Deserialize<LeaveEligibilityRules>(EligibilityRulesJson)
        ?? new LeaveEligibilityRules(0, false, [], []);

    [NotMapped]
    public IReadOnlyList<LeaveBlackoutDate> BlackoutDates => LeaveManagementJson.DeserializeList<LeaveBlackoutDate>(BlackoutDatesJson);

    [NotMapped]
    public IReadOnlyList<string> AllowedAttachmentTypes => LeaveManagementJson.DeserializeList<string>(AllowedAttachmentTypesJson);

    [NotMapped]
    public LeaveAutoApproveRules AutoApproveRules => LeaveManagementJson.Deserialize<LeaveAutoApproveRules>(AutoApproveRulesJson)
        ?? new LeaveAutoApproveRules(false, 0, 0);

    /// <summary>
    /// Creates a leave type.
    /// </summary>
    public static LeaveType Create(
        string name,
        string code,
        string description,
        string color,
        string? icon,
        bool isPaid,
        decimal defaultDaysPerYear,
        int? maxConsecutiveDays,
        decimal minDaysPerRequest,
        decimal? maxDaysPerRequest,
        bool requiresApproval,
        Guid? approvalChainId,
        bool carryForwardEnabled,
        decimal maxCarryForwardDays,
        int carryForwardExpiryMonths,
        bool encashmentEnabled,
        decimal? encashmentRate,
        bool prorationEnabled,
        LeaveEligibilityRules eligibilityRules,
        IReadOnlyCollection<LeaveBlackoutDate> blackoutDates,
        bool requiresAttachment,
        IReadOnlyCollection<string> allowedAttachmentTypes,
        LeaveAutoApproveRules autoApproveRules,
        bool isActive)
    {
        return new LeaveType(name, code, description, color, icon, isPaid, defaultDaysPerYear, maxConsecutiveDays, minDaysPerRequest, maxDaysPerRequest, requiresApproval, approvalChainId, carryForwardEnabled, maxCarryForwardDays, carryForwardExpiryMonths, encashmentEnabled, encashmentRate, prorationEnabled, eligibilityRules, blackoutDates, requiresAttachment, allowedAttachmentTypes, autoApproveRules, isActive);
    }

    /// <summary>
    /// Updates leave type settings.
    /// </summary>
    public void Update(
        string name,
        string description,
        string color,
        string? icon,
        bool isPaid,
        decimal defaultDaysPerYear,
        int? maxConsecutiveDays,
        decimal minDaysPerRequest,
        decimal? maxDaysPerRequest,
        bool requiresApproval,
        Guid? approvalChainId,
        bool carryForwardEnabled,
        decimal maxCarryForwardDays,
        int carryForwardExpiryMonths,
        bool encashmentEnabled,
        decimal? encashmentRate,
        bool prorationEnabled,
        LeaveEligibilityRules eligibilityRules,
        IReadOnlyCollection<LeaveBlackoutDate> blackoutDates,
        bool requiresAttachment,
        IReadOnlyCollection<string> allowedAttachmentTypes,
        LeaveAutoApproveRules autoApproveRules,
        bool isActive)
    {
        Name = name.Trim();
        Description = description.Trim();
        Color = color.Trim();
        Icon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim();
        IsPaid = isPaid;
        DefaultDaysPerYear = defaultDaysPerYear;
        MaxConsecutiveDays = maxConsecutiveDays;
        MinDaysPerRequest = minDaysPerRequest;
        MaxDaysPerRequest = maxDaysPerRequest;
        RequiresApproval = requiresApproval;
        ApprovalChainId = approvalChainId;
        CarryForwardEnabled = carryForwardEnabled;
        MaxCarryForwardDays = maxCarryForwardDays;
        CarryForwardExpiryMonths = carryForwardExpiryMonths;
        EncashmentEnabled = encashmentEnabled;
        EncashmentRate = encashmentRate;
        ProrationEnabled = prorationEnabled;
        EligibilityRulesJson = LeaveManagementJson.Serialize(eligibilityRules);
        BlackoutDatesJson = LeaveManagementJson.Serialize(blackoutDates);
        RequiresAttachment = requiresAttachment;
        AllowedAttachmentTypesJson = LeaveManagementJson.Serialize(allowedAttachmentTypes);
        AutoApproveRulesJson = LeaveManagementJson.Serialize(autoApproveRules);
        IsActive = isActive;
        Touch();
    }

    /// <summary>
    /// Deactivates the leave type.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}
