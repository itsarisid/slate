// Entities inferred from CreateLocationRequest, CreateAssetCategoryRequest,
// CreateAssetRequest/UpdateAssetRequest, AssignAssetRequest/UnassignAssetRequest/
// TransferAssetRequest, and ScheduleMaintenanceRequest/CompleteMaintenanceRequest
// in swagger.json. Reconcile against your real entities if they already exist.

using System;
using System.Collections.Generic;

namespace Alphabet.Domain.Entities
{
    public class Location
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string? Code { get; set; }
        public AssetLocationType Type { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public Guid? ParentLocationId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class AssetCategory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public double? DepreciationRate { get; set; } // e.g. 0.20 = 20%/yr
        public Guid? DefaultLocationId { get; set; }
    }

    public class Asset
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string AssetTag { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public string? Subcategory { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public DateOnly? WarrantyExpiry { get; set; }
        public double Cost { get; set; }
        public string? Currency { get; set; }
        public AssetStatus Status { get; set; }
        public AssetCondition Condition { get; set; }
        public Guid LocationId { get; set; }
        public Guid? SupplierId { get; set; }
        public Guid? AssignedToUserId { get; set; } // denormalized "current holder", nullable
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<string> Images { get; set; } = new();
        public List<string> Documents { get; set; } = new();
    }

    public class AssetAssignment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AssetId { get; set; }
        public Guid AssignedToUserId { get; set; }
        public DateOnly AssignedDate { get; set; }
        public DateOnly? ExpectedReturnDate { get; set; }
        public DateOnly? ActualReturnDate { get; set; }
        public AssetAssignmentType AssignmentType { get; set; }
        public string? Purpose { get; set; }
        public AssetCondition ConditionAtAssignment { get; set; }
        public AssetCondition? ConditionOnReturn { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } // true while asset is out with this user
    }

    public class AssetMaintenance
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AssetId { get; set; }
        public AssetMaintenanceType MaintenanceType { get; set; }
        public AssetMaintenancePriority Priority { get; set; }
        public DateOnly ScheduledDate { get; set; }
        public DateOnly? CompletionDate { get; set; }
        public string? Description { get; set; }
        public string? AssignedToVendor { get; set; }
        public double EstimatedCost { get; set; }
        public double? ActualCost { get; set; }
        public string? Notes { get; set; }
        public DateOnly? NextMaintenanceDueDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}
