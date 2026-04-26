using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Results;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Resets a user password with a reset token.
/// </summary>
public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword, string ConfirmPassword) : IRequest<Result>;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
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

public sealed class ResetPasswordCommandHandler(IIdentityService identityService) : IRequestHandler<ResetPasswordCommand, Result>
{
    public Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        => identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, cancellationToken);
}
