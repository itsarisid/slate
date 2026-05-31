using Alphabet.Domain.Enums;
using Alphabet.Domain.Models;

namespace Alphabet.Modules.LeaveManagementModule.Api.Models;

public sealed record CreateLeaveTypeRequest(
    string Name,
    string Code,
    string Description,
    string Color,
    string? Icon,
    bool IsPaid,
    decimal DefaultDaysPerYear,
    int? MaxConsecutiveDays,
    decimal MinDaysPerRequest,
    decimal? MaxDaysPerRequest,
    bool RequiresApproval,
    Guid? ApprovalChainId,
    bool CarryForwardEnabled,
    decimal MaxCarryForwardDays,
    int CarryForwardExpiryMonths,
    bool EncashmentEnabled,
    decimal? EncashmentRate,
    bool ProrationEnabled,
    LeaveEligibilityRules EligibilityRules,
    IReadOnlyCollection<LeaveBlackoutDate> BlackoutDates,
    bool RequiresAttachment,
    IReadOnlyCollection<string> AllowedAttachmentTypes,
    LeaveAutoApproveRules AutoApproveRules,
    bool IsActive);

public sealed record UpdateLeaveTypeRequest(
    string Name,
    string Description,
    string Color,
    string? Icon,
    bool IsPaid,
    decimal DefaultDaysPerYear,
    int? MaxConsecutiveDays,
    decimal MinDaysPerRequest,
    decimal? MaxDaysPerRequest,
    bool RequiresApproval,
    Guid? ApprovalChainId,
    bool CarryForwardEnabled,
    decimal MaxCarryForwardDays,
    int CarryForwardExpiryMonths,
    bool EncashmentEnabled,
    decimal? EncashmentRate,
    bool ProrationEnabled,
    LeaveEligibilityRules EligibilityRules,
    IReadOnlyCollection<LeaveBlackoutDate> BlackoutDates,
    bool RequiresAttachment,
    IReadOnlyCollection<string> AllowedAttachmentTypes,
    LeaveAutoApproveRules AutoApproveRules,
    bool IsActive);

public sealed record SubmitLeaveRequest(
    Guid LeaveTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    LeavePartialDays PartialDays,
    string Reason,
    IReadOnlyCollection<string> AttachmentIds,
    string? ContactNumber,
    string? AlternateArrangements,
    bool ApplyToAllDays,
    bool IsHalfDay);

public sealed record ModifyLeaveRequest(
    DateOnly StartDate,
    DateOnly EndDate,
    LeavePartialDays PartialDays,
    string Reason,
    IReadOnlyCollection<string> AttachmentIds,
    string? ContactNumber,
    string? AlternateArrangements);

public sealed record CancelLeaveRequest(string Reason);

public sealed record ApprovalDecisionRequest(string? Comment, IReadOnlyCollection<string> Attachments, int? Level, bool NotifyApplicant, bool ApplyPartialApproval, decimal? ApprovedDays);

public sealed record RejectLeaveRequest(string Reason, IReadOnlyCollection<LeaveSuggestedDateRange> SuggestedDates);

public sealed record RequestChangesRequest(string Comment);

public sealed record BatchApprovalRequest(IReadOnlyCollection<Guid> RequestIds, string Action, string Comment);

public sealed record InitializeLeaveBalanceRequest(Guid UserId, int Year, IReadOnlyCollection<InitializeLeaveBalanceRequestItem> Balances);

public sealed record InitializeLeaveBalanceRequestItem(Guid LeaveTypeId, decimal Allocated, decimal Remaining, decimal CarryForward);

public sealed record AdjustLeaveBalanceRequest(Guid UserId, Guid LeaveTypeId, int Year, decimal Adjustment, string Reason);

public sealed record CreateApprovalChainRequest(
    string Name,
    string Code,
    string Description,
    ApprovalChainApplicability ApplicableTo,
    IReadOnlyCollection<ApprovalLevelDefinition> ApprovalLevels,
    int FinalApprovalLevel,
    bool AllowSkipLevels,
    bool ParallelApproval,
    bool IsActive);

public sealed record CreateDelegationRequest(
    Guid DelegatorUserId,
    Guid DelegateToUserId,
    LeaveDelegationType DelegationType,
    LeaveDelegationPermission Permission,
    IReadOnlyCollection<Guid> ApplicableLeaveTypes,
    IReadOnlyCollection<int> ApplicableApprovalLevels,
    IReadOnlyCollection<string> ApplicableDepartments,
    IReadOnlyCollection<Guid> ApplicableEmployees,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Reason,
    bool IsActive);

public sealed record CreatePublicHolidayRequest(string Name, DateOnly Date, string Country, string? State, bool IsPaid, bool Recurring);

public sealed record CreateBlackoutPeriodRequest(DateOnly StartDate, DateOnly EndDate, string Reason, IReadOnlyCollection<string> ApplicableTo);

public sealed record CreateAccrualRuleRequest(Guid LeaveTypeId, LeaveAccrualMethod AccrualMethod, decimal AccrualRate, decimal MaxAccrual, string TenureRulesJson);
