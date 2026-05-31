using Alphabet.Application.Common.Interfaces.AssetManagement;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Models;
using Alphabet.Domain.ValueObjects;

namespace Alphabet.Application.Features.AssetManagement.Dtos;

/// <summary>
/// Represents a paged response payload.
/// </summary>
public sealed record AssetPagedResponseDto<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

/// <summary>
/// Represents a lightweight asset summary.
/// </summary>
public sealed record AssetListItemDto(
    Guid Id,
    string AssetTag,
    string Name,
    string? Manufacturer,
    string? Model,
    decimal Cost,
    string Currency,
    string Status,
    string Condition,
    Guid CategoryId,
    Guid LocationId,
    Guid? AssignedToUserId,
    DateOnly? WarrantyExpiry);

/// <summary>
/// Represents a detailed asset response.
/// </summary>
public sealed record AssetDetailsDto(
    Guid Id,
    string AssetTag,
    string Name,
    string Description,
    Guid CategoryId,
    string? Subcategory,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    DateOnly? PurchaseDate,
    DateOnly? WarrantyExpiry,
    decimal Cost,
    string Currency,
    string Status,
    string Condition,
    Guid LocationId,
    Guid? SupplierId,
    IReadOnlyDictionary<string, string> CustomFields,
    IReadOnlyList<string> Images,
    IReadOnlyList<string> Documents,
    string? QrCodePayload,
    string? BarcodePayload,
    Guid? AssignedToUserId,
    DateTimeOffset? AssignedAt,
    DateOnly? ExpectedReturnDate,
    IReadOnlyList<AssetAssignmentDto> Assignments,
    IReadOnlyList<AssetMaintenanceDto> MaintenanceHistory,
    IReadOnlyList<AssetActivityDto> Activity);

/// <summary>
/// Represents asset assignment history.
/// </summary>
public sealed record AssetAssignmentDto(
    Guid Id,
    Guid AssetId,
    Guid AssignedToUserId,
    Guid AssignedByUserId,
    DateTimeOffset AssignedAt,
    DateOnly? ExpectedReturnDate,
    DateOnly? ActualReturnDate,
    string AssignmentType,
    string ConditionAtAssignment,
    string? ConditionOnReturn,
    string? Purpose,
    string? Notes,
    bool IsActive);

/// <summary>
/// Represents an asset maintenance record.
/// </summary>
public sealed record AssetMaintenanceDto(
    Guid Id,
    Guid AssetId,
    string MaintenanceType,
    DateOnly ScheduledDate,
    string Description,
    string? AssignedToVendor,
    decimal EstimatedCost,
    string Priority,
    DateOnly? CompletionDate,
    decimal? ActualCost,
    string? Notes,
    DateOnly? NextMaintenanceDueDate,
    bool IsCompleted);

/// <summary>
/// Represents a category tree item.
/// </summary>
public sealed record AssetCategoryTreeDto(
    Guid Id,
    string Name,
    string Description,
    decimal? DepreciationRate,
    Guid? DefaultLocationId,
    IReadOnlyList<AssetCategoryTreeDto> Children);

/// <summary>
/// Represents a location response.
/// </summary>
public sealed record AssetLocationDto(
    Guid Id,
    string Name,
    string Code,
    string Type,
    Address Address,
    Guid? ParentLocationId,
    bool IsActive,
    Coordinates? Coordinates,
    string? ContactPerson,
    string? ContactPhone);

/// <summary>
/// Represents a workflow definition response.
/// </summary>
public sealed record AssetWorkflowDefinitionDto(
    Guid Id,
    string Name,
    string Description,
    int Version,
    IReadOnlyList<AssetWorkflowStepDto> Steps);

/// <summary>
/// Represents a workflow instance response.
/// </summary>
public sealed record AssetWorkflowInstanceDto(
    Guid Id,
    Guid WorkflowDefinitionId,
    Guid AssetId,
    string Status,
    Guid? CurrentStepId,
    Guid InitiatedByUserId,
    DateTimeOffset InitiatedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<AssetWorkflowStepDto> Steps);

