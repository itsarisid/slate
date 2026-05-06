namespace Alphabet.Application.Features.Privilege.Dtos;

/// <summary>
/// Represents the result of a single privilege evaluation.
/// </summary>
public sealed record PrivilegeCheckResultDto(
    bool HasPrivilege,
    string Source,
    DateTimeOffset? ExpiresAt,
    string? Reason);
