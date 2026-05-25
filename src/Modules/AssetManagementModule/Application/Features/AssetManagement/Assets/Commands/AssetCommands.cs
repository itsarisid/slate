using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.AssetManagement;
using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.AssetManagement;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Assets.Commands;

/// <summary>
/// Creates a new asset.
/// </summary>
public sealed record CreateAssetCommand(
    string? AssetTag,
    string Name,
    string Description,
    Guid CategoryId,
    string? Subcategory,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    DateOnly? PurchaseDate,
    DateOnly? WarrantyExpiry,
    decimal Cost,
    string Currency,
    AssetStatus Status,
    AssetCondition Condition,
    Guid LocationId,
    Guid? SupplierId,
    IReadOnlyDictionary<string, string>? CustomFields,
    IReadOnlyCollection<string>? Images,
    IReadOnlyCollection<string>? Documents) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Updates an existing asset.
/// </summary>
public sealed record UpdateAssetCommand(
    Guid AssetId,
    string Name,
    string Description,
    Guid CategoryId,
    string? Subcategory,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    DateOnly? PurchaseDate,
    DateOnly? WarrantyExpiry,
    decimal Cost,
    string Currency,
    AssetCondition Condition,
    Guid LocationId,
    Guid? SupplierId,
    IReadOnlyDictionary<string, string>? CustomFields,
    IReadOnlyCollection<string>? Images,
    IReadOnlyCollection<string>? Documents) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Retires or disposes an asset.
/// </summary>
public sealed record RetireAssetCommand(Guid AssetId, AssetStatus Status, string? Reason) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Moves an asset to a new location.
/// </summary>
public sealed record MoveAssetCommand(Guid AssetId, Guid NewLocationId, string Reason) : IRequest<Result<AssetMutationResultDto>>;

internal static class AssetCommandHelpers
{
    public static Guid RequireCurrentUserId(ICurrentUserService currentUserService)
    {
        return currentUserService.UserId ?? throw new InvalidOperationException("An authenticated user is required for this operation.");
    }

    public static async Task AddActivityAsync(
        IAssetRepository assetRepository,
        ICurrentUserService currentUserService,
        Guid? assetId,
        string action,
        string? oldValueJson,
        string? newValueJson,
        string? reason,
        CancellationToken cancellationToken)
    {
        await assetRepository.AddActivityAsync(
            AssetActivityLog.Create(
                assetId,
                currentUserService.UserId,
                action,
                oldValueJson,
                newValueJson,
                currentUserService.IpAddress,
                currentUserService.UserAgent,
                reason),
            cancellationToken);
    }
}

/// <summary>
/// Handles asset creation.
/// </summary>
public sealed class CreateAssetCommandHandler(
    IAssetRepository assetRepository,
    IAssetTagGenerator assetTagGenerator,
    IAssetBarcodeService assetBarcodeService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAssetCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        var assetTag = string.IsNullOrWhiteSpace(request.AssetTag)
            ? await assetTagGenerator.GenerateAsync(cancellationToken)
            : request.AssetTag.Trim().ToUpperInvariant();

        if (await assetRepository.AssetTagExistsAsync(assetTag, null, cancellationToken))
        {
            return Result<AssetMutationResultDto>.Failure($"Asset tag '{assetTag}' already exists.");
        }

        if (!string.IsNullOrWhiteSpace(request.SerialNumber) &&
            await assetRepository.SerialNumberExistsAsync(request.SerialNumber, null, cancellationToken))
        {
            return Result<AssetMutationResultDto>.Failure($"Serial number '{request.SerialNumber}' already exists.");
        }

        var asset = Asset.Create(
            assetTag,
            request.Name,
            request.Description,
            request.CategoryId,
            request.Subcategory,
            request.Manufacturer,
            request.Model,
            request.SerialNumber,
            request.PurchaseDate,
            request.WarrantyExpiry,
            request.Cost,
            request.Currency,
            request.Status,
            request.Condition,
            request.LocationId,
            request.SupplierId,
            request.CustomFields,
            request.Images,
            request.Documents,
            null,
            null);

        var codes = assetBarcodeService.GeneratePayload(asset);
        asset.SetCodes(codes.QrCodePayload, codes.BarcodePayload);

        await assetRepository.AddAssetAsync(asset, cancellationToken);
        await AssetCommandHelpers.AddActivityAsync(
            assetRepository,
            currentUserService,
            asset.Id,
            "Create",
            null,
            AssetManagementJson.Serialize(asset.ToListItemDto()),
            "Asset created",
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(asset.Id, $"Asset '{asset.AssetTag}' created successfully."));
    }
}

