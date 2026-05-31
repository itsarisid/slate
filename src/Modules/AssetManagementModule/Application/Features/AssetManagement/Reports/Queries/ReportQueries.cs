using Alphabet.Application.Common.Interfaces.AssetManagement;
using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces.AssetManagement;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Reports.Queries;

/// <summary>
/// Gets depreciation for a single asset.
/// </summary>
public sealed record GetAssetDepreciationQuery(Guid AssetId, DateOnly? AsOfDate) : IRequest<Result<AssetDepreciationDto>>;

/// <summary>
/// Gets asset utilization metrics.
/// </summary>
public sealed record GetAssetUtilizationReportQuery() : IRequest<Result<AssetReportSummaryDto>>;

/// <summary>
/// Gets asset lifecycle metrics.
/// </summary>
public sealed record GetAssetLifecycleReportQuery() : IRequest<Result<AssetReportSummaryDto>>;

/// <summary>
/// Gets compliance report metrics.
/// </summary>
public sealed record GetAssetComplianceReportQuery() : IRequest<Result<AssetReportSummaryDto>>;

/// <summary>
/// Handles depreciation queries.
/// </summary>
public sealed class GetAssetDepreciationQueryHandler(
    IAssetRepository assetRepository,
    IAssetDepreciationService depreciationService)
    : IRequestHandler<GetAssetDepreciationQuery, Result<AssetDepreciationDto>>
{
    public async Task<Result<AssetDepreciationDto>> Handle(GetAssetDepreciationQuery request, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetAssetByIdAsync(request.AssetId, cancellationToken);
        if (asset is null)
        {
            return Result<AssetDepreciationDto>.Failure("Asset was not found.");
        }

        var category = await assetRepository.GetCategoryByIdAsync(asset.CategoryId, cancellationToken);
        var calculation = depreciationService.Calculate(asset, category?.DepreciationRate, request.AsOfDate);
        return calculation.ToDto();
    }
}

/// <summary>
/// Handles utilization report queries.
/// </summary>
public sealed class GetAssetUtilizationReportQueryHandler(IAssetRepository assetRepository)
    : IRequestHandler<GetAssetUtilizationReportQuery, Result<AssetReportSummaryDto>>
{
    public async Task<Result<AssetReportSummaryDto>> Handle(GetAssetUtilizationReportQuery request, CancellationToken cancellationToken)
    {
        var assets = await assetRepository.GetAssetsAsync(new(null, null, null, null, null, null, null, null, null, null, null, true, "createdAt", "desc", 1, 5000), cancellationToken);
        var assigned = assets.Items.Count(x => x.Status == AssetStatus.Assigned);
        var idle = assets.Items.Count(x => x.Status == AssetStatus.Available);
        var utilizationRate = assets.TotalCount == 0 ? 0m : decimal.Round((decimal)assigned / assets.TotalCount * 100m, 2);

        return Result<AssetReportSummaryDto>.Success(new AssetReportSummaryDto(
            "Asset Utilization",
            new Dictionary<string, decimal>
            {
                ["totalAssets"] = assets.TotalCount,
                ["assignedAssets"] = assigned,
                ["idleAssets"] = idle,
                ["utilizationRate"] = utilizationRate
            },
            ["Use this report to identify underutilized or overbooked inventory."]));
    }
}

/// <summary>
/// Handles lifecycle report queries.
/// </summary>
public sealed class GetAssetLifecycleReportQueryHandler(IAssetRepository assetRepository)
    : IRequestHandler<GetAssetLifecycleReportQuery, Result<AssetReportSummaryDto>>
{
    public async Task<Result<AssetReportSummaryDto>> Handle(GetAssetLifecycleReportQuery request, CancellationToken cancellationToken)
    {
        var assets = await assetRepository.GetAssetsAsync(new(null, null, null, null, null, null, null, null, null, null, null, true, "createdAt", "desc", 1, 5000), cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var oneYear = assets.Items.Count(x => x.PurchaseDate.HasValue && x.PurchaseDate.Value >= today.AddYears(-1));
        var threeYears = assets.Items.Count(x => x.PurchaseDate.HasValue && x.PurchaseDate.Value < today.AddYears(-1) && x.PurchaseDate.Value >= today.AddYears(-3));
        var fiveYears = assets.Items.Count(x => x.PurchaseDate.HasValue && x.PurchaseDate.Value < today.AddYears(-3) && x.PurchaseDate.Value >= today.AddYears(-5));
        var legacy = assets.Items.Count(x => x.PurchaseDate.HasValue && x.PurchaseDate.Value < today.AddYears(-5));

        return Result<AssetReportSummaryDto>.Success(new AssetReportSummaryDto(
            "Asset Lifecycle",
            new Dictionary<string, decimal>
            {
                ["zeroToOneYear"] = oneYear,
                ["oneToThreeYears"] = threeYears,
                ["threeToFiveYears"] = fiveYears,
                ["fivePlusYears"] = legacy
            },
            ["Assets older than five years should be reviewed for replacement planning."]));
    }
}

/// <summary>
/// Handles compliance report queries.
/// </summary>
public sealed class GetAssetComplianceReportQueryHandler(IAssetRepository assetRepository)
    : IRequestHandler<GetAssetComplianceReportQuery, Result<AssetReportSummaryDto>>
{
    public async Task<Result<AssetReportSummaryDto>> Handle(GetAssetComplianceReportQuery request, CancellationToken cancellationToken)
    {
        var assets = await assetRepository.GetAssetsAsync(new(null, null, null, null, null, null, null, null, null, null, null, true, "createdAt", "desc", 1, 5000), cancellationToken);
        var maintenanceDue = await assetRepository.GetDueMaintenanceAsync(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14), cancellationToken);
        var unassigned = assets.Items.Count(x => x.Status == AssetStatus.Available);
        var pastReturn = assets.Items.Count(x => x.ExpectedReturnDate.HasValue && x.ExpectedReturnDate.Value < DateOnly.FromDateTime(DateTime.UtcNow));
        var noWarranty = assets.Items.Count(x => !x.WarrantyExpiry.HasValue);

        return Result<AssetReportSummaryDto>.Success(new AssetReportSummaryDto(
            "Asset Compliance",
            new Dictionary<string, decimal>
            {
                ["unassignedAssets"] = unassigned,
                ["pastExpectedReturn"] = pastReturn,
                ["assetsWithoutWarranty"] = noWarranty,
                ["maintenanceDueSoon"] = maintenanceDue.Count
            },
            ["Review overdue returns and due maintenance for compliance follow-up."]));
    }
}
