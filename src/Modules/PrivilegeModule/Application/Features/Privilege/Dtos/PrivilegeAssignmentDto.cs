namespace Alphabet.Application.Features.Privilege.Dtos;

/// <summary>
/// Represents a privilege assignment returned for role or user inspection.
/// </summary>
public sealed record PrivilegeAssignmentDto(
    Guid PrivilegeId,
    string PrivilegeName,
    string DisplayName,
    string AssignmentSource,
    DateTimeOffset GrantedAt,
    string GrantedBy,
    DateTimeOffset? ExpiresAt,
    bool IsActive);
