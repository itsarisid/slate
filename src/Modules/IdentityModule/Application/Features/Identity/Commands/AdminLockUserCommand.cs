using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Results;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Admin locks a user account for a specified duration in minutes.
/// Pass 0 for indefinite lockout.
/// </summary>
public sealed record AdminLockUserCommand(Guid UserId, int DurationMinutes = 0) : IRequest<Result>;

public sealed class AdminLockUserCommandValidator : AbstractValidator<AdminLockUserCommand>
{
    public AdminLockUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DurationMinutes).GreaterThanOrEqualTo(0);
    }
}

public sealed class AdminLockUserCommandHandler(IIdentityService identityService)
    : IRequestHandler<AdminLockUserCommand, Result>
{
    public Task<Result> Handle(AdminLockUserCommand request, CancellationToken cancellationToken)
        => identityService.LockUserAsync(request.UserId, request.DurationMinutes, cancellationToken);
}
