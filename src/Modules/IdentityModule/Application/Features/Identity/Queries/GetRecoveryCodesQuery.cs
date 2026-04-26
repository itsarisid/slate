using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Identity.Queries;

/// <summary>
/// Gets current recovery codes for the authenticated user.
/// </summary>
public sealed record GetRecoveryCodesQuery : IRequest<Result<RecoveryCodesDto>>;

public sealed class GetRecoveryCodesQueryHandler(IIdentityService identityService, ICurrentUserService currentUserService)
    : IRequestHandler<GetRecoveryCodesQuery, Result<RecoveryCodesDto>>
{
    public Task<Result<RecoveryCodesDto>> Handle(GetRecoveryCodesQuery request, CancellationToken cancellationToken)
    {
        return currentUserService.UserId is { } userId
            ? identityService.GetRecoveryCodesAsync(userId, cancellationToken)
            : Task.FromResult(Result<RecoveryCodesDto>.Failure("Authenticated user context is required."));
    }
}
