using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Interfaces.LeaveManagement;

/// <summary>
/// Provides persistence access for leave management workflows.
/// </summary>
public interface ILeaveRepository
{
    Task<LeaveType?> GetLeaveTypeByIdAsync(Guid leaveTypeId, CancellationToken cancellationToken);

    Task<LeaveType?> GetLeaveTypeByCodeAsync(string code, CancellationToken cancellationToken);

    Task<IReadOnlyList<LeaveType>> GetLeaveTypesAsync(bool? isActive, bool? isPaid, string? applicableToRole, CancellationToken cancellationToken);

    Task AddLeaveTypeAsync(LeaveType leaveType, CancellationToken cancellationToken);

    void UpdateLeaveType(LeaveType leaveType);

    Task<LeaveBalance?> GetBalanceAsync(Guid userId, Guid leaveTypeId, int year, CancellationToken cancellationToken);

    Task<IReadOnlyList<LeaveBalance>> GetBalancesAsync(Guid userId, int year, CancellationToken cancellationToken);

    Task AddBalanceAsync(LeaveBalance balance, CancellationToken cancellationToken);

    void UpdateBalance(LeaveBalance balance);

    Task<LeaveRequest?> GetRequestByIdAsync(Guid requestId, CancellationToken cancellationToken);

    Task<LeavePagedResult<LeaveRequest>> GetRequestsAsync(LeaveRequestFilter filter, CancellationToken cancellationToken);

    Task<bool> HasOverlappingRequestAsync(Guid userId, DateOnly startDate, DateOnly endDate, Guid? excludeRequestId, CancellationToken cancellationToken);

    Task AddRequestAsync(LeaveRequest request, CancellationToken cancellationToken);

    void UpdateRequest(LeaveRequest request);

    Task AddApprovalChainAsync(ApprovalChain chain, CancellationToken cancellationToken);

    Task<ApprovalChain?> GetApprovalChainByIdAsync(Guid chainId, CancellationToken cancellationToken);

    Task<ApprovalChain?> GetApplicableApprovalChainAsync(Guid leaveTypeId, decimal days, CancellationToken cancellationToken);

    Task<IReadOnlyList<ApprovalChain>> GetApprovalChainsAsync(CancellationToken cancellationToken);

    Task AddWorkflowAsync(ApprovalWorkflow workflow, CancellationToken cancellationToken);

    void UpdateWorkflow(ApprovalWorkflow workflow);

    Task<ApprovalWorkflow?> GetWorkflowByRequestIdAsync(Guid requestId, CancellationToken cancellationToken);

    Task<ApprovalWorkflow?> GetWorkflowByIdAsync(Guid workflowId, CancellationToken cancellationToken);

    Task AddWorkflowStepAsync(WorkflowStep step, CancellationToken cancellationToken);

    void UpdateWorkflowStep(WorkflowStep step);

    Task<IReadOnlyList<WorkflowStep>> GetWorkflowStepsAsync(Guid workflowId, CancellationToken cancellationToken);

    Task<WorkflowStep?> GetPendingStepAsync(Guid workflowId, int level, Guid? approverUserId, CancellationToken cancellationToken);

    Task<IReadOnlyList<LeaveApprovalTask>> GetPendingApprovalsAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken);

    Task<IReadOnlyList<WorkflowStep>> GetOverdueStepsAsync(DateTimeOffset now, CancellationToken cancellationToken);

    Task AddDelegationAsync(Delegation delegation, CancellationToken cancellationToken);

    void UpdateDelegation(Delegation delegation);

    Task<Delegation?> GetDelegationByIdAsync(Guid delegationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Delegation>> GetDelegationsForUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<Delegation?> GetActiveDelegationAsync(Guid delegatorUserId, Guid leaveTypeId, int approvalLevel, DateOnly date, CancellationToken cancellationToken);

    Task AddPublicHolidayAsync(PublicHoliday publicHoliday, CancellationToken cancellationToken);

    Task<IReadOnlyList<PublicHoliday>> GetPublicHolidaysAsync(DateOnly fromDate, DateOnly toDate, string? country, CancellationToken cancellationToken);

    Task AddBlackoutPeriodAsync(BlackoutPeriod blackoutPeriod, CancellationToken cancellationToken);

    Task<IReadOnlyList<BlackoutPeriod>> GetBlackoutPeriodsAsync(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken);

    Task AddAccrualRuleAsync(AccrualRule accrualRule, CancellationToken cancellationToken);

    Task<IReadOnlyList<AccrualRule>> GetAccrualRulesAsync(CancellationToken cancellationToken);

    Task AddActivityAsync(LeaveActivityLog activityLog, CancellationToken cancellationToken);

    Task<IReadOnlyList<LeaveActivityLog>> GetActivityAsync(Guid? userId, Guid? requestId, DateTimeOffset? fromDate, DateTimeOffset? toDate, int take, int skip, CancellationToken cancellationToken);
}
