using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.AssetManagement;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Inventory.Queries;

/// <summary>
/// Gets low stock assets.
/// </summary>
public sealed record GetLowStockAssetsQuery() : IRequest<Result<IReadOnlyList<InventoryBalanceDto>>>;

/// <summary>
/// Gets the current stock report.
/// </summary>
public sealed record GetCurrentStockReportQuery() : IRequest<Result<IReadOnlyList<InventoryBalanceDto>>>;

/// <summary>
/// Handles low stock queries.
/// </summary>
public sealed class GetLowStockAssetsQueryHandler(IAssetRepository assetRepository)
    : IRequestHandler<GetLowStockAssetsQuery, Result<IReadOnlyList<InventoryBalanceDto>>>
{
    public async Task<Result<IReadOnlyList<InventoryBalanceDto>>> Handle(GetLowStockAssetsQuery request, CancellationToken cancellationToken)
    {
        var balances = await assetRepository.GetLowStockBalancesAsync(cancellationToken);
        return Result<IReadOnlyList<InventoryBalanceDto>>.Success(balances.Select(x => x.ToInventoryDto()).ToArray());
    }
}

/// <summary>
/// Handles stock report queries.
/// </summary>
public sealed class GetCurrentStockReportQueryHandler(IAssetRepository assetRepository)
    : IRequestHandler<GetCurrentStockReportQuery, Result<IReadOnlyList<InventoryBalanceDto>>>
{
    public async Task<Result<IReadOnlyList<InventoryBalanceDto>>> Handle(GetCurrentStockReportQuery request, CancellationToken cancellationToken)
    {
        var balances = await assetRepository.GetInventoryBalancesAsync(cancellationToken);
        return Result<IReadOnlyList<InventoryBalanceDto>>.Success(balances.Select(x => x.ToInventoryDto()).ToArray());
    }
}
