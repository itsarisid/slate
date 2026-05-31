using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces.AssetManagement;
using Alphabet.Domain.Models;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Repositories.AssetManagement;

/// <summary>
/// EF Core persistence for asset management.
/// </summary>
public sealed class AssetRepository(AppDbContext dbContext) : IAssetRepository
{
    public Task<bool> AssetTagExistsAsync(string assetTag, Guid? excludeId, CancellationToken cancellationToken)
    {
        return dbContext.Set<Asset>()
            .AnyAsync(
                x => x.AssetTag == assetTag &&
                    (!excludeId.HasValue || x.Id != excludeId.Value),
                cancellationToken);
    }

    public Task<bool> SerialNumberExistsAsync(string serialNumber, Guid? excludeId, CancellationToken cancellationToken)
    {
        return dbContext.Set<Asset>()
            .AnyAsync(
                x => x.SerialNumber == serialNumber &&
                    (!excludeId.HasValue || x.Id != excludeId.Value),
                cancellationToken);
    }

    public async Task AddAssetAsync(Asset asset, CancellationToken cancellationToken)
        => await dbContext.Set<Asset>().AddAsync(asset, cancellationToken);

    public void UpdateAsset(Asset asset) => dbContext.Set<Asset>().Update(asset);

    public Task<Asset?> GetAssetByIdAsync(Guid assetId, CancellationToken cancellationToken)
        => dbContext.Set<Asset>().FirstOrDefaultAsync(x => x.Id == assetId, cancellationToken);

    public Task<Asset?> GetAssetByBarcodeAsync(string barcode, CancellationToken cancellationToken)
        => dbContext.Set<Asset>()
            .FirstOrDefaultAsync(
                x => x.AssetTag == barcode || x.BarcodePayload == barcode || x.QrCodePayload == barcode,
                cancellationToken);

    public async Task<AssetPagedResult<Asset>> GetAssetsAsync(AssetQueryFilter filter, CancellationToken cancellationToken)
    {
        IQueryable<Asset> query = dbContext.Set<Asset>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<AssetStatus>(filter.Status, true, out var status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
        }

        if (filter.LocationId.HasValue)
        {
            query = query.Where(x => x.LocationId == filter.LocationId.Value);
        }

        if (filter.AssignedToUserId.HasValue)
        {
            query = query.Where(x => x.AssignedToUserId == filter.AssignedToUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(x =>
                x.Name.Contains(search) ||
                x.Description.Contains(search) ||
                x.AssetTag.Contains(search) ||
                (x.SerialNumber != null && x.SerialNumber.Contains(search)));
        }

        if (filter.PurchaseDateFrom.HasValue)
        {
            query = query.Where(x => x.PurchaseDate >= filter.PurchaseDateFrom.Value);
        }

        if (filter.PurchaseDateTo.HasValue)
        {
            query = query.Where(x => x.PurchaseDate <= filter.PurchaseDateTo.Value);
        }

        if (filter.CostMin.HasValue)
        {
            query = query.Where(x => x.Cost >= filter.CostMin.Value);
        }

        if (filter.CostMax.HasValue)
        {
            query = query.Where(x => x.Cost <= filter.CostMax.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Condition) &&
            Enum.TryParse<AssetCondition>(filter.Condition, true, out var condition))
        {
            query = query.Where(x => x.Condition == condition);
        }

        if (filter.WarrantyExpiringInDays.HasValue)
        {
            var upper = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(filter.WarrantyExpiringInDays.Value);
            query = query.Where(x => x.WarrantyExpiry.HasValue && x.WarrantyExpiry.Value <= upper);
        }

        if (!filter.IncludeRetired)
        {
            query = query.Where(x => x.Status != AssetStatus.Retired && x.Status != AssetStatus.Disposed);
        }

        query = (filter.SortBy?.Trim().ToLowerInvariant(), filter.SortDirection?.Trim().ToLowerInvariant()) switch
        {
            ("purchasedate", "asc") => query.OrderBy(x => x.PurchaseDate),
            ("purchasedate", _) => query.OrderByDescending(x => x.PurchaseDate),
            ("cost", "asc") => query.OrderBy(x => x.Cost),
            ("cost", _) => query.OrderByDescending(x => x.Cost),
            ("name", "asc") => query.OrderBy(x => x.Name),
            ("name", _) => query.OrderByDescending(x => x.Name),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new AssetPagedResult<Asset>(items, filter.Page, filter.PageSize, totalCount);
    }

    public Task<IReadOnlyList<Asset>> GetAssetsByIdsAsync(IReadOnlyCollection<Guid> assetIds, CancellationToken cancellationToken)
        => dbContext.Set<Asset>()
            .Where(x => assetIds.Contains(x.Id))
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<Asset>)t.Result, cancellationToken);

