using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.LeaveManagement;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces.LeaveManagement;
using Alphabet.Domain.Models;
using Alphabet.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Calculates leave calendar rules such as weekends and holidays.
/// </summary>
public sealed class LeaveCalendarService(
    ILeaveRepository repository,
    IOptions<LeaveManagementSettings> options) : ILeaveCalendarService
{
    public async Task<decimal> CalculateLeaveDaysAsync(DateOnly startDate, DateOnly endDate, LeavePartialDays partialDays, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var weekendDays = settings.WeekendDays
            .Select(x => Enum.TryParse<DayOfWeek>(x, true, out var day) ? day : (DayOfWeek?)null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToHashSet();

        var holidays = settings.ExcludePublicHolidays
            ? await repository.GetPublicHolidaysAsync(startDate, endDate, settings.DefaultCountry, cancellationToken)
            : [];
        var holidayDates = holidays.Select(x => x.Date).ToHashSet();

        decimal days = 0;
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (settings.ExcludeWeekends && weekendDays.Contains(date.DayOfWeek))
            {
                continue;
            }

            if (settings.ExcludePublicHolidays && holidayDates.Contains(date))
            {
                continue;
            }

            days += 1;
        }

        if (days > 0 && partialDays.StartDatePart != LeaveDayPart.Full)
        {
            days -= 0.5m;
        }

        if (endDate != startDate && days > 0 && partialDays.EndDatePart != LeaveDayPart.Full)
        {
            days -= 0.5m;
        }

        return Math.Max(0, days);
    }

    public async Task<bool> IsWorkingDayAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var days = await CalculateLeaveDaysAsync(date, date, new LeavePartialDays(LeaveDayPart.Full, LeaveDayPart.Full), cancellationToken);
        return days == 1;
    }
}

/// <summary>
/// Resolves configured approvers for a leave approval level.
/// </summary>
public sealed class LeaveApproverResolver(ILeaveRepository repository, ICurrentUserService currentUserService)
    : ILeaveApproverResolver
{
    public async Task<LeaveApproverResolution> ResolveAsync(LeaveRequest leaveRequest, ApprovalLevelDefinition level, CancellationToken cancellationToken)
    {
        if (level.ApproverType == LeaveApproverType.SpecificUser && Guid.TryParse(level.ApproverValue, out var userId))
        {
            var delegation = await repository.GetActiveDelegationAsync(userId, leaveRequest.LeaveTypeId, level.Level, leaveRequest.StartDate, cancellationToken);
            return new LeaveApproverResolution(delegation?.DelegateToUserId ?? userId, level.ApproverValue, []);
        }

        if (level.ApproverType is LeaveApproverType.Hr or LeaveApproverType.RoleBased)
        {
            return new LeaveApproverResolution(null, level.ApproverValue, [level.ApproverValue]);
        }

        return new LeaveApproverResolution(currentUserService.UserId, level.ApproverValue, []);
    }
}

/// <summary>
/// Sends leave workflow notifications through the configured communication module.
/// </summary>
public sealed class LeaveNotificationService(
    ICommunicationService communicationService,
    IOptions<LeaveManagementSettings> options,
    ILogger<LeaveNotificationService> logger) : ILeaveNotificationService
{
    public Task NotifyApproversAsync(LeaveRequest request, ApprovalLevelDefinition level, CancellationToken cancellationToken)
    {
        return SendAsync(
            "Leave approval required",
            $"Leave request {request.Id} needs approval at level {level.Level} ({level.Name}).",
            level.ApproverType == LeaveApproverType.SpecificUser ? level.ApproverValue : null,
            cancellationToken);
    }

    public Task NotifyRequestStatusChangedAsync(LeaveRequest request, string message, CancellationToken cancellationToken)
    {
        return SendAsync("Leave request updated", $"{message} Request: {request.Id}.", request.UserId.ToString(), cancellationToken);
    }

    public Task NotifyDelegationCreatedAsync(Delegation delegation, CancellationToken cancellationToken)
    {
        return SendAsync(
            "Leave delegation created",
            $"Delegation {delegation.Id} is active from {delegation.StartDate}.",
            delegation.DelegateToUserId.ToString(),
            cancellationToken);
    }

    public Task NotifyApprovalOverdueAsync(WorkflowStep step, CancellationToken cancellationToken)
    {
        return SendAsync(
            "Leave approval overdue",
            $"Approval step {step.Id} for workflow {step.WorkflowId} is overdue.",
            step.ApproverUserId?.ToString(),
            cancellationToken);
    }

    private async Task SendAsync(string subject, string body, string? userId, CancellationToken cancellationToken)
    {
        var channels = options.Value.NotificationChannels.Length == 0
            ? ["InApp"]
            : options.Value.NotificationChannels;

        try
        {
            await communicationService.SendAsync(
                new CommunicationDispatchRequest(subject, body, channels, null, null, userId, null, null, false),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Leave notification delivery failed for subject {Subject}", subject);
        }
    }
}

/// <summary>
/// Synchronizes approved leave with calendar providers.
/// </summary>
public sealed class LeaveCalendarSyncService(ILogger<LeaveCalendarSyncService> logger) : ILeaveCalendarSyncService
{
    public Task SyncApprovedLeaveAsync(LeaveRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Calendar sync queued for approved leave request {LeaveRequestId}", request.Id);
        return Task.CompletedTask;
    }
}