/// <summary>
/// Represents a workflow step response.
/// </summary>
public sealed record AssetWorkflowStepDto(
    Guid StepId,
    string Name,
    int Order,
    string AssignedToRole,
    IReadOnlyCollection<string> Actions,
    string Status,
    DateTimeOffset DueAt,
    Guid? DelegateToUserId,
    DateTimeOffset? CompletedAt,
    string? Action,
    string? Comment,
    Guid? PerformedByUserId);

/// <summary>
/// Represents an activity timeline record.
/// </summary>
public sealed record AssetActivityDto(
    Guid Id,
    Guid? AssetId,
    Guid? UserId,
    string Action,
    string? OldValueJson,
    string? NewValueJson,
    DateTimeOffset Timestamp,
    string? IpAddress,
    string? UserAgent,
    string? Reason);

/// <summary>
/// Represents an inventory snapshot row.
/// </summary>
public sealed record InventoryBalanceDto(
    Guid Id,
    Guid AssetId,
    Guid LocationId,
    int QuantityOnHand,
    int MinimumThreshold,
    DateTimeOffset? LastCountedAt);

/// <summary>
/// Represents a depreciation query response.
/// </summary>
public sealed record AssetDepreciationDto(
    decimal OriginalCost,
    decimal CurrentValue,
    decimal DepreciationRate,
    string DepreciationMethod,
    decimal AccumulatedDepreciation,
    decimal SalvageValue,
    decimal DepreciationYtd,
    DateOnly AsOfDate);

/// <summary>
/// Represents a generic report summary response.
/// </summary>
public sealed record AssetReportSummaryDto(
    string Title,
    IReadOnlyDictionary<string, decimal> Metrics,
    IReadOnlyCollection<string> Highlights);

/// <summary>
/// Represents a scan result response.
/// </summary>
public sealed record AssetScanResultDto(
    Guid AssetId,
    string AssetTag,
    string Name,
    string Status,
    string Condition,
    Guid LocationId,
    Guid? AssignedToUserId);

/// <summary>
/// Represents a simple mutation result.
/// </summary>
public sealed record AssetMutationResultDto(Guid Id, string Message);

/// <summary>
/// Maps domain entities to asset management DTOs.
/// </summary>
public static class AssetManagementMappings
{
    /// <summary>
    /// Maps an asset to a list item DTO.
    /// </summary>
    public static AssetListItemDto ToListItemDto(this Asset asset)
    {
        return new AssetListItemDto(
            asset.Id,
            asset.AssetTag,
            asset.Name,
            asset.Manufacturer,
            asset.Model,
            asset.Cost,
            asset.Currency,
            asset.Status.ToString(),
            asset.Condition.ToString(),
            asset.CategoryId,
            asset.LocationId,
            asset.AssignedToUserId,
            asset.WarrantyExpiry);
    }

    /// <summary>
    /// Maps an asset to a detailed DTO.
    /// </summary>
    public static AssetDetailsDto ToDetailsDto(
        this Asset asset,
        IReadOnlyList<AssetAssignment> assignments,
        IReadOnlyList<AssetMaintenanceRecord> maintenanceRecords,
        IReadOnlyList<AssetActivityLog> activityLogs)
    {
        return new AssetDetailsDto(
            asset.Id,
            asset.AssetTag,
            asset.Name,
            asset.Description,
            asset.CategoryId,
            asset.Subcategory,
            asset.Manufacturer,
            asset.Model,
            asset.SerialNumber,
            asset.PurchaseDate,
            asset.WarrantyExpiry,
            asset.Cost,
            asset.Currency,
            asset.Status.ToString(),
            asset.Condition.ToString(),
            asset.LocationId,
            asset.SupplierId,
            asset.CustomFields,
            asset.Images,
            asset.Documents,
            asset.QrCodePayload,
            asset.BarcodePayload,
            asset.AssignedToUserId,
            asset.AssignedAt,
            asset.ExpectedReturnDate,
            assignments.Select(ToAssignmentDto).ToArray(),
            maintenanceRecords.Select(ToMaintenanceDto).ToArray(),
            activityLogs.Select(ToActivityDto).ToArray());
    }

