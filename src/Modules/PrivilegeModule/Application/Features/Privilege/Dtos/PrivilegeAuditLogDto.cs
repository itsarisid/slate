namespace Alphabet.Application.Features.Privilege.Dtos;

/// <summary>
/// Represents an audit entry for privilege-related operations.
/// </summary>
public sealed record PrivilegeAuditLogDto(
    Guid Id,
    Guid? UserId,
    Guid? PrivilegeId,
    string Action,
    string Source,
    string PerformedBy,
    DateTimeOffset PerformedAt,
    string? IpAddress,
    IReadOnlyDictionary<string, string?> Metadata);
