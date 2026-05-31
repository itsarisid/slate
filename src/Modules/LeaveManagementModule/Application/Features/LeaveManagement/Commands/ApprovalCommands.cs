using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.LeaveManagement;
using Alphabet.Application.Features.LeaveManagement.Dtos;
using Alphabet.Application.Features.LeaveManagement.Shared;
using Alphabet.Application.Results;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.LeaveManagement;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.LeaveManagement.Commands;

/// <summary>
/// Creates an approval chain.
/// </summary>
public sealed record CreateApprovalChainCommand(
    string Name,
    string Code,
    string Description,
    ApprovalChainApplicability ApplicableTo,
    IReadOnlyCollection<ApprovalLevelDefinition> ApprovalLevels,
    int FinalApprovalLevel,
    bool AllowSkipLevels,
    bool ParallelApproval,
    bool IsActive) : IRequest<Result<ApprovalChainDto>>;

/// <summary>
/// Approves a leave request.
/// </summary>
public sealed record ApproveLeaveRequestCommand(Guid RequestId, string? Comment, IReadOnlyCollection<string> Attachments, int? Level, bool NotifyApplicant, bool ApplyPartialApproval, decimal? ApprovedDays)
    : IRequest<Result<LeaveMutationResultDto>>;

/// <summary>
/// Rejects a leave request.
/// </summary>
public sealed record RejectLeaveRequestCommand(Guid RequestId, string Reason, IReadOnlyCollection<LeaveSuggestedDateRange> SuggestedDates)
    : IRequest<Result<LeaveMutationResultDto>>;

/// <summary>
/// Requests changes for a leave request.
/// </summary>
public sealed record RequestLeaveChangesCommand(Guid RequestId, string Comment) : IRequest<Result<LeaveMutationResultDto>>;

/// <summary>
/// Applies one action to multiple approval tasks.
/// </summary>
public sealed record BatchLeaveApprovalCommand(IReadOnlyCollection<Guid> RequestIds, string Action, string Comment) : IRequest<Result<LeaveMutationResultDto>>;

