namespace Alphabet.Domain.Enums;

/// <summary>
/// Represents the current approval state of a privilege request.
/// </summary>
public enum PrivilegeRequestStatus
{
    Pending = 1,
    Approved = 2,
    Denied = 3,
    Expired = 4
}
