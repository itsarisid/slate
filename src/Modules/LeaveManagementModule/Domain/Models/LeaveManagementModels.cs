using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Models;

/// <summary>
/// Represents leave-type eligibility rules.
/// </summary>
public sealed record LeaveEligibilityRules(
    int MinEmploymentDays,
    bool ProbationPassed,
    IReadOnlyCollection<string> ApplicableRoles,
    IReadOnlyCollection<string> ExcludedRoles);

/// <summary>
/// Represents a blackout range.
/// </summary>
public sealed record LeaveBlackoutDate(DateOnly Start, DateOnly End, string Reason);

/// <summary>
/// Represents leave auto-approval rules.
/// </summary>
public sealed record LeaveAutoApproveRules(bool Enabled, decimal MaxDays, int LeadTimeDays);

/// <summary>
/// Represents leave partial-day selection.
/// </summary>
public sealed record LeavePartialDays(LeaveDayPart StartDatePart, LeaveDayPart EndDatePart);

/// <summary>
/// Represents approval-chain applicability rules.
/// </summary>
public sealed record ApprovalChainApplicability(
    IReadOnlyCollection<Guid> LeaveTypeIds,
    IReadOnlyCollection<string> DepartmentIds,
    IReadOnlyCollection<string> EmployeeLevels,
    IReadOnlyCollection<string> GeographyIds,
    decimal MinDays,
    decimal? MaxDays);

/// <summary>
/// Represents an approval-chain level definition.
/// </summary>
public sealed record ApprovalLevelDefinition(
    int Level,
    string Name,
    LeaveApproverType ApproverType,
    string ApproverValue,
    int RequiredApprovers,
    int TimeoutHours,
    bool EscalationEnabled,
    int? EscalationAfterHours,
    string? EscalationToRole,
    bool AutoApproveOnTimeout,
    bool CanDelegate,
    IReadOnlyDictionary<string, string> Conditions);

/// <summary>
/// Represents leave request filtering.
/// </summary>
public sealed record LeaveRequestFilter(
    Guid? UserId,
    LeaveRequestStatus? Status,
    DateOnly? FromDate,
    DateOnly? ToDate,
    int Page,
    int PageSize);

/// <summary>
/// Represents a paged leave query result.
/// </summary>
public sealed record LeavePagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

/// <summary>
/// Represents a dated range suggestion.
/// </summary>
public sealed record LeaveSuggestedDateRange(DateOnly Start, DateOnly End);

/// <summary>
/// Represents an active approval task.
/// </summary>
public sealed record LeaveApprovalTask(
    Guid RequestId,
    Guid WorkflowId,
    Guid StepId,
    Guid EmployeeUserId,
    string LeaveType,
    decimal Days,
    DateTimeOffset SubmittedAt,
    DateTimeOffset DueAt,
    bool IsOverdue);
