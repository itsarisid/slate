using Alphabet.Application.Results;
using Alphabet.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands.Mfa;

/// <summary>
/// Sends an OTP challenge for email or SMS MFA.
/// </summary>
public sealed record EnableOtpCommand(TwoFactorMethod DeliveryMethod, string Destination) : IRequest<Result>;
/// <summary>
/// Enable otp command validator.
/// </summary>

public sealed class EnableOtpCommandValidator : AbstractValidator<EnableOtpCommand>
{
    public EnableOtpCommandValidator()
    {
        RuleFor(x => x.DeliveryMethod)
            .Must(x => x is TwoFactorMethod.Email or TwoFactorMethod.Sms)
            .WithMessage("Delivery method must be Email or Sms.");
        RuleFor(x => x.Destination).NotEmpty().MaximumLength(256);
    }
}
/// <summary>
/// Enable otp command handler.
/// </summary>

public sealed class EnableOtpCommandHandler(IIdentityService identityService, ICurrentUserService currentUserService)
    : IRequestHandler<EnableOtpCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public Task<Result> Handle(EnableOtpCommand request, CancellationToken cancellationToken)
    {
        return currentUserService.UserId is { } userId
            ? identityService.EnableOtpAsync(userId, request.DeliveryMethod, request.Destination, cancellationToken)
            : Task.FromResult(Result.Failure("Authenticated user context is required."));
    }
}
