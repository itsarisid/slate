using Alphabet.Application.Common.Interfaces.LeaveManagement;
using Alphabet.Application.Features.LeaveManagement.Dtos;
using Alphabet.Application.Features.LeaveManagement.Shared;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.LeaveManagement;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.LeaveManagement.Commands;

/// <summary>
/// Submits a leave request.
/// </summary>
public sealed record SubmitLeaveRequestCommand(
    Guid LeaveTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    LeavePartialDays PartialDays,
    string Reason,
    IReadOnlyCollection<string> AttachmentIds,
    string? ContactNumber,
    string? AlternateArrangements,
    bool ApplyToAllDays,
    bool IsHalfDay) : IRequest<Result<LeaveRequestDto>>;

/// <summary>
/// Modifies a pending leave request.
/// </summary>
public sealed record ModifyLeaveRequestCommand(
    Guid RequestId,
    DateOnly StartDate,
    DateOnly EndDate,
    LeavePartialDays PartialDays,
    string Reason,
    IReadOnlyCollection<string> AttachmentIds,
    string? ContactNumber,
    string? AlternateArrangements) : IRequest<Result<LeaveRequestDto>>;

/// <summary>
/// Cancels a leave request.
/// </summary>
public sealed record CancelLeaveRequestCommand(Guid RequestId, string Reason) : IRequest<Result<LeaveMutationResultDto>>;

/// <summary>
/// Withdraws a pending leave request.
/// </summary>
public sealed record WithdrawLeaveRequestCommand(Guid RequestId) : IRequest<Result<LeaveMutationResultDto>>;

/// <summary>
/// Resubmits a changed leave request.
/// </summary>
public sealed record ResubmitLeaveRequestCommand(Guid RequestId) : IRequest<Result<LeaveMutationResultDto>>;

