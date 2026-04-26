namespace Alphabet.Application.Features.Identity.Dtos;

/// <summary>
/// Represents a user projection returned by identity endpoints.
/// </summary>
public sealed record UserDto(Guid UserId, string Email, string FirstName, string LastName, bool EmailConfirmed);
