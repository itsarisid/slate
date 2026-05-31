using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands.Mfa;

/// <summary>
/// Verifies an authenticator app code and enables MFA.
/// </summary>
public sealed record VerifyAuthenticatorCommand(string VerificationCode) : IRequest<Result<RecoveryCodesDto>>;
/// <summary>
/// Verify authenticator command validator.
/// </summary>

public sealed class VerifyAuthenticatorCommandValidator : AbstractValidator<VerifyAuthenticatorCommand>
{
    public VerifyAuthenticatorCommandValidator()
    {
        RuleFor(x => x.VerificationCode).NotEmpty().Length(6);
    }
}
/// <summary>
/// Verify authenticator command handler.
/// </summary>

public sealed class VerifyAuthenticatorCommandHandler(IIdentityService identityService, ICurrentUserService currentUserService)
    : IRequestHandler<VerifyAuthenticatorCommand, Result<RecoveryCodesDto>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public Task<Result<RecoveryCodesDto>> Handle(VerifyAuthenticatorCommand request, CancellationToken cancellationToken)
    {
        return currentUserService.UserId is { } userId
            ? identityService.VerifyAuthenticatorAsync(userId, request.VerificationCode, cancellationToken)
            : Task.FromResult(Result<RecoveryCodesDto>.Failure("Authenticated user context is required."));
    }
}
