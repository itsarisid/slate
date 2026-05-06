namespace Alphabet.Domain.Enums;

/// <summary>
/// Describes the audited action performed in the privilege subsystem.
/// </summary>
public enum PrivilegeAction
{
    Assign = 1,
    Revoke = 2,
    Check = 3,
    Create = 4,
    Update = 5,
    Delete = 6,
    Approve = 7,
    Deny = 8
}
