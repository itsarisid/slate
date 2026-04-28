using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Admin force-logs out a user by revoking all refresh tokens and updating the security stamp.
/// </summary>
public sealed record AdminForceLogoutCommand(Guid UserId) : IRequest<Result>;

public sealed class AdminForceLogoutCommandHandler(IIdentityService identityService)
    : IRequestHandler<AdminForceLogoutCommand, Result>
{
    public Task<Result> Handle(AdminForceLogoutCommand request, CancellationToken cancellationToken)
        => identityService.AdminForceLogoutAsync(request.UserId, cancellationToken);
}