/// <summary>
/// Handles approval-chain creation.
/// </summary>
public sealed class CreateApprovalChainCommandHandler(ILeaveRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateApprovalChainCommand, Result<ApprovalChainDto>>
{
    public async Task<Result<ApprovalChainDto>> Handle(CreateApprovalChainCommand request, CancellationToken cancellationToken)
    {
        var chain = ApprovalChain.Create(request.Name, request.Code, request.Description, request.ApplicableTo, request.ApprovalLevels, request.FinalApprovalLevel, request.AllowSkipLevels, request.ParallelApproval, request.IsActive);
        await repository.AddApprovalChainAsync(chain, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ApprovalChainDto>.Success(chain.ToDto());
    }
}

/// <summary>
/// Handles request approval.
/// </summary>
public sealed class ApproveLeaveRequestCommandHandler(
    ILeaveRepository repository,
    ILeaveApproverResolver approverResolver,
    ILeaveNotificationService notificationService,
    ILeaveCalendarSyncService calendarSyncService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ApproveLeaveRequestCommand, Result<LeaveMutationResultDto>>
{
    public async Task<Result<LeaveMutationResultDto>> Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.GetRequestByIdAsync(request.RequestId, cancellationToken);
        var workflow = await repository.GetWorkflowByRequestIdAsync(request.RequestId, cancellationToken);
        if (leaveRequest is null || workflow is null)
        {
            return Result<LeaveMutationResultDto>.Failure("Leave request or workflow was not found.");
        }

        var currentUserId = LeaveManagementHandlerHelpers.RequireUserId(currentUserService);
        if (currentUserId == leaveRequest.UserId)
        {
            return Result<LeaveMutationResultDto>.Failure("Users cannot approve their own leave requests.");
        }

        var step = await repository.GetPendingStepAsync(workflow.Id, request.Level ?? workflow.CurrentLevel, currentUserId, cancellationToken)
            ?? await repository.GetPendingStepAsync(workflow.Id, request.Level ?? workflow.CurrentLevel, null, cancellationToken);
        if (step is null)
        {
            return Result<LeaveMutationResultDto>.Failure("No pending approval step was found for this request.");
        }

        step.Approve(request.Comment, request.Attachments);
        repository.UpdateWorkflowStep(step);
        var chain = await repository.GetApprovalChainByIdAsync(workflow.ApprovalChainId, cancellationToken);
        var nextLevel = chain?.ApprovalLevels.OrderBy(x => x.Level).FirstOrDefault(x => x.Level > step.Level);
        if (nextLevel is null)
        {
            workflow.Complete();
            leaveRequest.Approve();
            var balance = await repository.GetBalanceAsync(leaveRequest.UserId, leaveRequest.LeaveTypeId, leaveRequest.StartDate.Year, cancellationToken);
            balance?.Approve(request.ApprovedDays ?? leaveRequest.TotalDays);
            if (balance is not null)
            {
                repository.UpdateBalance(balance);
            }

            await calendarSyncService.SyncApprovedLeaveAsync(leaveRequest, cancellationToken);
            await notificationService.NotifyRequestStatusChangedAsync(leaveRequest, "Leave request approved.", cancellationToken);
        }
        else
        {
            workflow.AdvanceTo(nextLevel.Level);
            leaveRequest.MoveToApprovalLevel(nextLevel.Level);
            var resolution = await approverResolver.ResolveAsync(leaveRequest, nextLevel, cancellationToken);
            await repository.AddWorkflowStepAsync(WorkflowStep.Create(workflow.Id, nextLevel.Level, resolution.UserId, nextLevel.ApproverType, nextLevel.ApproverValue, nextLevel.TimeoutHours), cancellationToken);
            await notificationService.NotifyApproversAsync(leaveRequest, nextLevel, cancellationToken);
        }

        repository.UpdateWorkflow(workflow);
        repository.UpdateRequest(leaveRequest);
        await LeaveManagementHandlerHelpers.AddActivityAsync(repository, currentUserService, leaveRequest.Id, "Approve", null, null, request.Comment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveMutationResultDto>.Success(new LeaveMutationResultDto(leaveRequest.Id, "Approval recorded."));
    }
}

/// <summary>
/// Handles leave rejection.
/// </summary>
public sealed class RejectLeaveRequestCommandHandler(
    ILeaveRepository repository,
    ILeaveNotificationService notificationService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RejectLeaveRequestCommand, Result<LeaveMutationResultDto>>
{
    public async Task<Result<LeaveMutationResultDto>> Handle(RejectLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.GetRequestByIdAsync(request.RequestId, cancellationToken);
        var workflow = await repository.GetWorkflowByRequestIdAsync(request.RequestId, cancellationToken);
        if (leaveRequest is null)
        {
            return Result<LeaveMutationResultDto>.Failure("Leave request was not found.");
        }

        leaveRequest.Reject();
        workflow?.Reject();
        var balance = await repository.GetBalanceAsync(leaveRequest.UserId, leaveRequest.LeaveTypeId, leaveRequest.StartDate.Year, cancellationToken);
        balance?.Release(leaveRequest.TotalDays);
        if (balance is not null)
        {
            repository.UpdateBalance(balance);
        }

        repository.UpdateRequest(leaveRequest);
        if (workflow is not null)
        {
            repository.UpdateWorkflow(workflow);
        }

        await notificationService.NotifyRequestStatusChangedAsync(leaveRequest, $"Leave request rejected: {request.Reason}", cancellationToken);
        await LeaveManagementHandlerHelpers.AddActivityAsync(repository, currentUserService, leaveRequest.Id, "Reject", null, LeaveManagementJson.Serialize(request.SuggestedDates), request.Reason, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveMutationResultDto>.Success(new LeaveMutationResultDto(leaveRequest.Id, "Leave request rejected."));
    }
}

/// <summary>
/// Handles request-changes actions.
/// </summary>
public sealed class RequestLeaveChangesCommandHandler(
    ILeaveRepository repository,
    ILeaveNotificationService notificationService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RequestLeaveChangesCommand, Result<LeaveMutationResultDto>>
{
    public async Task<Result<LeaveMutationResultDto>> Handle(RequestLeaveChangesCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.GetRequestByIdAsync(request.RequestId, cancellationToken);
        if (leaveRequest is null)
        {
            return Result<LeaveMutationResultDto>.Failure("Leave request was not found.");
        }

        leaveRequest.RequestChanges();
        repository.UpdateRequest(leaveRequest);
        await notificationService.NotifyRequestStatusChangedAsync(leaveRequest, "Changes were requested for your leave request.", cancellationToken);
        await LeaveManagementHandlerHelpers.AddActivityAsync(repository, currentUserService, leaveRequest.Id, "RequestChanges", null, null, request.Comment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveMutationResultDto>.Success(new LeaveMutationResultDto(leaveRequest.Id, "Changes requested."));
    }
}

/// <summary>
/// Handles batch approval actions.
/// </summary>
public sealed class BatchLeaveApprovalCommandHandler(ISender sender)
    : IRequestHandler<BatchLeaveApprovalCommand, Result<LeaveMutationResultDto>>
{
    public async Task<Result<LeaveMutationResultDto>> Handle(BatchLeaveApprovalCommand request, CancellationToken cancellationToken)
    {
        foreach (var requestId in request.RequestIds)
        {
            if (request.Action.Equals("Approve", StringComparison.OrdinalIgnoreCase))
            {
                await sender.Send(new ApproveLeaveRequestCommand(requestId, request.Comment, [], null, true, false, null), cancellationToken);
            }
            else
            {
                await sender.Send(new RejectLeaveRequestCommand(requestId, request.Comment, []), cancellationToken);
            }
        }

        return Result<LeaveMutationResultDto>.Success(new LeaveMutationResultDto(Guid.NewGuid(), $"Processed {request.RequestIds.Count} leave request(s)."));
    }
}
