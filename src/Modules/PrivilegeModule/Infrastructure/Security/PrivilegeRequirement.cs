using Microsoft.AspNetCore.Authorization;

namespace Alphabet.Infrastructure.Security;

/// <summary>
/// Represents a privilege-based authorization requirement.
/// </summary>
public sealed class PrivilegeRequirement(IReadOnlyCollection<string> privileges, bool requireAll) : IAuthorizationRequirement
{
    public IReadOnlyCollection<string> Privileges { get; } = privileges;

    public bool RequireAll { get; } = requireAll;
}
