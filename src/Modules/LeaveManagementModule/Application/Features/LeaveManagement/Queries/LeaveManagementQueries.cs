using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.LeaveManagement;
using Alphabet.Application.Features.LeaveManagement.Dtos;
using Alphabet.Application.Features.LeaveManagement.Shared;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces.LeaveManagement;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.LeaveManagement.Queries;

/// <summary>
/// Gets configured leave types.
/// </summary>
public sealed record GetLeaveTypesQuery(bool? IsActive, bool? IsPaid, string? ApplicableToRole)
    : IRequest<Result<IReadOnlyList<LeaveTypeDto>>>;

/// <summary>
/// Gets a single leave request.
/// </summary>
public sealed record GetLeaveRequestByIdQuery(Guid RequestId) : IRequest<Result<LeaveRequestDto>>;

/// <summary>
/// Searches leave requests.
/// </summary>
public sealed record SearchLeaveRequestsQuery(Guid? UserId, LeaveRequestStatus? Status, DateOnly? FromDate, DateOnly? ToDate, int Page, int PageSize)
    : IRequest<Result<LeavePagedResponseDto<LeaveRequestDto>>>;

/// <summary>
/// Gets the current user's leave balances.
/// </summary>
public sealed record GetMyLeaveBalancesQuery(int Year) : IRequest<Result<LeaveBalanceSummaryDto>>;

/// <summary>
/// Gets leave balances for any user.
/// </summary>
public sealed record GetUserLeaveBalancesQuery(Guid UserId, int Year) : IRequest<Result<LeaveBalanceSummaryDto>>;

/// <summary>
/// Gets pending approval tasks for the current user.
/// </summary>
public sealed record GetPendingLeaveApprovalsQuery : IRequest<Result<IReadOnlyList<PendingApprovalDto>>>;

/// <summary>
/// Gets approval history for a request.
/// </summary>
public sealed record GetLeaveApprovalHistoryQuery(Guid RequestId) : IRequest<Result<IReadOnlyList<ApprovalHistoryDto>>>;

/// <summary>
/// Gets approval chains.
/// </summary>
public sealed record GetApprovalChainsQuery : IRequest<Result<IReadOnlyList<ApprovalChainDto>>>;

/// <summary>
/// Gets delegations that involve a user.
/// </summary>
public sealed record GetLeaveDelegationsQuery(Guid UserId) : IRequest<Result<IReadOnlyList<DelegationDto>>>;

/// <summary>
/// Gets public holidays.
/// </summary>
public sealed record GetPublicHolidaysQuery(DateOnly FromDate, DateOnly ToDate, string? Country)
    : IRequest<Result<IReadOnlyList<PublicHolidayDto>>>;

/// <summary>
/// Gets calendar leave coverage.
/// </summary>
public sealed record GetLeaveCalendarQuery(DateOnly FromDate, DateOnly ToDate, Guid? UserId)
    : IRequest<Result<IReadOnlyList<LeaveCalendarDayDto>>>;

/// <summary>
/// Gets leave activity logs.
/// </summary>
public sealed record GetLeaveActivityQuery(Guid? UserId, Guid? RequestId, DateTimeOffset? FromDate, DateTimeOffset? ToDate, int Take, int Skip)
    : IRequest<Result<IReadOnlyList<LeaveActivityDto>>>;

/// <summary>
/// Gets a lightweight leave reporting summary.
/// </summary>
public sealed record GetLeaveReportSummaryQuery(DateOnly FromDate, DateOnly ToDate, Guid? UserId)
    : IRequest<Result<LeaveReportSummaryDto>>;

public sealed class GetLeaveTypesQueryHandler(ILeaveRepository repository)
    : IRequestHandler<GetLeaveTypesQuery, Result<IReadOnlyList<LeaveTypeDto>>>
{
    public async Task<Result<IReadOnlyList<LeaveTypeDto>>> Handle(GetLeaveTypesQuery request, CancellationToken cancellationToken)
    {
        var items = await repository.GetLeaveTypesAsync(request.IsActive, request.IsPaid, request.ApplicableToRole, cancellationToken);
        return Result<IReadOnlyList<LeaveTypeDto>>.Success(items.Select(x => x.ToDto()).ToArray());
    }
}

public sealed class GetLeaveRequestByIdQueryHandler(ILeaveRepository repository)
    : IRequestHandler<GetLeaveRequestByIdQuery, Result<LeaveRequestDto>>
{
    public async Task<Result<LeaveRequestDto>> Handle(GetLeaveRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await repository.GetRequestByIdAsync(request.RequestId, cancellationToken);
        if (item is null)
        {
            return Result<LeaveRequestDto>.Failure("Leave request was not found.");
        }

        var leaveType = await repository.GetLeaveTypeByIdAsync(item.LeaveTypeId, cancellationToken);
        return Result<LeaveRequestDto>.Success(item.ToDto(leaveType));
    }
}

