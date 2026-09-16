using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.AssetManagement;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Assignments.Queries;

/// <summary>
/// Gets assignment history for an asset.
/// </summary>
public sealed record GetAssetAssignmentsQuery(Guid AssetId) : IRequest<Result<IReadOnlyList<AssetAssignmentDto>>>;

/// <summary>
/// Gets assets assigned to the current user.
/// </summary>
public sealed record GetMyAssignedAssetsQuery() : IRequest<Result<IReadOnlyList<AssetListItemDto>>>;

/// <summary>
/// Handles assignment history queries.
/// </summary>
public sealed class GetAssetAssignmentsQueryHandler(IAssetRepository assetRepository)
    : IRequestHandler<GetAssetAssignmentsQuery, Result<IReadOnlyList<AssetAssignmentDto>>>
{
    public async Task<Result<IReadOnlyList<AssetAssignmentDto>>> Handle(GetAssetAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var assignments = await assetRepository.GetAssignmentsAsync(request.AssetId, cancellationToken);
        return Result<IReadOnlyList<AssetAssignmentDto>>.Success(assignments.Select(x => x.ToAssignmentDto()).ToArray());
    }
}

/// <summary>
/// Handles my-assigned-assets queries.
/// </summary>
public sealed class GetMyAssignedAssetsQueryHandler(
    IAssetRepository assetRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetMyAssignedAssetsQuery, Result<IReadOnlyList<AssetListItemDto>>>
{
    public async Task<Result<IReadOnlyList<AssetListItemDto>>> Handle(GetMyAssignedAssetsQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
        {
            return Result<IReadOnlyList<AssetListItemDto>>.Failure("An authenticated user is required.");
        }

        var assets = await assetRepository.GetAssignedAssetsAsync(currentUserService.UserId.Value, cancellationToken);
        return Result<IReadOnlyList<AssetListItemDto>>.Success(assets.Select(x => x.ToListItemDto()).ToArray());
    }
}
