using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.AssetManagement;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces.AssetManagement;
using Alphabet.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alphabet.Infrastructure.Services.AssetManagement;

/// <summary>
/// Generates asset tags.
/// </summary>
public sealed class AssetTagGenerator(
    IOptions<AssetManagementSettings> options,
    IAssetRepository assetRepository) : IAssetTagGenerator
{
    private readonly AssetManagementSettings _settings = options.Value;

    public async Task<string> GenerateAsync(CancellationToken cancellationToken)
    {
        var random = Random.Shared;
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var number = random.NextInt64(1, (long)Math.Pow(10, _settings.AssetTagDigits));
            var tag = $"{_settings.AssetTagPrefix}-{number.ToString().PadLeft(_settings.AssetTagDigits, '0')}";
            if (!await assetRepository.AssetTagExistsAsync(tag, null, cancellationToken))
            {
                return tag;
            }
        }

        return $"{_settings.AssetTagPrefix}-{Guid.NewGuid():N}"[..(_settings.AssetTagPrefix.Length + 9)];
    }
}

/// <summary>
/// Generates barcode payloads for assets.
/// </summary>
public sealed class AssetBarcodeService(IOptions<AssetBarcodeSettings> options) : IAssetBarcodeService
{
    private readonly AssetBarcodeSettings _settings = options.Value;

    public AssetCodePayload GeneratePayload(Asset asset)
    {
        var basePayload = _settings.IncludeUrl
            ? $"{_settings.BaseUrl.TrimEnd('/')}/api/v1/assets/{asset.Id}"
            : asset.AssetTag;

        var qrPayload = _settings.Format.Equals("QRCode", StringComparison.OrdinalIgnoreCase) ? basePayload : null;
        return new AssetCodePayload(qrPayload, asset.AssetTag);
    }
}

/// <summary>
/// Calculates depreciation for assets.
/// </summary>
public sealed class AssetDepreciationService(IOptions<AssetDepreciationSettings> options) : IAssetDepreciationService
{
    private readonly AssetDepreciationSettings _settings = options.Value;

    public AssetDepreciationCalculation Calculate(Asset asset, decimal? depreciationRate, DateOnly? asOfDate)
    {
        var rate = depreciationRate ?? _settings.DefaultRate;
        var asOf = asOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var purchaseDate = asset.PurchaseDate ?? asOf;
        var ageInYears = Math.Max(0m, (decimal)(asOf.DayNumber - purchaseDate.DayNumber) / 365m);
        var salvageValue = decimal.Round(asset.Cost * (_settings.SalvagePercentage / 100m), 2, MidpointRounding.ToEven);
        var depreciableBase = Math.Max(0m, asset.Cost - salvageValue);
        var accumulatedDepreciation = decimal.Round(Math.Min(depreciableBase, depreciableBase * (rate / 100m) * ageInYears), 2, MidpointRounding.ToEven);
        var currentValue = decimal.Round(Math.Max(salvageValue, asset.Cost - accumulatedDepreciation), 2, MidpointRounding.ToEven);
        var depreciationYtd = decimal.Round(depreciableBase * (rate / 100m), 2, MidpointRounding.ToEven);

        return new AssetDepreciationCalculation(
            asset.Cost,
            currentValue,
            rate,
            _settings.DefaultMethod,
            accumulatedDepreciation,
            salvageValue,
            depreciationYtd,
            asOf);
    }
}

/// <summary>
/// Resolves user directory details for asset operations.
/// </summary>
public sealed class AssetUserDirectory(UserManager<ApplicationUser> userManager) : IAssetUserDirectory
{
    public async Task<AssetUserSnapshot?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return new AssetUserSnapshot(
            user.Id,
            user.Email ?? string.Empty,
            $"{user.FirstName} {user.LastName}".Trim(),
            !user.LockoutEnabled || !user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.UtcNow,
            roles.ToArray());
    }
}

/// <summary>
/// Sends module-specific asset notifications through configured channels.
/// </summary>
public sealed class AssetNotificationService(
    ICommunicationService communicationService,
    ILogger<AssetNotificationService> logger) : IAssetNotificationService
{
    public async Task NotifyAssetAssignedAsync(Asset asset, AssetUserSnapshot assignee, CancellationToken cancellationToken)
    {
        await communicationService.SendAsync(
            new CommunicationDispatchRequest(
                $"Asset assigned: {asset.AssetTag}",
                $"Asset '{asset.Name}' ({asset.AssetTag}) has been assigned to you.",
                ["Email", "InApp"],
                assignee.Email,
                null,
                assignee.UserId.ToString(),
                null,
                null,
                false),
            cancellationToken);
    }

    public async Task NotifyAssetReturnedAsync(Asset asset, CancellationToken cancellationToken)
    {
        logger.LogInformation("Asset {AssetTag} return recorded.", asset.AssetTag);
        await Task.CompletedTask;
    }

    public async Task NotifyWorkflowStepAssignedAsync(AssetWorkflowInstance instance, string roleName, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Workflow step assignment notification queued for workflow {WorkflowId} and role {RoleName}.",
            instance.Id,
            roleName);

        await Task.CompletedTask;
    }

    public async Task NotifyMaintenanceDueAsync(Asset asset, AssetMaintenanceRecord maintenanceRecord, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Maintenance reminder queued for asset {AssetTag} and maintenance record {MaintenanceId}.",
            asset.AssetTag,
            maintenanceRecord.Id);

        await Task.CompletedTask;
    }

    public async Task NotifyLowStockAsync(Asset asset, int quantityOnHand, int minimumThreshold, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Low stock alert for asset {AssetTag}. QuantityOnHand={QuantityOnHand}, MinimumThreshold={MinimumThreshold}.",
            asset.AssetTag,
            quantityOnHand,
            minimumThreshold);

        await Task.CompletedTask;
    }
}