public sealed class SearchLeaveRequestsQueryHandler(ILeaveRepository repository, ICurrentUserService currentUserService)
    : IRequestHandler<SearchLeaveRequestsQuery, Result<LeavePagedResponseDto<LeaveRequestDto>>>
{
    public async Task<Result<LeavePagedResponseDto<LeaveRequestDto>>> Handle(SearchLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var filterUserId = request.UserId ?? LeaveManagementHandlerHelpers.RequireUserId(currentUserService);
        var result = await repository.GetRequestsAsync(
            new LeaveRequestFilter(filterUserId, request.Status, request.FromDate, request.ToDate, Math.Max(1, request.Page), Math.Clamp(request.PageSize, 1, 200)),
            cancellationToken);

        var leaveTypes = await repository.GetLeaveTypesAsync(null, null, null, cancellationToken);
        var byId = leaveTypes.ToDictionary(x => x.Id);
        var items = result.Items
            .Select(x => x.ToDto(byId.GetValueOrDefault(x.LeaveTypeId)))
            .ToArray();

        return Result<LeavePagedResponseDto<LeaveRequestDto>>.Success(new LeavePagedResponseDto<LeaveRequestDto>(items, result.Page, result.PageSize, result.TotalCount));
    }
}

public sealed class GetMyLeaveBalancesQueryHandler(ILeaveRepository repository, ICurrentUserService currentUserService)
    : IRequestHandler<GetMyLeaveBalancesQuery, Result<LeaveBalanceSummaryDto>>
{
    public Task<Result<LeaveBalanceSummaryDto>> Handle(GetMyLeaveBalancesQuery request, CancellationToken cancellationToken)
    {
        var userId = LeaveManagementHandlerHelpers.RequireUserId(currentUserService);
        return BuildSummaryAsync(repository, userId, request.Year, cancellationToken);
    }

    internal static async Task<Result<LeaveBalanceSummaryDto>> BuildSummaryAsync(ILeaveRepository repository, Guid userId, int year, CancellationToken cancellationToken)
    {
        var balances = await repository.GetBalancesAsync(userId, year, cancellationToken);
        var leaveTypes = await repository.GetLeaveTypesAsync(null, null, null, cancellationToken);
        var byId = leaveTypes.ToDictionary(x => x.Id);
        var items = balances.Select(x => x.ToDto(byId.GetValueOrDefault(x.LeaveTypeId))).ToArray();
        return Result<LeaveBalanceSummaryDto>.Success(new LeaveBalanceSummaryDto(
            year,
            items,
            items.Sum(x => x.Allocated),
            items.Sum(x => x.Taken),
            items.Sum(x => x.TotalAvailable)));
    }
}

public sealed class GetUserLeaveBalancesQueryHandler(ILeaveRepository repository)
    : IRequestHandler<GetUserLeaveBalancesQuery, Result<LeaveBalanceSummaryDto>>
{
    public Task<Result<LeaveBalanceSummaryDto>> Handle(GetUserLeaveBalancesQuery request, CancellationToken cancellationToken)
        => GetMyLeaveBalancesQueryHandler.BuildSummaryAsync(repository, request.UserId, request.Year, cancellationToken);
}

public sealed class GetPendingLeaveApprovalsQueryHandler(ILeaveRepository repository, ICurrentUserService currentUserService)
    : IRequestHandler<GetPendingLeaveApprovalsQuery, Result<IReadOnlyList<PendingApprovalDto>>>
{
    public async Task<Result<IReadOnlyList<PendingApprovalDto>>> Handle(GetPendingLeaveApprovalsQuery request, CancellationToken cancellationToken)
    {
        var userId = LeaveManagementHandlerHelpers.RequireUserId(currentUserService);
        var tasks = await repository.GetPendingApprovalsAsync(userId, currentUserService.Roles, cancellationToken);
        return Result<IReadOnlyList<PendingApprovalDto>>.Success(tasks.Select(x => x.ToDto()).ToArray());
    }
}

public sealed class GetLeaveApprovalHistoryQueryHandler(ILeaveRepository repository)
    : IRequestHandler<GetLeaveApprovalHistoryQuery, Result<IReadOnlyList<ApprovalHistoryDto>>>
{
    public async Task<Result<IReadOnlyList<ApprovalHistoryDto>>> Handle(GetLeaveApprovalHistoryQuery request, CancellationToken cancellationToken)
    {
        var workflow = await repository.GetWorkflowByRequestIdAsync(request.RequestId, cancellationToken);
        if (workflow is null)
        {
            return Result<IReadOnlyList<ApprovalHistoryDto>>.Success([]);
        }

        var steps = await repository.GetWorkflowStepsAsync(workflow.Id, cancellationToken);
        return Result<IReadOnlyList<ApprovalHistoryDto>>.Success(steps.Select(x => x.ToDto()).ToArray());
    }
}

public sealed class GetApprovalChainsQueryHandler(ILeaveRepository repository)
    : IRequestHandler<GetApprovalChainsQuery, Result<IReadOnlyList<ApprovalChainDto>>>
{
    public async Task<Result<IReadOnlyList<ApprovalChainDto>>> Handle(GetApprovalChainsQuery request, CancellationToken cancellationToken)
    {
        var chains = await repository.GetApprovalChainsAsync(cancellationToken);
        return Result<IReadOnlyList<ApprovalChainDto>>.Success(chains.Select(x => x.ToDto()).ToArray());
    }
}

