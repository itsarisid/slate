using Microsoft.AspNetCore.Authorization;

namespace Alphabet.Application.Common.Security;

/// <summary>
/// Declares privilege-based authorization metadata for endpoints or handlers.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class PrivilegeAuthorizeAttribute : AuthorizeAttribute
{
    public PrivilegeAuthorizeAttribute(string privileges, bool requireAll = false)
    {
        Privileges = privileges;
        RequireAll = requireAll;
        Policy = $"Privilege:{Privileges}:{RequireAll}";
    }

    public string Privileges { get; }

    public bool RequireAll { get; }
}
