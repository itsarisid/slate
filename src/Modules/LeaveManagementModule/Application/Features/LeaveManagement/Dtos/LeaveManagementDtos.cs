using Alphabet.Domain.Entities;
using Alphabet.Domain.Models;

namespace Alphabet.Application.Features.LeaveManagement.Dtos;

/// <summary>
/// Represents a paged response payload.
/// </summary>
public sealed record LeavePagedResponseDto<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

/// <summary>
/// Represents a leave type.
/// </summary>
public sealed record LeaveTypeDto(
    Guid Id,
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
    bool RequiresAttachment,
    bool IsActive);

/// <summary>
/// Represents leave balance details.
/// </summary>
public sealed record LeaveBalanceDto(
    Guid LeaveTypeId,
    string LeaveType,
    decimal Allocated,
    decimal Taken,
    decimal Pending,
    decimal Approved,
    decimal Remaining,
    decimal CarryForward,
    decimal TotalAvailable);

/// <summary>
/// Represents a leave balance summary.
/// </summary>
public sealed record LeaveBalanceSummaryDto(
    int Year,
    IReadOnlyList<LeaveBalanceDto> Balances,
    decimal TotalAllocated,
    decimal TotalTaken,
    decimal TotalAvailable);

/// <summary>
/// Represents a leave request summary.
/// </summary>
public sealed record LeaveRequestDto(
    Guid Id,
    Guid LeaveTypeId,
    string LeaveTypeName,
    Guid UserId,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal TotalDays,
    string Reason,
    string Status,
    int CurrentApprovalLevel,
    DateTimeOffset AppliedAt,
    string? ContactNumber,
    string? AlternateArrangements);

/// <summary>
/// Represents an approval chain.
/// </summary>
public sealed record ApprovalChainDto(
    Guid Id,
    string Name,
    string Code,
    string Description,
    int FinalApprovalLevel,
    bool AllowSkipLevels,
    bool ParallelApproval,
    bool IsActive,
    IReadOnlyList<ApprovalLevelDefinition> ApprovalLevels);

/// <summary>
/// Represents an approval history row.
/// </summary>
public sealed record ApprovalHistoryDto(
    Guid StepId,
    int Level,
    Guid? ApproverUserId,
    string ApproverType,
    string ApproverValue,
    string Status,
    string? Action,
    string? Comment,
    DateTimeOffset AssignedAt,
    DateTimeOffset? RespondedAt,
    bool IsEscalated);

/// <summary>
/// Represents a pending approval task.
/// </summary>
public sealed record PendingApprovalDto(
    Guid RequestId,
    Guid WorkflowId,
    Guid StepId,
    Guid EmployeeUserId,
    string LeaveType,
    decimal Days,
    DateTimeOffset SubmittedAt,
    DateTimeOffset DueAt,
    bool IsOverdue);

/// <summary>
/// Represents a delegation.
/// </summary>
public sealed record DelegationDto(
    Guid Id,
    Guid DelegatorUserId,
    Guid DelegateToUserId,
    string DelegationType,
    string Permission,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Reason,
    bool IsActive,
    DateTimeOffset? RevokedAt);

/// <summary>
/// Represents a public holiday.
/// </summary>
public sealed record PublicHolidayDto(Guid Id, string Name, DateOnly Date, string Country, string? State, bool IsPaid, bool Recurring);

/// <summary>
/// Represents a leave activity entry.
/// </summary>
public sealed record LeaveActivityDto(
    Guid Id,
    Guid? UserId,
    Guid? LeaveRequestId,
    string Action,
    string? OldValueJson,
    string? NewValueJson,
    DateTimeOffset Timestamp,
    string? IpAddress,
    string? UserAgent,
    string? DetailsJson);

/// <summary>
/// Represents a leave calendar day.
/// </summary>
public sealed record LeaveCalendarDayDto(DateOnly Date, IReadOnlyList<LeaveRequestDto> Leaves, bool CoverageIssues);

