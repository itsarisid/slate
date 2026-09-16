using Alphabet.Application.Features.LeaveManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.LeaveManagement;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.LeaveManagement.Commands;

/// <summary>
/// Creates a leave type.
/// </summary>
public sealed record CreateLeaveTypeCommand(
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
    bool IsActive) : IRequest<Result<LeaveTypeDto>>;

/// <summary>
/// Updates a leave type.
/// </summary>
public sealed record UpdateLeaveTypeCommand(
    Guid LeaveTypeId,
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
    bool IsActive) : IRequest<Result<LeaveTypeDto>>;

/// <summary>
/// Deactivates a leave type.
/// </summary>
public sealed record DeleteLeaveTypeCommand(Guid LeaveTypeId) : IRequest<Result<LeaveMutationResultDto>>;

/// <summary>
/// Handles leave type creation.
/// </summary>
public sealed class CreateLeaveTypeCommandHandler(ILeaveRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateLeaveTypeCommand, Result<LeaveTypeDto>>
{
    public async Task<Result<LeaveTypeDto>> Handle(CreateLeaveTypeCommand request, CancellationToken cancellationToken)
    {
        if (await repository.GetLeaveTypeByCodeAsync(request.Code, cancellationToken) is not null)
        {
            return Result<LeaveTypeDto>.Failure($"Leave type code '{request.Code}' already exists.");
        }

        var leaveType = LeaveType.Create(request.Name, request.Code, request.Description, request.Color, request.Icon, request.IsPaid, request.DefaultDaysPerYear, request.MaxConsecutiveDays, request.MinDaysPerRequest, request.MaxDaysPerRequest, request.RequiresApproval, request.ApprovalChainId, request.CarryForwardEnabled, request.MaxCarryForwardDays, request.CarryForwardExpiryMonths, request.EncashmentEnabled, request.EncashmentRate, request.ProrationEnabled, request.EligibilityRules, request.BlackoutDates, request.RequiresAttachment, request.AllowedAttachmentTypes, request.AutoApproveRules, request.IsActive);
        await repository.AddLeaveTypeAsync(leaveType, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveTypeDto>.Success(leaveType.ToDto());
    }
}

/// <summary>
/// Handles leave type updates.
/// </summary>
public sealed class UpdateLeaveTypeCommandHandler(ILeaveRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateLeaveTypeCommand, Result<LeaveTypeDto>>
{
    public async Task<Result<LeaveTypeDto>> Handle(UpdateLeaveTypeCommand request, CancellationToken cancellationToken)
    {
        var leaveType = await repository.GetLeaveTypeByIdAsync(request.LeaveTypeId, cancellationToken);
        if (leaveType is null)
        {
            return Result<LeaveTypeDto>.Failure("Leave type was not found.");
        }

        leaveType.Update(request.Name, request.Description, request.Color, request.Icon, request.IsPaid, request.DefaultDaysPerYear, request.MaxConsecutiveDays, request.MinDaysPerRequest, request.MaxDaysPerRequest, request.RequiresApproval, request.ApprovalChainId, request.CarryForwardEnabled, request.MaxCarryForwardDays, request.CarryForwardExpiryMonths, request.EncashmentEnabled, request.EncashmentRate, request.ProrationEnabled, request.EligibilityRules, request.BlackoutDates, request.RequiresAttachment, request.AllowedAttachmentTypes, request.AutoApproveRules, request.IsActive);
        repository.UpdateLeaveType(leaveType);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveTypeDto>.Success(leaveType.ToDto());
    }
}

/// <summary>
/// Handles leave type deletion.
/// </summary>
public sealed class DeleteLeaveTypeCommandHandler(ILeaveRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteLeaveTypeCommand, Result<LeaveMutationResultDto>>
{
    public async Task<Result<LeaveMutationResultDto>> Handle(DeleteLeaveTypeCommand request, CancellationToken cancellationToken)
    {
        var leaveType = await repository.GetLeaveTypeByIdAsync(request.LeaveTypeId, cancellationToken);
        if (leaveType is null)
        {
            return Result<LeaveMutationResultDto>.Failure("Leave type was not found.");
        }

        leaveType.Deactivate();
        repository.UpdateLeaveType(leaveType);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveMutationResultDto>.Success(new LeaveMutationResultDto(leaveType.Id, $"Leave type '{leaveType.Name}' was deactivated."));
    }
}
