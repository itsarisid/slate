using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Results;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Changes the password for the current user.
/// </summary>
public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword, string ConfirmPassword) : IRequest<Result>;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .Matches("[^a-zA-Z0-9]");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword);
    }
}

public sealed class ChangePasswordCommandHandler(IIdentityService identityService, ICurrentUserService currentUserService)
    : IRequestHandler<ChangePasswordCommand, Result>
{
    public Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        return currentUserService.UserId is { } userId
            ? identityService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, cancellationToken)
            : Task.FromResult(Result.Failure("Authenticated user context is required."));
    }
}
