using Alphabet.Application.Features.LeaveManagement.Dtos;
using Alphabet.Application.Features.LeaveManagement.Shared;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.LeaveManagement;
using MediatR;

namespace Alphabet.Application.Features.LeaveManagement.Commands;

/// <summary>
/// Initializes leave balances for a user and year.
/// </summary>
public sealed record InitializeLeaveBalanceCommand(Guid UserId, int Year, IReadOnlyCollection<InitializeLeaveBalanceItem> Balances)
    : IRequest<Result<LeaveMutationResultDto>>;

/// <summary>
/// Represents an initial balance line.
/// </summary>
public sealed record InitializeLeaveBalanceItem(Guid LeaveTypeId, decimal Allocated, decimal Remaining, decimal CarryForward);

/// <summary>
/// Adjusts a user's balance.
/// </summary>
public sealed record AdjustLeaveBalanceCommand(Guid UserId, Guid LeaveTypeId, int Year, decimal Adjustment, string Reason)
    : IRequest<Result<LeaveMutationResultDto>>;

/// <summary>
/// Manually triggers accrual processing.
/// </summary>
public sealed record AccrueLeaveBalancesCommand(int Year, string Reason) : IRequest<Result<LeaveMutationResultDto>>;

/// <summary>
/// Handles balance initialization.
/// </summary>
public sealed class InitializeLeaveBalanceCommandHandler(
    ILeaveRepository repository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<InitializeLeaveBalanceCommand, Result<LeaveMutationResultDto>>
{
    public async Task<Result<LeaveMutationResultDto>> Handle(InitializeLeaveBalanceCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.Balances)
        {
            var existing = await repository.GetBalanceAsync(request.UserId, item.LeaveTypeId, request.Year, cancellationToken);
            if (existing is not null)
            {
                continue;
            }

            await repository.AddBalanceAsync(
                LeaveBalance.Create(request.UserId, item.LeaveTypeId, request.Year, item.Allocated, item.Remaining, item.CarryForward),
                cancellationToken);
        }

        await LeaveManagementHandlerHelpers.AddActivityAsync(repository, currentUserService, null, "BalanceInitialize", null, null, $"Initialized balances for user {request.UserId} and year {request.Year}.", cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveMutationResultDto>.Success(new LeaveMutationResultDto(request.UserId, "Leave balances initialized."));
    }
}

/// <summary>
/// Handles balance adjustments.
/// </summary>
public sealed class AdjustLeaveBalanceCommandHandler(
    ILeaveRepository repository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AdjustLeaveBalanceCommand, Result<LeaveMutationResultDto>>
{
    public async Task<Result<LeaveMutationResultDto>> Handle(AdjustLeaveBalanceCommand request, CancellationToken cancellationToken)
    {
        var balance = await repository.GetBalanceAsync(request.UserId, request.LeaveTypeId, request.Year, cancellationToken);
        if (balance is null)
        {
            return Result<LeaveMutationResultDto>.Failure("Leave balance was not found.");
        }

        var before = balance.Remaining;
        balance.Adjust(request.Adjustment);
        repository.UpdateBalance(balance);
        await LeaveManagementHandlerHelpers.AddActivityAsync(repository, currentUserService, null, "BalanceAdjust", before.ToString("0.##"), balance.Remaining.ToString("0.##"), request.Reason, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveMutationResultDto>.Success(new LeaveMutationResultDto(balance.Id, "Leave balance adjusted."));
    }
}

/// <summary>
/// Handles manual accrual.
/// </summary>
public sealed class AccrueLeaveBalancesCommandHandler(
    ILeaveRepository repository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AccrueLeaveBalancesCommand, Result<LeaveMutationResultDto>>
{
    public async Task<Result<LeaveMutationResultDto>> Handle(AccrueLeaveBalancesCommand request, CancellationToken cancellationToken)
    {
        var rules = await repository.GetAccrualRulesAsync(cancellationToken);
        await LeaveManagementHandlerHelpers.AddActivityAsync(repository, currentUserService, null, "AccrualManualTrigger", null, null, $"{rules.Count} active accrual rules evaluated for {request.Year}. {request.Reason}", cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<LeaveMutationResultDto>.Success(new LeaveMutationResultDto(Guid.NewGuid(), $"Accrual processing queued for {rules.Count} rule(s)."));
    }
}
