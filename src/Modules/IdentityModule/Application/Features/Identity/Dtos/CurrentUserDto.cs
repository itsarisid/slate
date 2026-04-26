namespace Alphabet.Application.Features.Identity.Dtos;

/// <summary>
/// Represents the currently authenticated user.
/// </summary>
public sealed record CurrentUserDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    bool EmailConfirmed,
    bool IsAuthenticated,
    string? AuthenticationType,
    IReadOnlyCollection<string> Roles);
