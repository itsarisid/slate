using Alphabet.Domain.Entities;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Interfaces.AssetManagement;

/// <summary>
/// Provides persistence access for asset management workflows.
/// </summary>
public interface IAssetRepository
{
    Task<bool> AssetTagExistsAsync(string assetTag, Guid? excludeId, CancellationToken cancellationToken);

    Task<bool> SerialNumberExistsAsync(string serialNumber, Guid? excludeId, CancellationToken cancellationToken);

    Task AddAssetAsync(Asset asset, CancellationToken cancellationToken);

    void UpdateAsset(Asset asset);

    Task<Asset?> GetAssetByIdAsync(Guid assetId, CancellationToken cancellationToken);

    Task<Asset?> GetAssetByBarcodeAsync(string barcode, CancellationToken cancellationToken);

    Task<AssetPagedResult<Asset>> GetAssetsAsync(AssetQueryFilter filter, CancellationToken cancellationToken);

    Task<IReadOnlyList<Asset>> GetAssetsByIdsAsync(IReadOnlyCollection<Guid> assetIds, CancellationToken cancellationToken);

    Task AddCategoryAsync(AssetCategory category, CancellationToken cancellationToken);

    Task<IReadOnlyList<AssetCategory>> GetCategoriesAsync(CancellationToken cancellationToken);

    Task<AssetCategory?> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken);

    Task AddLocationAsync(Location location, CancellationToken cancellationToken);

    Task<IReadOnlyList<Location>> GetLocationsAsync(CancellationToken cancellationToken);

    Task<Location?> GetLocationByIdAsync(Guid locationId, CancellationToken cancellationToken);

    Task AddAssignmentAsync(AssetAssignment assignment, CancellationToken cancellationToken);

    void UpdateAssignment(AssetAssignment assignment);

    Task<AssetAssignment?> GetActiveAssignmentAsync(Guid assetId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AssetAssignment>> GetAssignmentsAsync(Guid assetId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Asset>> GetAssignedAssetsAsync(Guid userId, CancellationToken cancellationToken);

    Task AddMovementAsync(AssetMovement movement, CancellationToken cancellationToken);

    Task AddMaintenanceAsync(AssetMaintenanceRecord maintenanceRecord, CancellationToken cancellationToken);

    void UpdateMaintenance(AssetMaintenanceRecord maintenanceRecord);

    Task<AssetMaintenanceRecord?> GetMaintenanceByIdAsync(Guid assetId, Guid maintenanceId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AssetMaintenanceRecord>> GetMaintenanceHistoryAsync(Guid assetId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AssetMaintenanceRecord>> GetDueMaintenanceAsync(DateOnly dueOnOrBefore, CancellationToken cancellationToken);

    Task AddWorkflowDefinitionAsync(AssetWorkflowDefinition definition, CancellationToken cancellationToken);

    Task<AssetWorkflowDefinition?> GetWorkflowDefinitionByIdAsync(Guid definitionId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AssetWorkflowDefinition>> GetWorkflowDefinitionsAsync(CancellationToken cancellationToken);

    Task AddWorkflowInstanceAsync(AssetWorkflowInstance instance, CancellationToken cancellationToken);

    void UpdateWorkflowInstance(AssetWorkflowInstance instance);

    Task<AssetWorkflowInstance?> GetWorkflowInstanceByIdAsync(Guid instanceId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AssetWorkflowInstance>> GetPendingWorkflowInstancesAsync(Guid? delegateToUserId, IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken);

    Task<IReadOnlyList<AssetWorkflowInstance>> GetOverdueWorkflowInstancesAsync(DateTimeOffset now, CancellationToken cancellationToken);

    Task AddInventoryBalanceAsync(InventoryBalance balance, CancellationToken cancellationToken);

    void UpdateInventoryBalance(InventoryBalance balance);

    Task<InventoryBalance?> GetInventoryBalanceAsync(Guid assetId, Guid locationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<InventoryBalance>> GetLowStockBalancesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<InventoryBalance>> GetInventoryBalancesAsync(CancellationToken cancellationToken);

    Task AddStockAdjustmentAsync(StockAdjustment adjustment, CancellationToken cancellationToken);

    Task AddActivityAsync(AssetActivityLog activityLog, CancellationToken cancellationToken);

    Task<IReadOnlyList<AssetActivityLog>> GetActivityAsync(AssetActivityFilter filter, CancellationToken cancellationToken);
}
