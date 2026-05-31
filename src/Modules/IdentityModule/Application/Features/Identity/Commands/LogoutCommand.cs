using Alphabet.Application.Results;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Logs a user out by revoking a refresh token.
/// </summary>
public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;
/// <summary>
/// Logout command validator.
/// </summary>

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
/// <summary>
/// Logout command handler.
/// </summary>

public sealed class LogoutCommandHandler(IIdentityService identityService) : IRequestHandler<LogoutCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    => identityService.LogoutAsync(request.RefreshToken, cancellationToken);
}
