using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.AssetManagement;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Audit.Queries;

/// <summary>
/// Gets activity for a single asset.
/// </summary>
public sealed record GetAssetActivityQuery(
    Guid AssetId,
    string? Action,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    Guid? UserId) : IRequest<Result<IReadOnlyList<AssetActivityDto>>>;

/// <summary>
/// Gets global asset activity for administrators.
/// </summary>
public sealed record GetGlobalAssetActivityQuery(
    string? Action,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    Guid? UserId,
    int Take,
    int Skip) : IRequest<Result<IReadOnlyList<AssetActivityDto>>>;

/// <summary>
/// Handles asset activity queries.
/// </summary>
public sealed class GetAssetActivityQueryHandler(IAssetRepository assetRepository)
    : IRequestHandler<GetAssetActivityQuery, Result<IReadOnlyList<AssetActivityDto>>>
{
    public async Task<Result<IReadOnlyList<AssetActivityDto>>> Handle(GetAssetActivityQuery request, CancellationToken cancellationToken)
    {
        var items = await assetRepository.GetActivityAsync(
            new AssetActivityFilter(request.AssetId, request.Action, request.FromDate, request.ToDate, request.UserId, 500, 0),
            cancellationToken);

        return Result<IReadOnlyList<AssetActivityDto>>.Success(items.Select(x => x.ToActivityDto()).ToArray());
    }
}

/// <summary>
/// Handles global activity queries.
/// </summary>
public sealed class GetGlobalAssetActivityQueryHandler(IAssetRepository assetRepository)
    : IRequestHandler<GetGlobalAssetActivityQuery, Result<IReadOnlyList<AssetActivityDto>>>
{
    public async Task<Result<IReadOnlyList<AssetActivityDto>>> Handle(GetGlobalAssetActivityQuery request, CancellationToken cancellationToken)
    {
        var items = await assetRepository.GetActivityAsync(
            new AssetActivityFilter(null, request.Action, request.FromDate, request.ToDate, request.UserId, request.Take <= 0 ? 100 : request.Take, request.Skip < 0 ? 0 : request.Skip),
            cancellationToken);

        return Result<IReadOnlyList<AssetActivityDto>>.Success(items.Select(x => x.ToActivityDto()).ToArray());
    }
}
