using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces.LeaveManagement;
using Alphabet.Domain.Models;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for leave management workflows.
/// </summary>
public sealed class LeaveRepository(AppDbContext dbContext) : ILeaveRepository
{
    public Task<LeaveType?> GetLeaveTypeByIdAsync(Guid leaveTypeId, CancellationToken cancellationToken)
        => dbContext.Set<LeaveType>().FirstOrDefaultAsync(x => x.Id == leaveTypeId, cancellationToken);

    public Task<LeaveType?> GetLeaveTypeByCodeAsync(string code, CancellationToken cancellationToken)
        => dbContext.Set<LeaveType>().FirstOrDefaultAsync(x => x.Code == code.Trim().ToUpper(), cancellationToken);

    public async Task<IReadOnlyList<LeaveType>> GetLeaveTypesAsync(bool? isActive, bool? isPaid, string? applicableToRole, CancellationToken cancellationToken)
    {
        var query = dbContext.Set<LeaveType>().AsNoTracking().AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (isPaid.HasValue)
        {
            query = query.Where(x => x.IsPaid == isPaid.Value);
        }

        var items = await query.OrderBy(x => x.Name).ToArrayAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(applicableToRole))
        {
            return items;
        }

        return items
            .Where(x => x.EligibilityRules.ApplicableRoles.Count == 0 || x.EligibilityRules.ApplicableRoles.Contains(applicableToRole, StringComparer.OrdinalIgnoreCase))
            .Where(x => !x.EligibilityRules.ExcludedRoles.Contains(applicableToRole, StringComparer.OrdinalIgnoreCase))
            .ToArray();
    }

    public async Task AddLeaveTypeAsync(LeaveType leaveType, CancellationToken cancellationToken)
        => await dbContext.Set<LeaveType>().AddAsync(leaveType, cancellationToken);

    public void UpdateLeaveType(LeaveType leaveType) => dbContext.Set<LeaveType>().Update(leaveType);

    public Task<LeaveBalance?> GetBalanceAsync(Guid userId, Guid leaveTypeId, int year, CancellationToken cancellationToken)
        => dbContext.Set<LeaveBalance>().FirstOrDefaultAsync(x => x.UserId == userId && x.LeaveTypeId == leaveTypeId && x.Year == year, cancellationToken);

    public async Task<IReadOnlyList<LeaveBalance>> GetBalancesAsync(Guid userId, int year, CancellationToken cancellationToken)
        => await dbContext.Set<LeaveBalance>().AsNoTracking().Where(x => x.UserId == userId && x.Year == year).OrderBy(x => x.CreatedAt).ToArrayAsync(cancellationToken);

    public async Task AddBalanceAsync(LeaveBalance balance, CancellationToken cancellationToken)
        => await dbContext.Set<LeaveBalance>().AddAsync(balance, cancellationToken);

    public void UpdateBalance(LeaveBalance balance) => dbContext.Set<LeaveBalance>().Update(balance);

    public Task<LeaveRequest?> GetRequestByIdAsync(Guid requestId, CancellationToken cancellationToken)
        => dbContext.Set<LeaveRequest>().FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);

    public async Task<LeavePagedResult<LeaveRequest>> GetRequestsAsync(LeaveRequestFilter filter, CancellationToken cancellationToken)
    {
        var query = dbContext.Set<LeaveRequest>().AsNoTracking().AsQueryable();

        if (filter.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == filter.UserId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(x => x.EndDate >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(x => x.StartDate <= filter.ToDate.Value);
        }

        query = query.OrderByDescending(x => x.AppliedAt);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToArrayAsync(cancellationToken);

        return new LeavePagedResult<LeaveRequest>(items, filter.Page, filter.PageSize, total);
    }

    public Task<bool> HasOverlappingRequestAsync(Guid userId, DateOnly startDate, DateOnly endDate, Guid? excludeRequestId, CancellationToken cancellationToken)
    {
        var query = dbContext.Set<LeaveRequest>()
            .Where(x => x.UserId == userId &&
                x.Status != LeaveRequestStatus.Cancelled &&
                x.Status != LeaveRequestStatus.Rejected &&
                x.Status != LeaveRequestStatus.Withdrawn &&
                x.StartDate <= endDate &&
                x.EndDate >= startDate);

        if (excludeRequestId.HasValue)
        {
            query = query.Where(x => x.Id != excludeRequestId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddRequestAsync(LeaveRequest request, CancellationToken cancellationToken)
        => await dbContext.Set<LeaveRequest>().AddAsync(request, cancellationToken);

    public void UpdateRequest(LeaveRequest request) => dbContext.Set<LeaveRequest>().Update(request);

    public async Task AddApprovalChainAsync(ApprovalChain chain, CancellationToken cancellationToken)
        => await dbContext.Set<ApprovalChain>().AddAsync(chain, cancellationToken);

    public Task<ApprovalChain?> GetApprovalChainByIdAsync(Guid chainId, CancellationToken cancellationToken)
        => dbContext.Set<ApprovalChain>().FirstOrDefaultAsync(x => x.Id == chainId, cancellationToken);

    public async Task<ApprovalChain?> GetApplicableApprovalChainAsync(Guid leaveTypeId, decimal days, CancellationToken cancellationToken)
    {
        var chains = await dbContext.Set<ApprovalChain>().Where(x => x.IsActive).ToArrayAsync(cancellationToken);
        return chains.FirstOrDefault(x =>
            (x.ApplicableTo.LeaveTypeIds.Count == 0 || x.ApplicableTo.LeaveTypeIds.Contains(leaveTypeId)) &&
            days >= x.ApplicableTo.MinDays &&
            (!x.ApplicableTo.MaxDays.HasValue || days <= x.ApplicableTo.MaxDays.Value));
    }

    public async Task<IReadOnlyList<ApprovalChain>> GetApprovalChainsAsync(CancellationToken cancellationToken)
        => await dbContext.Set<ApprovalChain>().AsNoTracking().OrderBy(x => x.Name).ToArrayAsync(cancellationToken);

    public async Task AddWorkflowAsync(ApprovalWorkflow workflow, CancellationToken cancellationToken)
        => await dbContext.Set<ApprovalWorkflow>().AddAsync(workflow, cancellationToken);

    public void UpdateWorkflow(ApprovalWorkflow workflow) => dbContext.Set<ApprovalWorkflow>().Update(workflow);

    public Task<ApprovalWorkflow?> GetWorkflowByRequestIdAsync(Guid requestId, CancellationToken cancellationToken)
        => dbContext.Set<ApprovalWorkflow>().FirstOrDefaultAsync(x => x.LeaveRequestId == requestId, cancellationToken);

    public Task<ApprovalWorkflow?> GetWorkflowByIdAsync(Guid workflowId, CancellationToken cancellationToken)
        => dbContext.Set<ApprovalWorkflow>().FirstOrDefaultAsync(x => x.Id == workflowId, cancellationToken);

    public async Task AddWorkflowStepAsync(WorkflowStep step, CancellationToken cancellationToken)
        => await dbContext.Set<WorkflowStep>().AddAsync(step, cancellationToken);

    public void UpdateWorkflowStep(WorkflowStep step) => dbContext.Set<WorkflowStep>().Update(step);

    public async Task<IReadOnlyList<WorkflowStep>> GetWorkflowStepsAsync(Guid workflowId, CancellationToken cancellationToken)
        => await dbContext.Set<WorkflowStep>().AsNoTracking().Where(x => x.WorkflowId == workflowId).OrderBy(x => x.Level).ThenBy(x => x.AssignedAt).ToArrayAsync(cancellationToken);

    public Task<WorkflowStep?> GetPendingStepAsync(Guid workflowId, int level, Guid? approverUserId, CancellationToken cancellationToken)
    {
        var query = dbContext.Set<WorkflowStep>().Where(x => x.WorkflowId == workflowId && x.Level == level && x.Status == LeaveWorkflowStepStatus.Pending);
        if (approverUserId.HasValue)
        {
            query = query.Where(x => x.ApproverUserId == approverUserId.Value);
        }

        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LeaveApprovalTask>> GetPendingApprovalsAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var roleSet = roles.Select(x => x.ToLowerInvariant()).ToHashSet();
        var query =
            from step in dbContext.Set<WorkflowStep>().AsNoTracking()
            join workflow in dbContext.Set<ApprovalWorkflow>().AsNoTracking() on step.WorkflowId equals workflow.Id
            join request in dbContext.Set<LeaveRequest>().AsNoTracking() on workflow.LeaveRequestId equals request.Id
            join type in dbContext.Set<LeaveType>().AsNoTracking() on request.LeaveTypeId equals type.Id
            where step.Status == LeaveWorkflowStepStatus.Pending && workflow.Status == LeaveWorkflowStatus.Active
            select new { step, workflow, request, type };

        var rows = await query.ToArrayAsync(cancellationToken);
        return rows
            .Where(x => x.step.ApproverUserId == userId ||
                (x.step.ApproverType == LeaveApproverType.RoleBased && roleSet.Contains(x.step.ApproverValue.ToLowerInvariant())) ||
                x.step.ApproverType == LeaveApproverType.Hr)
            .Select(x => new LeaveApprovalTask(
                x.request.Id,
                x.workflow.Id,
                x.step.Id,
                x.request.UserId,
                x.type.Name,
                x.request.TotalDays,
                x.request.AppliedAt,
                x.step.AssignedAt.AddHours(x.step.TimeoutHours),
                x.step.AssignedAt.AddHours(x.step.TimeoutHours) < now))
            .ToArray();
    }

    public async Task<IReadOnlyList<WorkflowStep>> GetOverdueStepsAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var pending = await dbContext.Set<WorkflowStep>()
            .Where(x => x.Status == LeaveWorkflowStepStatus.Pending)
            .ToArrayAsync(cancellationToken);

        return pending.Where(x => x.AssignedAt.AddHours(x.TimeoutHours) < now).ToArray();
    }

    public async Task AddDelegationAsync(Delegation delegation, CancellationToken cancellationToken)
        => await dbContext.Set<Delegation>().AddAsync(delegation, cancellationToken);

    public void UpdateDelegation(Delegation delegation) => dbContext.Set<Delegation>().Update(delegation);

    public Task<Delegation?> GetDelegationByIdAsync(Guid delegationId, CancellationToken cancellationToken)
        => dbContext.Set<Delegation>().FirstOrDefaultAsync(x => x.Id == delegationId, cancellationToken);

    public async Task<IReadOnlyList<Delegation>> GetDelegationsForUserAsync(Guid userId, CancellationToken cancellationToken)
        => await dbContext.Set<Delegation>().AsNoTracking().Where(x => x.DelegatorUserId == userId || x.DelegateToUserId == userId).OrderByDescending(x => x.CreatedAt).ToArrayAsync(cancellationToken);

    public async Task<Delegation?> GetActiveDelegationAsync(Guid delegatorUserId, Guid leaveTypeId, int approvalLevel, DateOnly date, CancellationToken cancellationToken)
    {
        var items = await dbContext.Set<Delegation>().Where(x => x.DelegatorUserId == delegatorUserId && x.IsActive).ToArrayAsync(cancellationToken);
        return items.FirstOrDefault(x => x.IsEffective(date) &&
            (x.ApplicableLeaveTypes.Count == 0 || x.ApplicableLeaveTypes.Contains(leaveTypeId)) &&
            (x.ApplicableApprovalLevels.Count == 0 || x.ApplicableApprovalLevels.Contains(approvalLevel)));
    }

    public async Task AddPublicHolidayAsync(PublicHoliday publicHoliday, CancellationToken cancellationToken)
        => await dbContext.Set<PublicHoliday>().AddAsync(publicHoliday, cancellationToken);

    public async Task<IReadOnlyList<PublicHoliday>> GetPublicHolidaysAsync(DateOnly fromDate, DateOnly toDate, string? country, CancellationToken cancellationToken)
    {
        var query = dbContext.Set<PublicHoliday>().AsNoTracking().Where(x => x.Date >= fromDate && x.Date <= toDate);
        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(x => x.Country == country.Trim().ToUpper());
        }

        return await query.OrderBy(x => x.Date).ToArrayAsync(cancellationToken);
    }

    public async Task AddBlackoutPeriodAsync(BlackoutPeriod blackoutPeriod, CancellationToken cancellationToken)
        => await dbContext.Set<BlackoutPeriod>().AddAsync(blackoutPeriod, cancellationToken);

    public async Task<IReadOnlyList<BlackoutPeriod>> GetBlackoutPeriodsAsync(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
        => await dbContext.Set<BlackoutPeriod>().AsNoTracking().Where(x => x.StartDate <= toDate && x.EndDate >= fromDate).ToArrayAsync(cancellationToken);

    public async Task AddAccrualRuleAsync(AccrualRule accrualRule, CancellationToken cancellationToken)
        => await dbContext.Set<AccrualRule>().AddAsync(accrualRule, cancellationToken);

    public async Task<IReadOnlyList<AccrualRule>> GetAccrualRulesAsync(CancellationToken cancellationToken)
        => await dbContext.Set<AccrualRule>().AsNoTracking().Where(x => x.IsActive).ToArrayAsync(cancellationToken);

    public async Task AddActivityAsync(LeaveActivityLog activityLog, CancellationToken cancellationToken)
        => await dbContext.Set<LeaveActivityLog>().AddAsync(activityLog, cancellationToken);

    public async Task<IReadOnlyList<LeaveActivityLog>> GetActivityAsync(Guid? userId, Guid? requestId, DateTimeOffset? fromDate, DateTimeOffset? toDate, int take, int skip, CancellationToken cancellationToken)
    {
        var query = dbContext.Set<LeaveActivityLog>().AsNoTracking().AsQueryable();
        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        if (requestId.HasValue)
        {
            query = query.Where(x => x.LeaveRequestId == requestId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.Timestamp <= toDate.Value);
        }

        return await query.OrderByDescending(x => x.Timestamp).Skip(skip).Take(take).ToArrayAsync(cancellationToken);
    }
}
