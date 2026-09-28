// As with the Assets module: swagger.json only gives bare integer enums for
// LeaveAccrualMethod, LeaveDayPart, LeaveDelegationPermission, and
// LeaveDelegationType — names below are my best guess. Match the *integer
// values* against your real enums if they already exist.
//
// LeaveRequestStatus isn't an enum in swagger at all (leave requests are
// mutated via actions: Submit/Modify/Cancel/Reject/Approve), so I've added
// one to represent request state for seeding purposes — adjust to match
// however your domain models it.

namespace Alphabet.Domain.Entities
{
    public enum LeaveAccrualMethod
    {
        Monthly = 1,
        Annual = 2,
        PerPayPeriod = 3,
        Custom = 4
    }

    public enum LeaveDayPart
    {
        FullDay = 1,
        FirstHalf = 2,
        SecondHalf = 3
    }

    public enum LeaveDelegationType
    {
        FullDelegation = 1,
        ApprovalOnly = 2,
        ViewOnly = 3
    }

    public enum LeaveDelegationPermission
    {
        Approve = 1,
        Reject = 2,
        ApproveAndReject = 3
    }

    public enum LeaveRequestStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Cancelled = 4
    }
}
