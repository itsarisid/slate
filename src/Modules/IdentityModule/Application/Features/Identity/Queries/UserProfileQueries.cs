using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Identity.Queries;

public sealed record GetUserSessionsQuery(Guid UserId) : IRequest<Result<IReadOnlyList<UserSessionDto>>>;

public sealed class GetUserSessionsQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetUserSessionsQuery, Result<IReadOnlyList<UserSessionDto>>>
{
    public Task<Result<IReadOnlyList<UserSessionDto>>> Handle(GetUserSessionsQuery request, CancellationToken cancellationToken)
        => identityService.GetSessionsAsync(request.UserId, cancellationToken);
}