/// <summary>
/// Handles asset updates.
/// </summary>
public sealed class UpdateAssetCommandHandler(
    IAssetRepository assetRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAssetCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetAssetByIdAsync(request.AssetId, cancellationToken);
        if (asset is null)
        {
            return Result<AssetMutationResultDto>.Failure("Asset was not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.SerialNumber) &&
            await assetRepository.SerialNumberExistsAsync(request.SerialNumber, request.AssetId, cancellationToken))
        {
            return Result<AssetMutationResultDto>.Failure($"Serial number '{request.SerialNumber}' already exists.");
        }

        var before = AssetManagementJson.Serialize(asset.ToListItemDto());
        asset.UpdateDetails(
            request.Name,
            request.Description,
            request.CategoryId,
            request.Subcategory,
            request.Manufacturer,
            request.Model,
            request.SerialNumber,
            request.PurchaseDate,
            request.WarrantyExpiry,
            request.Cost,
            request.Currency,
            request.Condition,
            request.LocationId,
            request.SupplierId,
            request.CustomFields,
            request.Images,
            request.Documents);

        assetRepository.UpdateAsset(asset);
        await AssetCommandHelpers.AddActivityAsync(
            assetRepository,
            currentUserService,
            asset.Id,
            "Update",
            before,
            AssetManagementJson.Serialize(asset.ToListItemDto()),
            "Asset updated",
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(asset.Id, $"Asset '{asset.AssetTag}' updated successfully."));
    }
}

/// <summary>
/// Handles asset retirement.
/// </summary>
public sealed class RetireAssetCommandHandler(
    IAssetRepository assetRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RetireAssetCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(RetireAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetAssetByIdAsync(request.AssetId, cancellationToken);
        if (asset is null)
        {
            return Result<AssetMutationResultDto>.Failure("Asset was not found.");
        }

        asset.Retire(request.Status);
        assetRepository.UpdateAsset(asset);

        await AssetCommandHelpers.AddActivityAsync(
            assetRepository,
            currentUserService,
            asset.Id,
            request.Status == AssetStatus.Disposed ? "Dispose" : "Retire",
            null,
            AssetManagementJson.Serialize(asset.ToListItemDto()),
            request.Reason,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(asset.Id, $"Asset '{asset.AssetTag}' was marked as {asset.Status}."));
    }
}

/// <summary>
/// Handles asset movement.
/// </summary>
public sealed class MoveAssetCommandHandler(
    IAssetRepository assetRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<MoveAssetCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(MoveAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetAssetByIdAsync(request.AssetId, cancellationToken);
        if (asset is null)
        {
            return Result<AssetMutationResultDto>.Failure("Asset was not found.");
        }

        var movedByUserId = AssetCommandHelpers.RequireCurrentUserId(currentUserService);
        var originalLocationId = asset.LocationId;
        asset.Move(request.NewLocationId);
        assetRepository.UpdateAsset(asset);

        await assetRepository.AddMovementAsync(
            AssetMovement.Create(asset.Id, originalLocationId, request.NewLocationId, request.Reason, movedByUserId),
            cancellationToken);

        await AssetCommandHelpers.AddActivityAsync(
            assetRepository,
            currentUserService,
            asset.Id,
            "Move",
            AssetManagementJson.Serialize(new { FromLocationId = originalLocationId }),
            AssetManagementJson.Serialize(new { ToLocationId = request.NewLocationId }),
            request.Reason,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(asset.Id, $"Asset '{asset.AssetTag}' moved successfully."));
    }
}
