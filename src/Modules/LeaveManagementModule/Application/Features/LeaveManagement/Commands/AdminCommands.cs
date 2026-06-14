using Alphabet.Application.Common.Interfaces.LeaveManagement;
using Alphabet.Application.Features.LeaveManagement.Dtos;
using Alphabet.Application.Features.LeaveManagement.Shared;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.LeaveManagement;
using MediatR;

namespace Alphabet.Application.Features.LeaveManagement.Commands;

/// <summary>
/// Creates a delegation.
/// </summary>
public sealed record CreateDelegationCommand(Guid DelegatorUserId, Guid DelegateToUserId, LeaveDelegationType DelegationType, LeaveDelegationPermission Permission, IReadOnlyCollection<Guid> ApplicableLeaveTypes, IReadOnlyCollection<int> ApplicableApprovalLevels, IReadOnlyCollection<string> ApplicableDepartments, IReadOnlyCollection<Guid> ApplicableEmployees, DateOnly StartDate, DateOnly? EndDate, string Reason, bool IsActive)
    : IRequest<Result<DelegationDto>>;

/// <summary>
/// Revokes a delegation.
/// </summary>
public sealed record RevokeDelegationCommand(Guid DelegationId) : IRequest<Result<LeaveMutationResultDto>>;

/// <summary>
/// Creates a public holiday.
/// </summary>
public sealed record CreatePublicHolidayCommand(string Name, DateOnly Date, string Country, string? State, bool IsPaid, bool Recurring)
    : IRequest<Result<PublicHolidayDto>>;

/// <summary>
/// Creates a blackout period.
/// </summary>
public sealed record CreateBlackoutPeriodCommand(DateOnly StartDate, DateOnly EndDate, string Reason, IReadOnlyCollection<string> ApplicableTo)
    : IRequest<Result<LeaveMutationResultDto>>;

/// <summary>
/// Creates an accrual rule.
/// </summary>
public sealed record CreateAccrualRuleCommand(Guid LeaveTypeId, LeaveAccrualMethod AccrualMethod, decimal AccrualRate, decimal MaxAccrual, string TenureRulesJson)
    : IRequest<Result<LeaveMutationResultDto>>;

public sealed class CreateDelegationCommandHandler(
    ILeaveRepository repository,
    ILeaveNotificationService notificationService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateDelegationCommand, Result<DelegationDto>>
{
    public async Task<Result<DelegationDto>> Handle(CreateDelegationCommand request, CancellationToken cancellationToken)
    {
        var delegation = Delegation.Create(request.DelegatorUserId, request.DelegateToUserId, request.DelegationType, request.Permission, request.ApplicableLeaveTypes, request.ApplicableApprovalLevels, request.ApplicableDepartments, request.ApplicableEmployees, request.StartDate, request.EndDate, request.Reason, request.IsActive);
        await repository.AddDelegationAsync(delegation, cancellationToken);
        await LeaveManagementHandlerHelpers.AddActivityAsync(repository, currentUserService, null, "DelegationCreate", null, null, request.Reason, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyDelegationCreatedAsync(delegation, cancellationToken);
        return Result<DelegationDto>.Success(delegation.ToDto());
    }
}

public sealed class RevokeDelegationCommandHandler(
    ILeaveRepository repository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RevokeDelegationCommand, Result<LeaveMutationResultDto>>
{
    public async Task<Result<LeaveMutationResultDto>> Handle(RevokeDelegationCommand request, CancellationToken cancellationToken)
    {
        var delegation = await repository.GetDelegationByIdAsync(request.DelegationId, cancellationToken);
        if (delegation is null)
        {
            return Result<LeaveMutationResultDto>.Failure("Delegation was not found.");
        }

        delegation.Revoke();
        repository.UpdateDelegation(delegation);
        await LeaveManagementHandlerHelpers.AddActivityAsync(repository, currentUserService, null, "DelegationRevoke", null, null, $"Delegation {delegation.Id} revoked.", cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveMutationResultDto>.Success(new LeaveMutationResultDto(delegation.Id, "Delegation revoked."));
    }
}

public sealed class CreatePublicHolidayCommandHandler(ILeaveRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePublicHolidayCommand, Result<PublicHolidayDto>>
{
    public async Task<Result<PublicHolidayDto>> Handle(CreatePublicHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = PublicHoliday.Create(request.Name, request.Date, request.Country, request.State, request.IsPaid, request.Recurring);
        await repository.AddPublicHolidayAsync(holiday, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PublicHolidayDto>.Success(new PublicHolidayDto(holiday.Id, holiday.Name, holiday.Date, holiday.Country, holiday.State, holiday.IsPaid, holiday.Recurring));
    }
}

public sealed class CreateBlackoutPeriodCommandHandler(ILeaveRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBlackoutPeriodCommand, Result<LeaveMutationResultDto>>
{
    public async Task<Result<LeaveMutationResultDto>> Handle(CreateBlackoutPeriodCommand request, CancellationToken cancellationToken)
    {
        var blackout = BlackoutPeriod.Create(request.StartDate, request.EndDate, request.Reason, request.ApplicableTo);
        await repository.AddBlackoutPeriodAsync(blackout, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveMutationResultDto>.Success(new LeaveMutationResultDto(blackout.Id, "Blackout period created."));
    }
}

public sealed class CreateAccrualRuleCommandHandler(ILeaveRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAccrualRuleCommand, Result<LeaveMutationResultDto>>
{
    public async Task<Result<LeaveMutationResultDto>> Handle(CreateAccrualRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = AccrualRule.Create(request.LeaveTypeId, request.AccrualMethod, request.AccrualRate, request.MaxAccrual, request.TenureRulesJson);
        await repository.AddAccrualRuleAsync(rule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveMutationResultDto>.Success(new LeaveMutationResultDto(rule.Id, "Accrual rule created."));
    }
}
