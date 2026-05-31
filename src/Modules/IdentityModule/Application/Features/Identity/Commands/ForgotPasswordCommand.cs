using Alphabet.Application.Results;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Triggers the forgot-password workflow.
/// </summary>
public sealed record ForgotPasswordCommand(string Email) : IRequest<Result>;
/// <summary>
/// Forgot password command validator.
/// </summary>

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
/// <summary>
/// Forgot password command handler.
/// </summary>

public sealed class ForgotPasswordCommandHandler(IIdentityService identityService) : IRequestHandler<ForgotPasswordCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    => identityService.SendForgotPasswordAsync(request.Email, cancellationToken);
}
