using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.AssetManagement;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Assets.Queries;

/// <summary>
/// Retrieves assets using advanced filtering.
/// </summary>
public sealed record GetAssetsQuery(
    string? Status,
    Guid? CategoryId,
    Guid? LocationId,
    Guid? AssignedToUserId,
    string? Search,
    DateOnly? PurchaseDateFrom,
    DateOnly? PurchaseDateTo,
    decimal? CostMin,
    decimal? CostMax,
    string? Condition,
    int? WarrantyExpiringInDays,
    bool IncludeRetired,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize) : IRequest<Result<AssetPagedResponseDto<AssetListItemDto>>>;

/// <summary>
/// Retrieves an asset by identifier.
/// </summary>
public sealed record GetAssetByIdQuery(Guid AssetId) : IRequest<Result<AssetDetailsDto>>;

/// <summary>
/// Scans an asset by barcode or QR code payload.
/// </summary>
public sealed record ScanAssetQuery(string Barcode) : IRequest<Result<AssetScanResultDto>>;

/// <summary>
/// Handles asset list queries.
/// </summary>
public sealed class GetAssetsQueryHandler(IAssetRepository assetRepository)
    : IRequestHandler<GetAssetsQuery, Result<AssetPagedResponseDto<AssetListItemDto>>>
{
    public async Task<Result<AssetPagedResponseDto<AssetListItemDto>>> Handle(GetAssetsQuery request, CancellationToken cancellationToken)
    {
        var filter = new AssetQueryFilter(
            request.Status,
            request.CategoryId,
            request.LocationId,
            request.AssignedToUserId,
            request.Search,
            request.PurchaseDateFrom,
            request.PurchaseDateTo,
            request.CostMin,
            request.CostMax,
            request.Condition,
            request.WarrantyExpiringInDays,
            request.IncludeRetired,
            request.SortBy,
            request.SortDirection,
            request.Page <= 0 ? 1 : request.Page,
            request.PageSize <= 0 ? 50 : request.PageSize);

        var results = await assetRepository.GetAssetsAsync(filter, cancellationToken);
        return Result<AssetPagedResponseDto<AssetListItemDto>>.Success(new AssetPagedResponseDto<AssetListItemDto>(
            results.Items.Select(x => x.ToListItemDto()).ToArray(),
            results.Page,
            results.PageSize,
            results.TotalCount));
    }
}

/// <summary>
/// Handles get-by-id asset queries.
/// </summary>
public sealed class GetAssetByIdQueryHandler(IAssetRepository assetRepository)
    : IRequestHandler<GetAssetByIdQuery, Result<AssetDetailsDto>>
{
    public async Task<Result<AssetDetailsDto>> Handle(GetAssetByIdQuery request, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetAssetByIdAsync(request.AssetId, cancellationToken);
        if (asset is null)
        {
            return Result<AssetDetailsDto>.Failure("Asset was not found.");
        }

        var assignments = await assetRepository.GetAssignmentsAsync(asset.Id, cancellationToken);
        var maintenanceHistory = await assetRepository.GetMaintenanceHistoryAsync(asset.Id, cancellationToken);
        var activity = await assetRepository.GetActivityAsync(
            new AssetActivityFilter(asset.Id, null, null, null, null, 200, 0),
            cancellationToken);

        return asset.ToDetailsDto(assignments, maintenanceHistory, activity);
    }
}

/// <summary>
/// Handles asset scan queries.
/// </summary>
public sealed class ScanAssetQueryHandler(IAssetRepository assetRepository)
    : IRequestHandler<ScanAssetQuery, Result<AssetScanResultDto>>
{
    public async Task<Result<AssetScanResultDto>> Handle(ScanAssetQuery request, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetAssetByBarcodeAsync(request.Barcode, cancellationToken);
        if (asset is null)
        {
            return Result<AssetScanResultDto>.Failure("No asset matched the provided scan payload.");
        }

        return Result<AssetScanResultDto>.Success(new AssetScanResultDto(
            asset.Id,
            asset.AssetTag,
            asset.Name,
            asset.Status.ToString(),
            asset.Condition.ToString(),
            asset.LocationId,
            asset.AssignedToUserId));
    }
}
