namespace Alphabet.Domain.Enums;

/// <summary>
/// Represents the state of a leave request.
/// </summary>
public enum LeaveRequestStatus
{
    Draft = 1,
    Pending = 2,
    ChangesRequested = 3,
    Approved = 4,
    Rejected = 5,
    Cancelled = 6,
    Withdrawn = 7
}

/// <summary>
/// Represents partial-day leave selection.
/// </summary>
public enum LeaveDayPart
{
    Full = 1,
    Morning = 2,
    Afternoon = 3
}

/// <summary>
/// Represents an approver resolution type.
/// </summary>
public enum LeaveApproverType
{
    DirectManager = 1,
    RoleBased = 2,
    SpecificUser = 3,
    DepartmentHead = 4,
    Hr = 5,
    Custom = 6
}

/// <summary>
/// Represents approval workflow state.
/// </summary>
public enum LeaveWorkflowStatus
{
    Active = 1,
    Completed = 2,
    Rejected = 3,
    Cancelled = 4,
    Escalated = 5
}

/// <summary>
/// Represents an individual approval step state.
/// </summary>
public enum LeaveWorkflowStepStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    ChangesRequested = 4,
    Delegated = 5,
    Escalated = 6,
    Skipped = 7
}

/// <summary>
/// Represents delegation duration and scope.
/// </summary>
public enum LeaveDelegationType
{
    Temporary = 1,
    Permanent = 2,
    Partial = 3
}

/// <summary>
/// Represents delegation permission.
/// </summary>
public enum LeaveDelegationPermission
{
    Full = 1,
    ApproveOnly = 2,
    ViewOnly = 3
}

/// <summary>
/// Represents accrual calculation cadence.
/// </summary>
public enum LeaveAccrualMethod
{
    Monthly = 1,
    Daily = 2,
    Yearly = 3,
    TenureBased = 4
}
