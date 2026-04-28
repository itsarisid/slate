using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Identity.Queries;

/// <summary>
/// Returns detailed user information for an admin view.
/// </summary>
public sealed record GetUserByIdQuery(Guid UserId) : IRequest<Result<AdminUserDetailDto>>;

public sealed class GetUserByIdQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetUserByIdQuery, Result<AdminUserDetailDto>>
{
    public Task<Result<AdminUserDetailDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        => identityService.GetUserByIdAsync(request.UserId, cancellationToken);
}
