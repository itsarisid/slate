using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.AssetManagement;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Categories.Queries;

/// <summary>
/// Gets the asset category hierarchy.
/// </summary>
public sealed record GetAssetCategoryTreeQuery() : IRequest<Result<IReadOnlyList<AssetCategoryTreeDto>>>;

/// <summary>
/// Handles category tree queries.
/// </summary>
public sealed class GetAssetCategoryTreeQueryHandler(IAssetRepository assetRepository)
    : IRequestHandler<GetAssetCategoryTreeQuery, Result<IReadOnlyList<AssetCategoryTreeDto>>>
{
    public async Task<Result<IReadOnlyList<AssetCategoryTreeDto>>> Handle(GetAssetCategoryTreeQuery request, CancellationToken cancellationToken)
    {
        var categories = await assetRepository.GetCategoriesAsync(cancellationToken);
        var lookup = categories.ToLookup(x => x.ParentCategoryId);

        IReadOnlyList<AssetCategoryTreeDto> Build(Guid? parentId)
        {
            return lookup[parentId]
                .OrderBy(x => x.Name)
                .Select(x => new AssetCategoryTreeDto(
                    x.Id,
                    x.Name,
                    x.Description,
                    x.DepreciationRate,
                    x.DefaultLocationId,
                    Build(x.Id)))
                .ToArray();
        }

        return Result<IReadOnlyList<AssetCategoryTreeDto>>.Success(Build(null));
    }
}
