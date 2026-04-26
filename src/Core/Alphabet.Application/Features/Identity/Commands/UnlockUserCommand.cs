using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Unlocks a user account.
/// </summary>
public sealed record UnlockUserCommand(Guid UserId) : IRequest<Result>;

public sealed class UnlockUserCommandHandler(IIdentityService identityService) : IRequestHandler<UnlockUserCommand, Result>
{
    public Task<Result> Handle(UnlockUserCommand request, CancellationToken cancellationToken)
        => identityService.UnlockUserAsync(request.UserId, cancellationToken);
}
