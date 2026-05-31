namespace Alphabet.Domain.Enums;

/// <summary>
/// Represents the current asset lifecycle status.
/// </summary>
public enum AssetStatus
{
    Available = 1,
    Assigned = 2,
    Reserved = 3,
    UnderRepair = 4,
    Retired = 5,
    Lost = 6,
    Disposed = 7
}

/// <summary>
/// Represents the physical condition of an asset.
/// </summary>
public enum AssetCondition
{
    New = 1,
    Good = 2,
    Fair = 3,
    Poor = 4,
    Damaged = 5
}

/// <summary>
/// Represents a location type for storing or assigning assets.
/// </summary>
public enum AssetLocationType
{
    Office = 1,
    Warehouse = 2,
    Datacenter = 3,
    Storage = 4,
    Remote = 5
}

/// <summary>
/// Represents how an asset is assigned.
/// </summary>
public enum AssetAssignmentType
{
    Permanent = 1,
    Temporary = 2,
    Loan = 3,
    CheckOut = 4
}

/// <summary>
/// Represents a workflow instance status.
/// </summary>
public enum AssetWorkflowStatus
{
    Active = 1,
    Completed = 2,
    Cancelled = 3,
    Escalated = 4,
    Rejected = 5
}

/// <summary>
/// Represents the status of a workflow step.
/// </summary>
public enum AssetWorkflowStepStatus
{
    Pending = 1,
    Completed = 2,
    Rejected = 3,
    Delegated = 4,
    Escalated = 5
}

/// <summary>
/// Represents the type of maintenance being scheduled.
/// </summary>
public enum AssetMaintenanceType
{
    Preventive = 1,
    Corrective = 2,
    Emergency = 3
}

/// <summary>
/// Represents the priority of maintenance work.
/// </summary>
public enum AssetMaintenancePriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Represents inventory adjustments.
/// </summary>
public enum StockAdjustmentType
{
    Add = 1,
    Remove = 2,
    Set = 3
}

/// <summary>
/// Represents supported barcode or QR payload formats.
/// </summary>
public enum BarcodeFormat
{
    QRCode = 1,
    Code128 = 2,
    DataMatrix = 3
}
