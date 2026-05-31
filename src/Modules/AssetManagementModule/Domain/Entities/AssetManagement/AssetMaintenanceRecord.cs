using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents scheduled or completed maintenance for an asset.
/// </summary>
public sealed class AssetMaintenanceRecord : BaseEntity
{
    private AssetMaintenanceRecord()
    {
    }

    private AssetMaintenanceRecord(
        Guid assetId,
        AssetMaintenanceType maintenanceType,
        DateOnly scheduledDate,
        string description,
        string? assignedToVendor,
        decimal estimatedCost,
        AssetMaintenancePriority priority)
    {
        AssetId = assetId;
        MaintenanceType = maintenanceType;
        ScheduledDate = scheduledDate;
        Description = description.Trim();
        AssignedToVendor = string.IsNullOrWhiteSpace(assignedToVendor) ? null : assignedToVendor.Trim();
        EstimatedCost = decimal.Round(estimatedCost, 2, MidpointRounding.ToEven);
        Priority = priority;
    }

    public Guid AssetId { get; private set; }

    public AssetMaintenanceType MaintenanceType { get; private set; }

    public DateOnly ScheduledDate { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public string? AssignedToVendor { get; private set; }

    public decimal EstimatedCost { get; private set; }

    public AssetMaintenancePriority Priority { get; private set; }

    public DateOnly? CompletionDate { get; private set; }

    public decimal? ActualCost { get; private set; }

    public string? Notes { get; private set; }

    public DateOnly? NextMaintenanceDueDate { get; private set; }

    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Creates a maintenance schedule.
    /// </summary>
    public static AssetMaintenanceRecord Create(
        Guid assetId,
        AssetMaintenanceType maintenanceType,
        DateOnly scheduledDate,
        string description,
        string? assignedToVendor,
        decimal estimatedCost,
        AssetMaintenancePriority priority)
    {
        return new AssetMaintenanceRecord(assetId, maintenanceType, scheduledDate, description, assignedToVendor, estimatedCost, priority);
    }

    /// <summary>
    /// Completes the maintenance item.
    /// </summary>
    public void Complete(DateOnly completionDate, decimal actualCost, string? notes, DateOnly? nextMaintenanceDueDate)
    {
        CompletionDate = completionDate;
        ActualCost = decimal.Round(actualCost, 2, MidpointRounding.ToEven);
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        NextMaintenanceDueDate = nextMaintenanceDueDate;
        IsCompleted = true;
        Touch();
    }
}
