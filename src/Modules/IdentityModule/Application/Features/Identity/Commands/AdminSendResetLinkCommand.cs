using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Admin sends a password-reset link to the user's email.
/// </summary>
public sealed record AdminSendResetLinkCommand(Guid UserId) : IRequest<Result>;

public sealed class AdminSendResetLinkCommandHandler(IIdentityService identityService)
    : IRequestHandler<AdminSendResetLinkCommand, Result>
{
    public Task<Result> Handle(AdminSendResetLinkCommand request, CancellationToken cancellationToken)
        => identityService.AdminSendResetLinkAsync(request.UserId, cancellationToken);
}
