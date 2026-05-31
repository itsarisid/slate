using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Unlocks a user account.
/// </summary>
public sealed record UnlockUserCommand(Guid UserId) : IRequest<Result>;
/// <summary>
/// Unlock user command handler.
/// </summary>

public sealed class UnlockUserCommandHandler(IIdentityService identityService) : IRequestHandler<UnlockUserCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public Task<Result> Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    => identityService.UnlockUserAsync(request.UserId, cancellationToken);
}