    /// <summary>
    /// Maps an assignment to a DTO.
    /// </summary>
    public static AssetAssignmentDto ToAssignmentDto(this AssetAssignment assignment)
    {
        return new AssetAssignmentDto(
            assignment.Id,
            assignment.AssetId,
            assignment.AssignedToUserId,
            assignment.AssignedByUserId,
            assignment.AssignedAt,
            assignment.ExpectedReturnDate,
            assignment.ActualReturnDate,
            assignment.AssignmentType.ToString(),
            assignment.ConditionAtAssignment.ToString(),
            assignment.ConditionOnReturn?.ToString(),
            assignment.Purpose,
            assignment.Notes,
            assignment.IsActive);
    }

    /// <summary>
    /// Maps a maintenance record to a DTO.
    /// </summary>
    public static AssetMaintenanceDto ToMaintenanceDto(this AssetMaintenanceRecord record)
    {
        return new AssetMaintenanceDto(
            record.Id,
            record.AssetId,
            record.MaintenanceType.ToString(),
            record.ScheduledDate,
            record.Description,
            record.AssignedToVendor,
            record.EstimatedCost,
            record.Priority.ToString(),
            record.CompletionDate,
            record.ActualCost,
            record.Notes,
            record.NextMaintenanceDueDate,
            record.IsCompleted);
    }

    /// <summary>
    /// Maps a workflow definition to a DTO.
    /// </summary>
    public static AssetWorkflowDefinitionDto ToDefinitionDto(this AssetWorkflowDefinition definition)
    {
        return new AssetWorkflowDefinitionDto(
            definition.Id,
            definition.Name,
            definition.Description,
            definition.Version,
            definition.Steps
                .OrderBy(x => x.Order)
                .Select(step => new AssetWorkflowStepDto(
                    step.Id,
                    step.Name,
                    step.Order,
                    step.AssignedToRole,
                    step.Actions,
                    "Defined",
                    DateTimeOffset.MinValue,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ToArray());
    }

    /// <summary>
    /// Maps a workflow instance to a DTO.
    /// </summary>
    public static AssetWorkflowInstanceDto ToInstanceDto(this AssetWorkflowInstance instance)
    {
        return new AssetWorkflowInstanceDto(
            instance.Id,
            instance.WorkflowDefinitionId,
            instance.AssetId,
            instance.Status.ToString(),
            instance.CurrentStepId,
            instance.InitiatedByUserId,
            instance.InitiatedAt,
            instance.CompletedAt,
            instance.Steps
                .OrderBy(x => x.Order)
                .Select(step => new AssetWorkflowStepDto(
                    step.StepId,
                    step.Name,
                    step.Order,
                    step.AssignedToRole,
                    step.AllowedActions,
                    step.Status.ToString(),
                    step.DueAt,
                    step.DelegateToUserId,
                    step.CompletedAt,
                    step.Action,
                    step.Comment,
                    step.PerformedByUserId))
                .ToArray());
    }

    /// <summary>
    /// Maps an activity log to a DTO.
    /// </summary>
    public static AssetActivityDto ToActivityDto(this AssetActivityLog activityLog)
    {
        return new AssetActivityDto(
            activityLog.Id,
            activityLog.AssetId,
            activityLog.UserId,
            activityLog.Action,
            activityLog.OldValueJson,
            activityLog.NewValueJson,
            activityLog.Timestamp,
            activityLog.IpAddress,
            activityLog.UserAgent,
            activityLog.Reason);
    }

    /// <summary>
    /// Maps an inventory balance to a DTO.
    /// </summary>
    public static InventoryBalanceDto ToInventoryDto(this InventoryBalance balance)
    {
        return new InventoryBalanceDto(
            balance.Id,
            balance.AssetId,
            balance.LocationId,
            balance.QuantityOnHand,
            balance.MinimumThreshold,
            balance.LastCountedAt);
    }

    /// <summary>
    /// Maps a depreciation calculation to a DTO.
    /// </summary>
    public static AssetDepreciationDto ToDto(this AssetDepreciationCalculation calculation)
    {
        return new AssetDepreciationDto(
            calculation.OriginalCost,
            calculation.CurrentValue,
            calculation.DepreciationRate,
            calculation.DepreciationMethod,
            calculation.AccumulatedDepreciation,
            calculation.SalvageValue,
            calculation.DepreciationYtd,
            calculation.AsOfDate);
    }
}
