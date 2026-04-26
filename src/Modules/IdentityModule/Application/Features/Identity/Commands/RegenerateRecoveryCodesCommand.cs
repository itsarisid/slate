using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Regenerates MFA recovery codes for the authenticated user.
/// </summary>
public sealed record RegenerateRecoveryCodesCommand : IRequest<Result<RecoveryCodesDto>>;

public sealed class RegenerateRecoveryCodesCommandHandler(IIdentityService identityService, ICurrentUserService currentUserService)
    : IRequestHandler<RegenerateRecoveryCodesCommand, Result<RecoveryCodesDto>>
{
    public Task<Result<RecoveryCodesDto>> Handle(RegenerateRecoveryCodesCommand request, CancellationToken cancellationToken)
    {
        return currentUserService.UserId is { } userId
            ? identityService.RegenerateRecoveryCodesAsync(userId, cancellationToken)
            : Task.FromResult(Result<RecoveryCodesDto>.Failure("Authenticated user context is required."));
    }
}
