using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands.Mfa;

/// <summary>
/// Generates authenticator app setup information.
/// </summary>
public sealed record EnableAuthenticatorCommand : IRequest<Result<AuthenticatorSetupDto>>;

public sealed class EnableAuthenticatorCommandHandler(IIdentityService identityService, ICurrentUserService currentUserService)
    : IRequestHandler<EnableAuthenticatorCommand, Result<AuthenticatorSetupDto>>
{
    public Task<Result<AuthenticatorSetupDto>> Handle(EnableAuthenticatorCommand request, CancellationToken cancellationToken)
    {
        return currentUserService.UserId is { } userId
            ? identityService.EnableAuthenticatorAsync(userId, cancellationToken)
            : Task.FromResult(Result<AuthenticatorSetupDto>.Failure("Authenticated user context is required."));
    }
}