public sealed class GetLeaveDelegationsQueryHandler(ILeaveRepository repository)
    : IRequestHandler<GetLeaveDelegationsQuery, Result<IReadOnlyList<DelegationDto>>>
{
    public async Task<Result<IReadOnlyList<DelegationDto>>> Handle(GetLeaveDelegationsQuery request, CancellationToken cancellationToken)
    {
        var delegations = await repository.GetDelegationsForUserAsync(request.UserId, cancellationToken);
        return Result<IReadOnlyList<DelegationDto>>.Success(delegations.Select(x => x.ToDto()).ToArray());
    }
}

public sealed class GetPublicHolidaysQueryHandler(ILeaveRepository repository)
    : IRequestHandler<GetPublicHolidaysQuery, Result<IReadOnlyList<PublicHolidayDto>>>
{
    public async Task<Result<IReadOnlyList<PublicHolidayDto>>> Handle(GetPublicHolidaysQuery request, CancellationToken cancellationToken)
    {
        var holidays = await repository.GetPublicHolidaysAsync(request.FromDate, request.ToDate, request.Country, cancellationToken);
        return Result<IReadOnlyList<PublicHolidayDto>>.Success(holidays.Select(x => new PublicHolidayDto(x.Id, x.Name, x.Date, x.Country, x.State, x.IsPaid, x.Recurring)).ToArray());
    }
}

public sealed class GetLeaveCalendarQueryHandler(ILeaveRepository repository)
    : IRequestHandler<GetLeaveCalendarQuery, Result<IReadOnlyList<LeaveCalendarDayDto>>>
{
    public async Task<Result<IReadOnlyList<LeaveCalendarDayDto>>> Handle(GetLeaveCalendarQuery request, CancellationToken cancellationToken)
    {
        var result = await repository.GetRequestsAsync(new LeaveRequestFilter(request.UserId, LeaveRequestStatus.Approved, request.FromDate, request.ToDate, 1, 500), cancellationToken);
        var leaveTypes = await repository.GetLeaveTypesAsync(null, null, null, cancellationToken);
        var byId = leaveTypes.ToDictionary(x => x.Id);
        var days = new List<LeaveCalendarDayDto>();

        for (var date = request.FromDate; date <= request.ToDate; date = date.AddDays(1))
        {
            var leaves = result.Items
                .Where(x => x.StartDate <= date && x.EndDate >= date)
                .Select(x => x.ToDto(byId.GetValueOrDefault(x.LeaveTypeId)))
                .ToArray();
            days.Add(new LeaveCalendarDayDto(date, leaves, leaves.Length > 3));
        }

        return Result<IReadOnlyList<LeaveCalendarDayDto>>.Success(days);
    }
}

public sealed class GetLeaveActivityQueryHandler(ILeaveRepository repository)
    : IRequestHandler<GetLeaveActivityQuery, Result<IReadOnlyList<LeaveActivityDto>>>
{
    public async Task<Result<IReadOnlyList<LeaveActivityDto>>> Handle(GetLeaveActivityQuery request, CancellationToken cancellationToken)
    {
        var activities = await repository.GetActivityAsync(request.UserId, request.RequestId, request.FromDate, request.ToDate, Math.Clamp(request.Take, 1, 500), Math.Max(0, request.Skip), cancellationToken);
        return Result<IReadOnlyList<LeaveActivityDto>>.Success(activities.Select(x => x.ToDto()).ToArray());
    }
}

public sealed class GetLeaveReportSummaryQueryHandler(ILeaveRepository repository)
    : IRequestHandler<GetLeaveReportSummaryQuery, Result<LeaveReportSummaryDto>>
{
    public async Task<Result<LeaveReportSummaryDto>> Handle(GetLeaveReportSummaryQuery request, CancellationToken cancellationToken)
    {
        var result = await repository.GetRequestsAsync(new LeaveRequestFilter(request.UserId, null, request.FromDate, request.ToDate, 1, 1000), cancellationToken);
        var metrics = new Dictionary<string, decimal>
        {
            ["totalRequests"] = result.TotalCount,
            ["approvedRequests"] = result.Items.Count(x => x.Status == LeaveRequestStatus.Approved),
            ["pendingRequests"] = result.Items.Count(x => x.Status == LeaveRequestStatus.Pending),
            ["rejectedRequests"] = result.Items.Count(x => x.Status == LeaveRequestStatus.Rejected),
            ["totalApprovedDays"] = result.Items.Where(x => x.Status == LeaveRequestStatus.Approved).Sum(x => x.TotalDays)
        };

        return Result<LeaveReportSummaryDto>.Success(new LeaveReportSummaryDto(
            "Leave Management Summary",
            metrics,
            ["Use this report for team capacity planning and policy compliance checks."]));
    }
}
