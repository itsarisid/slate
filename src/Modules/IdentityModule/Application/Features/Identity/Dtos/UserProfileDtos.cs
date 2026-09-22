namespace Alphabet.Application.Features.Identity.Dtos;

/// <summary>Represents the editable profile of the authenticated user.</summary>
public sealed record UserProfileDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? Bio,
    string? Department,
    string? Location,
    string? AvatarUrl);

/// <summary>Represents a saved UI preference set.</summary>
public sealed record UserPreferencesDto(string? Theme, bool EmailNotifications, bool PushNotifications, string? Timezone);

/// <summary>Represents an authenticated refresh-token session.</summary>
public sealed record UserSessionDto(Guid Id, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt, string? IpAddress, bool IsCurrent);

/// <summary>Represents the authenticated user's audit history and active network locations.</summary>
public sealed record UserActivityDto(IReadOnlyList<AuditLogDto> Events, IReadOnlyList<string> ActiveIpAddresses);

/// <summary>Represents a self-service user profile update.</summary>
public sealed record UpdateUserProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? Bio,
    string? Department,
    string? Location);

/// <summary>Represents the user's saved interface preferences.</summary>
public sealed record UpdateUserPreferencesRequest(
    string? Theme,
    bool EmailNotifications,
    bool PushNotifications,
    string? Timezone);
