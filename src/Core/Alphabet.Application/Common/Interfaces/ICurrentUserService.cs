namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Provides access to the current authenticated user context.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }

    string? Email { get; }

    IReadOnlyCollection<string> Roles { get; }

    string? IpAddress { get; }

    string? UserAgent { get; }
}