/// <summary>
/// Represents a report summary.
/// </summary>
public sealed record LeaveReportSummaryDto(string Title, IReadOnlyDictionary<string, decimal> Metrics, IReadOnlyCollection<string> Highlights);

/// <summary>
/// Represents a mutation result.
/// </summary>
public sealed record LeaveMutationResultDto(Guid Id, string Message);

/// <summary>
/// Maps leave domain objects to DTOs.
/// </summary>
public static class LeaveManagementMappings
{
    public static LeaveTypeDto ToDto(this LeaveType leaveType)
    {
        return new LeaveTypeDto(
            leaveType.Id,
            leaveType.Name,
            leaveType.Code,
            leaveType.Description,
            leaveType.Color,
            leaveType.Icon,
            leaveType.IsPaid,
            leaveType.DefaultDaysPerYear,
            leaveType.MaxConsecutiveDays,
            leaveType.MinDaysPerRequest,
            leaveType.MaxDaysPerRequest,
            leaveType.RequiresApproval,
            leaveType.ApprovalChainId,
            leaveType.CarryForwardEnabled,
            leaveType.MaxCarryForwardDays,
            leaveType.RequiresAttachment,
            leaveType.IsActive);
    }

    public static LeaveRequestDto ToDto(this LeaveRequest request, LeaveType? leaveType)
    {
        return new LeaveRequestDto(
            request.Id,
            request.LeaveTypeId,
            leaveType?.Name ?? "Unknown",
            request.UserId,
            request.StartDate,
            request.EndDate,
            request.TotalDays,
            request.Reason,
            request.Status.ToString(),
            request.CurrentApprovalLevel,
            request.AppliedAt,
            request.ContactNumber,
            request.AlternateArrangements);
    }

    public static LeaveBalanceDto ToDto(this LeaveBalance balance, LeaveType? leaveType)
    {
        return new LeaveBalanceDto(
            balance.LeaveTypeId,
            leaveType?.Name ?? "Unknown",
            balance.Allocated,
            balance.Taken,
            balance.Pending,
            balance.Approved,
            balance.Remaining,
            balance.CarryForward,
            balance.TotalAvailable);
    }

    public static ApprovalChainDto ToDto(this ApprovalChain chain)
    {
        return new ApprovalChainDto(
            chain.Id,
            chain.Name,
            chain.Code,
            chain.Description,
            chain.FinalApprovalLevel,
            chain.AllowSkipLevels,
            chain.ParallelApproval,
            chain.IsActive,
            chain.ApprovalLevels);
    }

    public static ApprovalHistoryDto ToDto(this WorkflowStep step)
    {
        return new ApprovalHistoryDto(
            step.Id,
            step.Level,
            step.ApproverUserId,
            step.ApproverType.ToString(),
            step.ApproverValue,
            step.Status.ToString(),
            step.Action,
            step.Comment,
            step.AssignedAt,
            step.RespondedAt,
            step.IsEscalated);
    }

    public static PendingApprovalDto ToDto(this LeaveApprovalTask task)
    {
        return new PendingApprovalDto(task.RequestId, task.WorkflowId, task.StepId, task.EmployeeUserId, task.LeaveType, task.Days, task.SubmittedAt, task.DueAt, task.IsOverdue);
    }

    public static DelegationDto ToDto(this Delegation delegation)
    {
        return new DelegationDto(
            delegation.Id,
            delegation.DelegatorUserId,
            delegation.DelegateToUserId,
            delegation.DelegationType.ToString(),
            delegation.Permission.ToString(),
            delegation.StartDate,
            delegation.EndDate,
            delegation.Reason,
            delegation.IsActive,
            delegation.RevokedAt);
    }

    public static LeaveActivityDto ToDto(this LeaveActivityLog activity)
    {
        return new LeaveActivityDto(activity.Id, activity.UserId, activity.LeaveRequestId, activity.Action, activity.OldValueJson, activity.NewValueJson, activity.Timestamp, activity.IpAddress, activity.UserAgent, activity.DetailsJson);
    }
}
