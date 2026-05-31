using Alphabet.Application.Features.LeaveManagement.Commands;
using FluentValidation;

namespace Alphabet.Application.Features.LeaveManagement.Shared;

/// <summary>
/// Validates leave type creation.
/// </summary>
public sealed class CreateLeaveTypeCommandValidator : AbstractValidator<CreateLeaveTypeCommand>
{
    public CreateLeaveTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DefaultDaysPerYear).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinDaysPerRequest).GreaterThan(0);
    }
}

/// <summary>
/// Validates leave request submission.
/// </summary>
public sealed class SubmitLeaveRequestCommandValidator : AbstractValidator<SubmitLeaveRequestCommand>
{
    public SubmitLeaveRequestCommandValidator()
    {
        RuleFor(x => x.LeaveTypeId).NotEmpty();
        RuleFor(x => x.StartDate).LessThanOrEqualTo(x => x.EndDate);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
    }
}

/// <summary>
/// Validates balance adjustment.
/// </summary>
public sealed class AdjustLeaveBalanceCommandValidator : AbstractValidator<AdjustLeaveBalanceCommand>
{
    public AdjustLeaveBalanceCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.LeaveTypeId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
