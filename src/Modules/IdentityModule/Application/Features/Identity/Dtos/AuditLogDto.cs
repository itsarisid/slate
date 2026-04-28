namespace Alphabet.Application.Features.Identity.Dtos;

/// <summary>
/// Represents a single audit-log entry returned by admin queries.
/// </summary>
public sealed record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string Action,
    bool Success,
    string Message,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset Timestamp);
