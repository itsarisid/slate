using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Identity.Queries;

/// <summary>
/// Returns detailed user information for an admin view.
/// </summary>
public sealed record GetUserByIdQuery(Guid UserId) : IRequest<Result<AdminUserDetailDto>>;
/// <summary>
/// Get user by id query handler.
/// </summary>

public sealed class GetUserByIdQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetUserByIdQuery, Result<AdminUserDetailDto>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public Task<Result<AdminUserDetailDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    => identityService.GetUserByIdAsync(request.UserId, cancellationToken);
}
