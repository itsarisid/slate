namespace Alphabet.Application.Features.Privilege.Dtos;

/// <summary>
/// Represents an evaluated privilege for a user together with its origin.
/// </summary>
public sealed record UserEffectivePrivilegeDto(
    string PrivilegeName,
    bool IsGranted,
    string Source,
    DateTimeOffset? ExpiresAt,
    string? Reason);
