using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Admin sends a password-reset link to the user's email.
/// </summary>
public sealed record AdminSendResetLinkCommand(Guid UserId) : IRequest<Result>;
/// <summary>
/// Admin send reset link command handler.
/// </summary>

public sealed class AdminSendResetLinkCommandHandler(IIdentityService identityService)
    : IRequestHandler<AdminSendResetLinkCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public Task<Result> Handle(AdminSendResetLinkCommand request, CancellationToken cancellationToken)
    => identityService.AdminSendResetLinkAsync(request.UserId, cancellationToken);
}
