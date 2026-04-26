using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Results;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands.Mfa;

/// <summary>
/// Verifies an OTP challenge and enables OTP MFA.
/// </summary>
public sealed record VerifyOtpCommand(string VerificationCode, string Destination) : IRequest<Result>;

public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.VerificationCode).NotEmpty().Length(6);
        RuleFor(x => x.Destination).NotEmpty();
    }
}

public sealed class VerifyOtpCommandHandler(IIdentityService identityService, ICurrentUserService currentUserService)
    : IRequestHandler<VerifyOtpCommand, Result>
{
    public Task<Result> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        return currentUserService.UserId is { } userId
            ? identityService.VerifyOtpAsync(userId, request.Destination, request.VerificationCode, cancellationToken)
            : Task.FromResult(Result.Failure("Authenticated user context is required."));
    }
}
