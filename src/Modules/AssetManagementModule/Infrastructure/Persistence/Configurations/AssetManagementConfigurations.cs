using Alphabet.Domain.Entities;
using Alphabet.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alphabet.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the asset entity.
/// </summary>
public sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AssetTag).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.AssetTag).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.Subcategory).HasMaxLength(100);
        builder.Property(x => x.Manufacturer).HasMaxLength(100);
        builder.Property(x => x.Model).HasMaxLength(100);
        builder.Property(x => x.SerialNumber).HasMaxLength(100);
        builder.HasIndex(x => x.SerialNumber).IsUnique().HasFilter("[SerialNumber] IS NOT NULL");
        builder.Property(x => x.Cost).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Condition).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.CustomFieldsJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ImagesJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.DocumentsJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.QrCodePayload).HasColumnType("nvarchar(max)");
        builder.Property(x => x.BarcodePayload).HasColumnType("nvarchar(max)");
    }
}

/// <summary>
/// Configures asset categories.
/// </summary>
public sealed class AssetCategoryConfiguration : IEntityTypeConfiguration<AssetCategory>
{
    public void Configure(EntityTypeBuilder<AssetCategory> builder)
    {
        builder.ToTable("AssetCategories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.CustomFieldsSchemaJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.DepreciationRate).HasPrecision(6, 2);
    }
}

/// <summary>
/// Configures locations.
/// </summary>
public sealed class AssetLocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("AssetLocations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ContactPerson).HasMaxLength(200);
        builder.Property(x => x.ContactPhone).HasMaxLength(50);

        builder.OwnsOne(
            x => x.Address,
            address =>
            {
                address.Property(a => a.Street).HasColumnName("Street").HasMaxLength(200);
                address.Property(a => a.City).HasColumnName("City").HasMaxLength(100);
                address.Property(a => a.State).HasColumnName("State").HasMaxLength(100);
                address.Property(a => a.PostalCode).HasColumnName("PostalCode").HasMaxLength(20);
                address.Property(a => a.Country).HasColumnName("Country").HasMaxLength(100);
            });

        builder.OwnsOne(
            x => x.Coordinates,
            coordinates =>
            {
                coordinates.Property(c => c.Latitude).HasColumnName("Latitude").HasPrecision(10, 6);
                coordinates.Property(c => c.Longitude).HasColumnName("Longitude").HasPrecision(10, 6);
            });
    }
}

/// <summary>
/// Configures assignments.
/// </summary>
public sealed class AssetAssignmentConfiguration : IEntityTypeConfiguration<AssetAssignment>
{
    public void Configure(EntityTypeBuilder<AssetAssignment> builder)
    {
        builder.ToTable("AssetAssignments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AssignmentType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ConditionAtAssignment).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ConditionOnReturn).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Purpose).HasMaxLength(500);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.DamageNotes).HasMaxLength(2000);
        builder.Property(x => x.MissingItemsJson).HasColumnType("nvarchar(max)");
    }
}

/// <summary>
/// Configures asset movements.
/// </summary>
public sealed class AssetMovementConfiguration : IEntityTypeConfiguration<AssetMovement>
{
    public void Configure(EntityTypeBuilder<AssetMovement> builder)
    {
        builder.ToTable("AssetMovements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
    }
}

/// <summary>
/// Configures maintenance records.
/// </summary>
public sealed class AssetMaintenanceConfiguration : IEntityTypeConfiguration<AssetMaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<AssetMaintenanceRecord> builder)
    {
        builder.ToTable("AssetMaintenanceRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MaintenanceType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.AssignedToVendor).HasMaxLength(200);
        builder.Property(x => x.EstimatedCost).HasPrecision(18, 2);
        builder.Property(x => x.ActualCost).HasPrecision(18, 2);
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(4000);
    }
}

/// <summary>
/// Configures workflow definitions.
/// </summary>
public sealed class AssetWorkflowDefinitionConfiguration : IEntityTypeConfiguration<AssetWorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<AssetWorkflowDefinition> builder)
    {
        builder.ToTable("AssetWorkflowDefinitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.StepsJson).HasColumnType("nvarchar(max)");
    }
}

/// <summary>
/// Configures workflow instances.
/// </summary>
public sealed class AssetWorkflowInstanceConfiguration : IEntityTypeConfiguration<AssetWorkflowInstance>
{
    public void Configure(EntityTypeBuilder<AssetWorkflowInstance> builder)
    {
        builder.ToTable("AssetWorkflowInstances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ContextJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.StepsJson).HasColumnType("nvarchar(max)");
    }
}

/// <summary>
/// Configures inventory balances.
/// </summary>
public sealed class InventoryBalanceConfiguration : IEntityTypeConfiguration<InventoryBalance>
{
    public void Configure(EntityTypeBuilder<InventoryBalance> builder)
    {
        builder.ToTable("InventoryBalances");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.AssetId, x.LocationId }).IsUnique();
    }
}

/// <summary>
/// Configures stock adjustments.
/// </summary>
public sealed class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ToTable("StockAdjustments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AdjustmentType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
    }
}

/// <summary>
/// Configures asset activity logs.
/// </summary>
public sealed class AssetActivityLogConfiguration : IEntityTypeConfiguration<AssetActivityLog>
{
    public void Configure(EntityTypeBuilder<AssetActivityLog> builder)
    {
        builder.ToTable("AssetActivityLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OldValueJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.NewValueJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(1000);
        builder.Property(x => x.Reason).HasMaxLength(2000);
        builder.HasIndex(x => x.Timestamp);
    }
}

/// <summary>
/// Configures asset reservations.
/// </summary>
public sealed class AssetReservationConfiguration : IEntityTypeConfiguration<AssetReservation>
{
    public void Configure(EntityTypeBuilder<AssetReservation> builder)
    {
        builder.ToTable("AssetReservations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Purpose).HasMaxLength(1000);
    }
}
