using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.AssetManagement;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Maintenance.Queries;

/// <summary>
/// Gets maintenance history for an asset.
/// </summary>
public sealed record GetAssetMaintenanceHistoryQuery(Guid AssetId) : IRequest<Result<IReadOnlyList<AssetMaintenanceDto>>>;

/// <summary>
/// Handles maintenance history queries.
/// </summary>
public sealed class GetAssetMaintenanceHistoryQueryHandler(IAssetRepository assetRepository)
    : IRequestHandler<GetAssetMaintenanceHistoryQuery, Result<IReadOnlyList<AssetMaintenanceDto>>>
{
    public async Task<Result<IReadOnlyList<AssetMaintenanceDto>>> Handle(GetAssetMaintenanceHistoryQuery request, CancellationToken cancellationToken)
    {
        var items = await assetRepository.GetMaintenanceHistoryAsync(request.AssetId, cancellationToken);
        return Result<IReadOnlyList<AssetMaintenanceDto>>.Success(items.Select(x => x.ToMaintenanceDto()).ToArray());
    }
}
