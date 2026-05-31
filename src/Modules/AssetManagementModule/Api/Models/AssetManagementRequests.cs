using Alphabet.Domain.Enums;
using Alphabet.Domain.ValueObjects;

namespace Alphabet.Modules.AssetManagementModule.Api.Models;

/// <summary>
/// Represents a create-asset request body.
/// </summary>
public sealed class CreateAssetRequest
{
    public string? AssetTag { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public Guid CategoryId { get; init; }

    public string? Subcategory { get; init; }

    public string? Manufacturer { get; init; }

    public string? Model { get; init; }

    public string? SerialNumber { get; init; }

    public DateOnly? PurchaseDate { get; init; }

    public DateOnly? WarrantyExpiry { get; init; }

    public decimal Cost { get; init; }

    public string Currency { get; init; } = "USD";

    public AssetStatus Status { get; init; } = AssetStatus.Available;

    public AssetCondition Condition { get; init; } = AssetCondition.Good;

    public Guid LocationId { get; init; }

    public Guid? SupplierId { get; init; }

    public IReadOnlyDictionary<string, string>? CustomFields { get; init; }

    public IReadOnlyCollection<string>? Images { get; init; }

    public IReadOnlyCollection<string>? Documents { get; init; }
}

/// <summary>
/// Represents an update-asset request body.
/// </summary>
public sealed class UpdateAssetRequest
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public Guid CategoryId { get; init; }

    public string? Subcategory { get; init; }

    public string? Manufacturer { get; init; }

    public string? Model { get; init; }

    public string? SerialNumber { get; init; }

    public DateOnly? PurchaseDate { get; init; }

    public DateOnly? WarrantyExpiry { get; init; }

    public decimal Cost { get; init; }

    public string Currency { get; init; } = "USD";

    public AssetCondition Condition { get; init; } = AssetCondition.Good;

    public Guid LocationId { get; init; }

    public Guid? SupplierId { get; init; }

    public IReadOnlyDictionary<string, string>? CustomFields { get; init; }

    public IReadOnlyCollection<string>? Images { get; init; }

    public IReadOnlyCollection<string>? Documents { get; init; }
}

/// <summary>
/// Represents an asset retirement request body.
/// </summary>
public sealed class RetireAssetRequest
{
    public AssetStatus Status { get; init; } = AssetStatus.Retired;

    public string? Reason { get; init; }
}

/// <summary>
/// Represents an asset move request body.
/// </summary>
public sealed class MoveAssetRequest
{
    public Guid NewLocationId { get; init; }

    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Represents a category creation request body.
/// </summary>
public sealed class CreateAssetCategoryRequest
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public Guid? ParentCategoryId { get; init; }

    public IReadOnlyDictionary<string, string>? CustomFieldsSchema { get; init; }

    public decimal? DepreciationRate { get; init; }

    public Guid? DefaultLocationId { get; init; }
}

/// <summary>
/// Represents a location creation request body.
/// </summary>
public sealed class CreateLocationRequest
{
    public string Name { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;

    public AssetLocationType Type { get; init; } = AssetLocationType.Office;

    public Address Address { get; init; } = new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    public Guid? ParentLocationId { get; init; }

    public bool IsActive { get; init; } = true;

    public Coordinates? Coordinates { get; init; }

    public string? ContactPerson { get; init; }

    public string? ContactPhone { get; init; }
}

/// <summary>
/// Represents an asset assignment request body.
/// </summary>
public sealed class AssignAssetRequest
{
    public Guid AssignedToUserId { get; init; }

    public DateOnly? ExpectedReturnDate { get; init; }

    public AssetAssignmentType AssignmentType { get; init; } = AssetAssignmentType.Permanent;

    public string Purpose { get; init; } = string.Empty;

    public AssetCondition ConditionAtAssignment { get; init; } = AssetCondition.Good;

    public string? Notes { get; init; }
}

/// <summary>
/// Represents an asset return request body.
/// </summary>
public sealed class UnassignAssetRequest
{
    public Guid ReturnedByUserId { get; init; }

    public Guid ReceivedByUserId { get; init; }

