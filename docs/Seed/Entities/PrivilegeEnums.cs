// PrivilegePolicyCondition and PrivilegeEffect are bare 2-value integer enums
// in swagger; PrivilegeAction is an 8-value one. Names below are best-guess —
// match the integer values against your real enums if they exist.

namespace Alphabet.Domain.Entities
{
    public enum PrivilegePolicyCondition
    {
        AllOf = 1, // user must hold ALL listed privileges
        AnyOf = 2  // user must hold ANY listed privilege
    }

    public enum PrivilegeEffect
    {
        Allow = 1,
        Deny = 2
    }

    public enum PrivilegeAction
    {
        Create = 1,
        Read = 2,
        Update = 3,
        Delete = 4,
        Approve = 5,
        Export = 6,
        Assign = 7,
        Manage = 8
    }

    public enum PrivilegeAccessRequestStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }
}
