using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands.Mfa;

/// <summary>
/// Completes an MFA login using a short-lived MFA token and verification code.
/// </summary>
public sealed record MfaLoginCommand(string MfaToken, string VerificationCode) : IRequest<Result<AuthResponseDto>>;
/// <summary>
/// Mfa login command validator.
/// </summary>

public sealed class MfaLoginCommandValidator : AbstractValidator<MfaLoginCommand>
{
    public MfaLoginCommandValidator()
    {
        RuleFor(x => x.MfaToken).NotEmpty();
        RuleFor(x => x.VerificationCode).NotEmpty().MinimumLength(6);
    }
}
/// <summary>
/// Mfa login command handler.
/// </summary>

public sealed class MfaLoginCommandHandler(IIdentityService identityService)
    : IRequestHandler<MfaLoginCommand, Result<AuthResponseDto>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public Task<Result<AuthResponseDto>> Handle(MfaLoginCommand request, CancellationToken cancellationToken)
    => identityService.CompleteMfaLoginAsync(request.MfaToken, request.VerificationCode, cancellationToken);
}
