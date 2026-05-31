using Alphabet.Application.Features.Identity.Dtos;
using MediatR;

namespace Alphabet.Application.Features.Identity.Queries;

/// <summary>
/// Returns a list of registered users for administration.
/// </summary>
public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserDto>>;
/// <summary>
/// Get users query handler.
/// </summary>

public sealed class GetUsersQueryHandler(IIdentityService identityService) : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    => identityService.GetUsersAsync(cancellationToken);
}