    public async Task AddCategoryAsync(AssetCategory category, CancellationToken cancellationToken)
        => await dbContext.Set<AssetCategory>().AddAsync(category, cancellationToken);

    public Task<IReadOnlyList<AssetCategory>> GetCategoriesAsync(CancellationToken cancellationToken)
        => dbContext.Set<AssetCategory>().OrderBy(x => x.Name).ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<AssetCategory>)t.Result, cancellationToken);

    public Task<AssetCategory?> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken)
        => dbContext.Set<AssetCategory>().FirstOrDefaultAsync(x => x.Id == categoryId, cancellationToken);

    public async Task AddLocationAsync(Location location, CancellationToken cancellationToken)
        => await dbContext.Set<Location>().AddAsync(location, cancellationToken);

    public Task<IReadOnlyList<Location>> GetLocationsAsync(CancellationToken cancellationToken)
        => dbContext.Set<Location>().OrderBy(x => x.Name).ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<Location>)t.Result, cancellationToken);

    public Task<Location?> GetLocationByIdAsync(Guid locationId, CancellationToken cancellationToken)
        => dbContext.Set<Location>().FirstOrDefaultAsync(x => x.Id == locationId, cancellationToken);

    public async Task AddAssignmentAsync(AssetAssignment assignment, CancellationToken cancellationToken)
        => await dbContext.Set<AssetAssignment>().AddAsync(assignment, cancellationToken);

    public void UpdateAssignment(AssetAssignment assignment) => dbContext.Set<AssetAssignment>().Update(assignment);

    public Task<AssetAssignment?> GetActiveAssignmentAsync(Guid assetId, CancellationToken cancellationToken)
        => dbContext.Set<AssetAssignment>().FirstOrDefaultAsync(x => x.AssetId == assetId && x.IsActive, cancellationToken);

    public Task<IReadOnlyList<AssetAssignment>> GetAssignmentsAsync(Guid assetId, CancellationToken cancellationToken)
        => dbContext.Set<AssetAssignment>()
            .Where(x => x.AssetId == assetId)
            .OrderByDescending(x => x.AssignedAt)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<AssetAssignment>)t.Result, cancellationToken);

    public Task<IReadOnlyList<Asset>> GetAssignedAssetsAsync(Guid userId, CancellationToken cancellationToken)
        => dbContext.Set<Asset>()
            .Where(x => x.AssignedToUserId == userId && x.Status == AssetStatus.Assigned)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<Asset>)t.Result, cancellationToken);

    public async Task AddMovementAsync(AssetMovement movement, CancellationToken cancellationToken)
        => await dbContext.Set<AssetMovement>().AddAsync(movement, cancellationToken);

    public async Task AddMaintenanceAsync(AssetMaintenanceRecord maintenanceRecord, CancellationToken cancellationToken)
        => await dbContext.Set<AssetMaintenanceRecord>().AddAsync(maintenanceRecord, cancellationToken);

    public void UpdateMaintenance(AssetMaintenanceRecord maintenanceRecord)
        => dbContext.Set<AssetMaintenanceRecord>().Update(maintenanceRecord);

    public Task<AssetMaintenanceRecord?> GetMaintenanceByIdAsync(Guid assetId, Guid maintenanceId, CancellationToken cancellationToken)
        => dbContext.Set<AssetMaintenanceRecord>()
            .FirstOrDefaultAsync(x => x.AssetId == assetId && x.Id == maintenanceId, cancellationToken);

    public Task<IReadOnlyList<AssetMaintenanceRecord>> GetMaintenanceHistoryAsync(Guid assetId, CancellationToken cancellationToken)
        => dbContext.Set<AssetMaintenanceRecord>()
            .Where(x => x.AssetId == assetId)
            .OrderByDescending(x => x.ScheduledDate)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<AssetMaintenanceRecord>)t.Result, cancellationToken);

    public Task<IReadOnlyList<AssetMaintenanceRecord>> GetDueMaintenanceAsync(DateOnly dueOnOrBefore, CancellationToken cancellationToken)
        => dbContext.Set<AssetMaintenanceRecord>()
            .Where(x => !x.IsCompleted && x.ScheduledDate <= dueOnOrBefore)
            .OrderBy(x => x.ScheduledDate)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<AssetMaintenanceRecord>)t.Result, cancellationToken);

    public async Task AddWorkflowDefinitionAsync(AssetWorkflowDefinition definition, CancellationToken cancellationToken)
        => await dbContext.Set<AssetWorkflowDefinition>().AddAsync(definition, cancellationToken);

    public Task<AssetWorkflowDefinition?> GetWorkflowDefinitionByIdAsync(Guid definitionId, CancellationToken cancellationToken)
        => dbContext.Set<AssetWorkflowDefinition>().FirstOrDefaultAsync(x => x.Id == definitionId, cancellationToken);

    public Task<IReadOnlyList<AssetWorkflowDefinition>> GetWorkflowDefinitionsAsync(CancellationToken cancellationToken)
        => dbContext.Set<AssetWorkflowDefinition>()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<AssetWorkflowDefinition>)t.Result, cancellationToken);

    public async Task AddWorkflowInstanceAsync(AssetWorkflowInstance instance, CancellationToken cancellationToken)
        => await dbContext.Set<AssetWorkflowInstance>().AddAsync(instance, cancellationToken);

    public void UpdateWorkflowInstance(AssetWorkflowInstance instance) => dbContext.Set<AssetWorkflowInstance>().Update(instance);

    public Task<AssetWorkflowInstance?> GetWorkflowInstanceByIdAsync(Guid instanceId, CancellationToken cancellationToken)
        => dbContext.Set<AssetWorkflowInstance>().FirstOrDefaultAsync(x => x.Id == instanceId, cancellationToken);

    public async Task<IReadOnlyList<AssetWorkflowInstance>> GetPendingWorkflowInstancesAsync(Guid? delegateToUserId, IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken)
    {
        var items = await dbContext.Set<AssetWorkflowInstance>()
            .Where(x => x.Status == AssetWorkflowStatus.Active || x.Status == AssetWorkflowStatus.Escalated)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return items
            .Where(instance =>
            {
                var currentStep = instance.Steps.FirstOrDefault(x => x.StepId == instance.CurrentStepId);
                if (currentStep is null)
                {
                    return false;
                }

                return (delegateToUserId.HasValue && currentStep.DelegateToUserId == delegateToUserId.Value) ||
                    roleNames.Contains(currentStep.AssignedToRole);
            })
            .ToArray();
    }

    public async Task<IReadOnlyList<AssetWorkflowInstance>> GetOverdueWorkflowInstancesAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var items = await dbContext.Set<AssetWorkflowInstance>()
            .Where(x => x.Status == AssetWorkflowStatus.Active)
            .ToListAsync(cancellationToken);

        return items
            .Where(instance => instance.Steps.Any(step => step.StepId == instance.CurrentStepId && step.DueAt < now))
            .ToArray();
    }

    public async Task AddInventoryBalanceAsync(InventoryBalance balance, CancellationToken cancellationToken)
        => await dbContext.Set<InventoryBalance>().AddAsync(balance, cancellationToken);

    public void UpdateInventoryBalance(InventoryBalance balance) => dbContext.Set<InventoryBalance>().Update(balance);

    public Task<InventoryBalance?> GetInventoryBalanceAsync(Guid assetId, Guid locationId, CancellationToken cancellationToken)
        => dbContext.Set<InventoryBalance>()
            .FirstOrDefaultAsync(x => x.AssetId == assetId && x.LocationId == locationId, cancellationToken);

    public Task<IReadOnlyList<InventoryBalance>> GetLowStockBalancesAsync(CancellationToken cancellationToken)
        => dbContext.Set<InventoryBalance>()
            .Where(x => x.QuantityOnHand <= x.MinimumThreshold)
            .OrderBy(x => x.QuantityOnHand)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<InventoryBalance>)t.Result, cancellationToken);

    public Task<IReadOnlyList<InventoryBalance>> GetInventoryBalancesAsync(CancellationToken cancellationToken)
        => dbContext.Set<InventoryBalance>()
            .OrderBy(x => x.LocationId)
            .ThenBy(x => x.AssetId)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<InventoryBalance>)t.Result, cancellationToken);

    public async Task AddStockAdjustmentAsync(StockAdjustment adjustment, CancellationToken cancellationToken)
        => await dbContext.Set<StockAdjustment>().AddAsync(adjustment, cancellationToken);

    public async Task AddActivityAsync(AssetActivityLog activityLog, CancellationToken cancellationToken)
        => await dbContext.Set<AssetActivityLog>().AddAsync(activityLog, cancellationToken);

    public async Task<IReadOnlyList<AssetActivityLog>> GetActivityAsync(AssetActivityFilter filter, CancellationToken cancellationToken)
    {
        IQueryable<AssetActivityLog> query = dbContext.Set<AssetActivityLog>().AsQueryable();

        if (filter.AssetId.HasValue)
        {
            query = query.Where(x => x.AssetId == filter.AssetId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            query = query.Where(x => x.Action == filter.Action);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(x => x.Timestamp >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(x => x.Timestamp <= filter.ToDate.Value);
        }

        if (filter.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == filter.UserId.Value);
        }

        return await query
            .OrderByDescending(x => x.Timestamp)
            .Skip(filter.Skip)
            .Take(filter.Take)
            .ToListAsync(cancellationToken);
    }
}
