using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Results;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Triggers the forgot-password workflow.
/// </summary>
public sealed record ForgotPasswordCommand(string Email) : IRequest<Result>;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public sealed class ForgotPasswordCommandHandler(IIdentityService identityService) : IRequestHandler<ForgotPasswordCommand, Result>
{
    public Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        => identityService.SendForgotPasswordAsync(request.Email, cancellationToken);
}
