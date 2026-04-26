using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Confirms a user email address.
/// </summary>
public sealed record ConfirmEmailCommand(Guid UserId, string Token) : IRequest<Result>;

public sealed class ConfirmEmailCommandHandler(IIdentityService identityService) : IRequestHandler<ConfirmEmailCommand, Result>
{
    public Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        => identityService.ConfirmEmailAsync(request.UserId, request.Token, cancellationToken);
}
