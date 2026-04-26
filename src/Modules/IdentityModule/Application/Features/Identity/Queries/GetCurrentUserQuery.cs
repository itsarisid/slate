using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Identity.Queries;

/// <summary>
/// Returns the currently authenticated user and resolved roles.
/// </summary>
public sealed record GetCurrentUserQuery : IRequest<Result<CurrentUserDto>>;

/// <summary>
/// Handles the current-user query.
/// </summary>
public sealed class GetCurrentUserQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository)
    : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserDto>>
{
    /// <summary>
    /// Resolves the current authenticated user from the request context.
    /// </summary>
    public async Task<Result<CurrentUserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId is null)
        {
            return Result<CurrentUserDto>.Failure("No authenticated user is associated with the current request.");
        }

        var user = await userRepository.GetByIdAsync(currentUserService.UserId.Value, cancellationToken);
        if (user is null)
        {
            return Result<CurrentUserDto>.Failure("The authenticated user could not be loaded from the data store.");
        }

        return Result<CurrentUserDto>.Success(
            new CurrentUserDto(
                user.Id,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                user.EmailConfirmed,
                currentUserService.IsAuthenticated,
                currentUserService.AuthenticationType,
                currentUserService.Roles));
    }
}
