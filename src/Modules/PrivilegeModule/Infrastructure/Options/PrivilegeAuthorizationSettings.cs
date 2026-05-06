namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Configuration for privilege-based authorization behavior.
/// </summary>
public sealed class PrivilegeAuthorizationSettings
{
    public const string SectionName = "Authorization";

    public string DefaultPolicy { get; init; } = "PrivilegeBased";

    public bool FallbackToRoles { get; init; } = true;
}
