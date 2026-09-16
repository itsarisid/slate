namespace Alphabet.Application.Common.Interfaces.AssetManagement;

/// <summary>
/// Generates asset tags according to configured rules.
/// </summary>
public interface IAssetTagGenerator
{
    /// <summary>
    /// Generates the next asset tag.
    /// </summary>
    Task<string> GenerateAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Generates barcode and QR code payloads for assets.
/// </summary>
public interface IAssetBarcodeService
{
    /// <summary>
    /// Generates barcode payloads for the asset.
    /// </summary>
    AssetCodePayload GeneratePayload(Asset asset);
}

/// <summary>
/// Calculates depreciation values for assets.
/// </summary>
public interface IAssetDepreciationService
{
    /// <summary>
    /// Calculates depreciation for the asset.
    /// </summary>
    AssetDepreciationCalculation Calculate(Asset asset, decimal? depreciationRate, DateOnly? asOfDate);
}

/// <summary>
/// Resolves application users relevant to the asset management module.
/// </summary>
public interface IAssetUserDirectory
{
    /// <summary>
    /// Gets a user snapshot.
    /// </summary>
    Task<AssetUserSnapshot?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
}

/// <summary>
/// Sends asset management notifications.
/// </summary>
public interface IAssetNotificationService
{
    /// <summary>
    /// Sends an assignment confirmation notification.
    /// </summary>
    Task NotifyAssetAssignedAsync(Asset asset, AssetUserSnapshot assignee, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a return confirmation notification.
    /// </summary>
    Task NotifyAssetReturnedAsync(Asset asset, CancellationToken cancellationToken);

    /// <summary>
    /// Broadcasts workflow work assignment information.
    /// </summary>
    Task NotifyWorkflowStepAssignedAsync(AssetWorkflowInstance instance, string roleName, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a maintenance reminder notification.
    /// </summary>
    Task NotifyMaintenanceDueAsync(Asset asset, AssetMaintenanceRecord maintenanceRecord, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a low stock alert.
    /// </summary>
    Task NotifyLowStockAsync(Asset asset, int quantityOnHand, int minimumThreshold, CancellationToken cancellationToken);
}

/// <summary>
/// Represents generated asset code payloads.
/// </summary>
public sealed record AssetCodePayload(string? QrCodePayload, string? BarcodePayload);

/// <summary>
/// Represents a user snapshot needed by the module.
/// </summary>
public sealed record AssetUserSnapshot(
    Guid UserId,
    string Email,
    string DisplayName,
    bool IsActive,
    IReadOnlyCollection<string> Roles);

/// <summary>
/// Represents depreciation values for an asset.
/// </summary>
public sealed record AssetDepreciationCalculation(
    decimal OriginalCost,
    decimal CurrentValue,
    decimal DepreciationRate,
    string DepreciationMethod,
    decimal AccumulatedDepreciation,
    decimal SalvageValue,
    decimal DepreciationYtd,
    DateOnly AsOfDate);