/// <summary>
/// Handles leave request submission.
/// </summary>
public sealed class SubmitLeaveRequestCommandHandler(
    ILeaveRepository repository,
    ILeaveCalendarService calendarService,
    ILeaveApproverResolver approverResolver,
    ILeaveNotificationService notificationService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SubmitLeaveRequestCommand, Result<LeaveRequestDto>>
{
    public async Task<Result<LeaveRequestDto>> Handle(SubmitLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = LeaveManagementHandlerHelpers.RequireUserId(currentUserService);
        var leaveType = await repository.GetLeaveTypeByIdAsync(request.LeaveTypeId, cancellationToken);
        if (leaveType is null || !leaveType.IsActive)
        {
            return Result<LeaveRequestDto>.Failure("Leave type was not found or is inactive.");
        }

        if (await repository.HasOverlappingRequestAsync(currentUserId, request.StartDate, request.EndDate, null, cancellationToken))
        {
            return Result<LeaveRequestDto>.Failure("An overlapping leave request already exists.");
        }

        if (leaveType.BlackoutDates.Any(x => request.StartDate <= x.End && request.EndDate >= x.Start))
        {
            return Result<LeaveRequestDto>.Failure("The selected dates overlap a blackout period.");
        }

        var blackoutPeriods = await repository.GetBlackoutPeriodsAsync(request.StartDate, request.EndDate, cancellationToken);
        if (blackoutPeriods.Any(x => x.IsActive))
        {
            return Result<LeaveRequestDto>.Failure("The selected dates overlap a configured blackout period.");
        }

        var totalDays = await calendarService.CalculateLeaveDaysAsync(request.StartDate, request.EndDate, request.PartialDays, cancellationToken);
        if (leaveType.MaxConsecutiveDays.HasValue && totalDays > leaveType.MaxConsecutiveDays.Value)
        {
            return Result<LeaveRequestDto>.Failure($"This leave type allows a maximum of {leaveType.MaxConsecutiveDays.Value} consecutive days.");
        }

        if (totalDays < leaveType.MinDaysPerRequest || (leaveType.MaxDaysPerRequest.HasValue && totalDays > leaveType.MaxDaysPerRequest.Value))
        {
            return Result<LeaveRequestDto>.Failure("Requested days are outside the allowed range for this leave type.");
        }

        var balance = await repository.GetBalanceAsync(currentUserId, request.LeaveTypeId, request.StartDate.Year, cancellationToken);
        if (balance is null)
        {
            return Result<LeaveRequestDto>.Failure("Leave balance was not initialized for this leave type and year.");
        }

        try
        {
            balance.Reserve(totalDays);
        }
        catch (Exception ex)
        {
            return Result<LeaveRequestDto>.Failure(ex.Message);
        }

        var leaveRequest = LeaveRequest.Create(request.LeaveTypeId, currentUserId, request.StartDate, request.EndDate, request.PartialDays, totalDays, request.Reason, request.AttachmentIds, request.ContactNumber, request.AlternateArrangements, request.ApplyToAllDays, request.IsHalfDay);
        await repository.AddRequestAsync(leaveRequest, cancellationToken);
        repository.UpdateBalance(balance);

        var chain = await repository.GetApplicableApprovalChainAsync(request.LeaveTypeId, totalDays, cancellationToken);
        if (chain is not null && leaveType.RequiresApproval && !leaveType.AutoApproveRules.Enabled)
        {
            var workflow = ApprovalWorkflow.Create(leaveRequest.Id, chain.Id);
            await repository.AddWorkflowAsync(workflow, cancellationToken);
            var firstLevel = chain.ApprovalLevels.OrderBy(x => x.Level).First();
            var approver = await approverResolver.ResolveAsync(leaveRequest, firstLevel, cancellationToken);
            var step = WorkflowStep.Create(workflow.Id, firstLevel.Level, approver.UserId, firstLevel.ApproverType, firstLevel.ApproverValue, firstLevel.TimeoutHours);
            await repository.AddWorkflowStepAsync(step, cancellationToken);
            await notificationService.NotifyApproversAsync(leaveRequest, firstLevel, cancellationToken);
        }
        else
        {
            leaveRequest.Approve();
            balance.Approve(totalDays);
            repository.UpdateBalance(balance);
            await notificationService.NotifyRequestStatusChangedAsync(leaveRequest, "Leave request auto-approved.", cancellationToken);
        }

        await LeaveManagementHandlerHelpers.AddActivityAsync(repository, currentUserService, leaveRequest.Id, "Submit", null, LeaveManagementJson.Serialize(leaveRequest.ToDto(leaveType)), request.Reason, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveRequestDto>.Success(leaveRequest.ToDto(leaveType));
    }
}

/// <summary>
/// Handles leave request modification.
/// </summary>
public sealed class ModifyLeaveRequestCommandHandler(
    ILeaveRepository repository,
    ILeaveCalendarService calendarService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ModifyLeaveRequestCommand, Result<LeaveRequestDto>>
{
    public async Task<Result<LeaveRequestDto>> Handle(ModifyLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.GetRequestByIdAsync(request.RequestId, cancellationToken);
        if (leaveRequest is null)
        {
            return Result<LeaveRequestDto>.Failure("Leave request was not found.");
        }

        var leaveType = await repository.GetLeaveTypeByIdAsync(leaveRequest.LeaveTypeId, cancellationToken);
        var totalDays = await calendarService.CalculateLeaveDaysAsync(request.StartDate, request.EndDate, request.PartialDays, cancellationToken);
        var before = LeaveManagementJson.Serialize(leaveRequest.ToDto(leaveType));
        leaveRequest.Modify(request.StartDate, request.EndDate, request.PartialDays, totalDays, request.Reason, request.AttachmentIds, request.ContactNumber, request.AlternateArrangements);
        repository.UpdateRequest(leaveRequest);
        await LeaveManagementHandlerHelpers.AddActivityAsync(repository, currentUserService, leaveRequest.Id, "Modify", before, LeaveManagementJson.Serialize(leaveRequest.ToDto(leaveType)), request.Reason, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveRequestDto>.Success(leaveRequest.ToDto(leaveType));
    }
}

/// <summary>
/// Handles cancellation.
/// </summary>
public sealed class CancelLeaveRequestCommandHandler(
    ILeaveRepository repository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CancelLeaveRequestCommand, Result<LeaveMutationResultDto>>
{
    public async Task<Result<LeaveMutationResultDto>> Handle(CancelLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.GetRequestByIdAsync(request.RequestId, cancellationToken);
        if (leaveRequest is null)
        {
            return Result<LeaveMutationResultDto>.Failure("Leave request was not found.");
        }

        var balance = await repository.GetBalanceAsync(leaveRequest.UserId, leaveRequest.LeaveTypeId, leaveRequest.StartDate.Year, cancellationToken);
        if (balance is not null && leaveRequest.Status == LeaveRequestStatus.Pending)
        {
            balance.Release(leaveRequest.TotalDays);
            repository.UpdateBalance(balance);
        }

        leaveRequest.Cancel(request.Reason);
        repository.UpdateRequest(leaveRequest);
        await LeaveManagementHandlerHelpers.AddActivityAsync(repository, currentUserService, leaveRequest.Id, "Cancel", null, null, request.Reason, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveMutationResultDto>.Success(new LeaveMutationResultDto(leaveRequest.Id, "Leave request cancelled."));
    }
}

/// <summary>
/// Handles withdrawal.
/// </summary>
public sealed class WithdrawLeaveRequestCommandHandler(
    ILeaveRepository repository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<WithdrawLeaveRequestCommand, Result<LeaveMutationResultDto>>
{
    public async Task<Result<LeaveMutationResultDto>> Handle(WithdrawLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.GetRequestByIdAsync(request.RequestId, cancellationToken);
        if (leaveRequest is null)
        {
            return Result<LeaveMutationResultDto>.Failure("Leave request was not found.");
        }

        leaveRequest.Withdraw();
        repository.UpdateRequest(leaveRequest);
        await LeaveManagementHandlerHelpers.AddActivityAsync(repository, currentUserService, leaveRequest.Id, "Withdraw", null, null, null, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveMutationResultDto>.Success(new LeaveMutationResultDto(leaveRequest.Id, "Leave request withdrawn."));
    }
}

/// <summary>
/// Handles resubmission.
/// </summary>
public sealed class ResubmitLeaveRequestCommandHandler(
    ILeaveRepository repository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ResubmitLeaveRequestCommand, Result<LeaveMutationResultDto>>
{
    public async Task<Result<LeaveMutationResultDto>> Handle(ResubmitLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.GetRequestByIdAsync(request.RequestId, cancellationToken);
        if (leaveRequest is null)
        {
            return Result<LeaveMutationResultDto>.Failure("Leave request was not found.");
        }

        leaveRequest.MoveToApprovalLevel(1);
        repository.UpdateRequest(leaveRequest);
        await LeaveManagementHandlerHelpers.AddActivityAsync(repository, currentUserService, leaveRequest.Id, "Resubmit", null, null, "Employee resubmitted after changes.", cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveMutationResultDto>.Success(new LeaveMutationResultDto(leaveRequest.Id, "Leave request resubmitted."));
    }
}