    public AssetCondition ConditionOnReturn { get; init; } = AssetCondition.Good;

    public string? DamageNotes { get; init; }

    public IReadOnlyCollection<string>? MissingItems { get; init; }

    public DateOnly ActualReturnDate { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);
}

/// <summary>
/// Represents an asset transfer request body.
/// </summary>
public sealed class TransferAssetRequest
{
    public Guid FromUserId { get; init; }

    public Guid ToUserId { get; init; }

    public string Reason { get; init; } = string.Empty;

    public DateOnly TransferDate { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateOnly? ExpectedReturnDate { get; init; }
}

/// <summary>
/// Represents a maintenance scheduling request body.
/// </summary>
public sealed class ScheduleMaintenanceRequest
{
    public AssetMaintenanceType MaintenanceType { get; init; } = AssetMaintenanceType.Preventive;

    public DateOnly ScheduledDate { get; init; }

    public string Description { get; init; } = string.Empty;

    public string? AssignedToVendor { get; init; }

    public decimal EstimatedCost { get; init; }

    public AssetMaintenancePriority Priority { get; init; } = AssetMaintenancePriority.Medium;
}

/// <summary>
/// Represents a maintenance completion request body.
/// </summary>
public sealed class CompleteMaintenanceRequest
{
    public DateOnly CompletionDate { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public decimal ActualCost { get; init; }

    public string? Notes { get; init; }

    public DateOnly? NextMaintenanceDueDate { get; init; }
}

/// <summary>
/// Represents a workflow definition creation request body.
/// </summary>
public sealed class CreateWorkflowDefinitionRequest
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public int Version { get; init; } = 1;

    public IReadOnlyCollection<CreateWorkflowDefinitionStepRequest> Steps { get; init; } = Array.Empty<CreateWorkflowDefinitionStepRequest>();
}

/// <summary>
/// Represents a workflow definition step request body.
/// </summary>
public sealed class CreateWorkflowDefinitionStepRequest
{
    public string Name { get; init; } = string.Empty;

    public int Order { get; init; }

    public string AssignedToRole { get; init; } = string.Empty;

    public int RequiredApprovals { get; init; }

    public int TimeoutHours { get; init; }

    public IReadOnlyCollection<string> Actions { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Represents a start-workflow request body.
/// </summary>
public sealed class StartWorkflowRequest
{
    public Guid WorkflowDefinitionId { get; init; }

    public IReadOnlyDictionary<string, string>? Context { get; init; }
}

/// <summary>
/// Represents a workflow step action request body.
/// </summary>
public sealed class WorkflowStepActionRequest
{
    public string Action { get; init; } = string.Empty;

    public string? Comment { get; init; }
}

/// <summary>
/// Represents a stock adjustment request body.
/// </summary>
public sealed class StockAdjustmentRequest
{
    public Guid AssetId { get; init; }

    public Guid LocationId { get; init; }

    public StockAdjustmentType AdjustmentType { get; init; } = StockAdjustmentType.Add;

    public int Quantity { get; init; }

    public string Reason { get; init; } = string.Empty;

    public string? ReferenceNumber { get; init; }

    public int MinimumThreshold { get; init; } = 5;
}

/// <summary>
/// Represents a stock-take request body.
/// </summary>
public sealed class StockTakeRequest
{
    public Guid LocationId { get; init; }

    public IReadOnlyCollection<StockTakeCountedItemRequest> CountedItems { get; init; } = Array.Empty<StockTakeCountedItemRequest>();
}

/// <summary>
/// Represents a stock-take counted item request body.
/// </summary>
public sealed class StockTakeCountedItemRequest
{
    public Guid AssetId { get; init; }

    public int CountedQuantity { get; init; }

    public int ExpectedQuantity { get; init; }

    public int Discrepancy { get; init; }
}

/// <summary>
/// Represents an audit report generation request body.
/// </summary>
public sealed class GenerateAuditReportRequest
{
    public DateTimeOffset Start { get; init; }

    public DateTimeOffset End { get; init; }

    public string Format { get; init; } = "PDF";

    public bool IncludeDeleted { get; init; }
}
