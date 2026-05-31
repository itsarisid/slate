using Alphabet.Domain.Entities;
using Alphabet.Domain.Models;

namespace Alphabet.Application.Common.Interfaces.LeaveManagement;

/// <summary>
/// Calculates leave days using configured calendar rules.
/// </summary>
public interface ILeaveCalendarService
{
    /// <summary>
    /// Calculates leave days for the supplied date range.
    /// </summary>
    Task<decimal> CalculateLeaveDaysAsync(
        DateOnly startDate,
        DateOnly endDate,
        LeavePartialDays partialDays,
        CancellationToken cancellationToken);
}

/// <summary>
/// Resolves approver users for a leave approval level.
/// </summary>
public interface ILeaveApproverResolver
{
    /// <summary>
    /// Resolves an approver for the request and approval level.
    /// </summary>
    Task<LeaveApproverResolution> ResolveAsync(
        LeaveRequest request,
        ApprovalLevelDefinition level,
        CancellationToken cancellationToken);
}

/// <summary>
/// Sends leave-management notifications.
/// </summary>
public interface ILeaveNotificationService
{
    /// <summary>
    /// Notifies approvers that a request needs action.
    /// </summary>
    Task NotifyApproversAsync(LeaveRequest request, ApprovalLevelDefinition level, CancellationToken cancellationToken);

    /// <summary>
    /// Notifies an employee that a leave request status changed.
    /// </summary>
    Task NotifyRequestStatusChangedAsync(LeaveRequest request, string message, CancellationToken cancellationToken);

    /// <summary>
    /// Notifies a delegate that approval authority was assigned.
    /// </summary>
    Task NotifyDelegationCreatedAsync(Delegation delegation, CancellationToken cancellationToken);

    /// <summary>
    /// Notifies an approver that a pending step is overdue.
    /// </summary>
    Task NotifyApprovalOverdueAsync(WorkflowStep step, CancellationToken cancellationToken);
}

/// <summary>
/// Syncs approved leave with external calendars or collaboration tools.
/// </summary>
public interface ILeaveCalendarSyncService
{
    /// <summary>
    /// Syncs an approved leave request.
    /// </summary>
    Task SyncApprovedLeaveAsync(LeaveRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Represents approver resolution output.
/// </summary>
public sealed record LeaveApproverResolution(Guid? UserId, string ApproverValue, IReadOnlyCollection<string> Roles);
