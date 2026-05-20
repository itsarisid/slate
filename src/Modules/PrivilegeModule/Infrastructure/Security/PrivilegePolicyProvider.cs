using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Alphabet.Infrastructure.Security;

/// <summary>
/// Dynamically builds privilege-based authorization policies.
/// </summary>
public sealed class PrivilegePolicyProvider(IOptions<AuthorizationOptions> options)
    : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackProvider = new(options);
    /// <summary>
    /// Get default policy async.
    /// </summary>

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackProvider.GetDefaultPolicyAsync();
    /// <summary>
    /// Get fallback policy async.
    /// </summary>

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackProvider.GetFallbackPolicyAsync();
    /// <summary>
    /// Get policy async.
    /// </summary>

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith("Privilege:", StringComparison.OrdinalIgnoreCase))
        {
            return _fallbackProvider.GetPolicyAsync(policyName);
        }

        var pieces = policyName.Split(':', 3, StringSplitOptions.TrimEntries);
        if (pieces.Length < 3)
        {
            return Task.FromResult<AuthorizationPolicy?>(null);
        }

        var privileges = pieces[1]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(static privilege => privilege.Trim())
            .Where(static privilege => !string.IsNullOrWhiteSpace(privilege))
            .ToArray();

        var requireAll = bool.TryParse(pieces[2], out var parsedRequireAll) && parsedRequireAll;

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PrivilegeRequirement(privileges, requireAll))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
