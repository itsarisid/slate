using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces.AssetManagement;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Categories.Commands;

/// <summary>
/// Creates a new asset category.
/// </summary>
public sealed record CreateAssetCategoryCommand(
    string Name,
    string Description,
    Guid? ParentCategoryId,
    IReadOnlyDictionary<string, string>? CustomFieldsSchema,
    decimal? DepreciationRate,
    Guid? DefaultLocationId) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Handles category creation.
/// </summary>
public sealed class CreateAssetCategoryCommandHandler(
    IAssetRepository assetRepository,
    Alphabet.Domain.Interfaces.IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAssetCategoryCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(CreateAssetCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = AssetCategory.Create(
            request.Name,
            request.Description,
            request.ParentCategoryId,
            Alphabet.Domain.Models.AssetManagementJson.Serialize(request.CustomFieldsSchema ?? new Dictionary<string, string>()),
            request.DepreciationRate,
            request.DefaultLocationId);

        await assetRepository.AddCategoryAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(category.Id, $"Category '{category.Name}' created successfully."));
    }
}
