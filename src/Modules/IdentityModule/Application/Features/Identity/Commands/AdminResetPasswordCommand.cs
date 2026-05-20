using Alphabet.Application.Results;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Admin resets a user's password without requiring the old password.
/// </summary>
public sealed record AdminResetPasswordCommand(Guid UserId, string NewPassword) : IRequest<Result>;
/// <summary>
/// Admin reset password command validator.
/// </summary>

public sealed class AdminResetPasswordCommandValidator : AbstractValidator<AdminResetPasswordCommand>
{
    public AdminResetPasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.");
    }
}
/// <summary>
/// Admin reset password command handler.
/// </summary>

public sealed class AdminResetPasswordCommandHandler(IIdentityService identityService)
    : IRequestHandler<AdminResetPasswordCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public Task<Result> Handle(AdminResetPasswordCommand request, CancellationToken cancellationToken)
    => identityService.AdminResetPasswordAsync(request.UserId, request.NewPassword, cancellationToken);
}
