using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.AssetManagement;
using Alphabet.Application.Features.AssetManagement.Assets.Commands;
using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.AssetManagement;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Inventory.Commands;

/// <summary>
/// Applies a stock adjustment.
/// </summary>
public sealed record AdjustInventoryStockCommand(
    Guid AssetId,
    Guid LocationId,
    StockAdjustmentType AdjustmentType,
    int Quantity,
    string Reason,
    string? ReferenceNumber,
    int MinimumThreshold) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Performs a stock take for a location.
/// </summary>
public sealed record PerformAssetStockTakeCommand(
    Guid LocationId,
    IReadOnlyCollection<StockTakeCountedItem> CountedItems) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Handles stock adjustments.
/// </summary>
public sealed class AdjustInventoryStockCommandHandler(
    IAssetRepository assetRepository,
    IAssetNotificationService notificationService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AdjustInventoryStockCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(AdjustInventoryStockCommand request, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetAssetByIdAsync(request.AssetId, cancellationToken);
        if (asset is null)
        {
            return Result<AssetMutationResultDto>.Failure("Asset was not found.");
        }

        var existingBalance = await assetRepository.GetInventoryBalanceAsync(request.AssetId, request.LocationId, cancellationToken);
        var balance = existingBalance ?? InventoryBalance.Create(request.AssetId, request.LocationId, 0, request.MinimumThreshold);

        var updatedQuantity = request.AdjustmentType switch
        {
            StockAdjustmentType.Add => balance.QuantityOnHand + request.Quantity,
            StockAdjustmentType.Remove => balance.QuantityOnHand - request.Quantity,
            StockAdjustmentType.Set => request.Quantity,
            _ => balance.QuantityOnHand
        };

        if (updatedQuantity < 0)
        {
            return Result<AssetMutationResultDto>.Failure("Stock adjustments cannot reduce quantity below zero.");
        }

        balance.Apply(updatedQuantity);
        if (existingBalance is null)
        {
            await assetRepository.AddInventoryBalanceAsync(balance, cancellationToken);
        }
        else
        {
            assetRepository.UpdateInventoryBalance(balance);
        }

        var performedByUserId = AssetCommandHelpers.RequireCurrentUserId(currentUserService);
        await assetRepository.AddStockAdjustmentAsync(
            StockAdjustment.Create(
                request.AssetId,
                request.LocationId,
                request.AdjustmentType,
                request.Quantity,
                request.Reason,
                performedByUserId,
                request.ReferenceNumber),
            cancellationToken);

        await AssetCommandHelpers.AddActivityAsync(
            assetRepository,
            currentUserService,
            asset.Id,
            "StockAdjustment",
            null,
            Alphabet.Domain.Models.AssetManagementJson.Serialize(balance.ToInventoryDto()),
            request.Reason,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (balance.QuantityOnHand <= balance.MinimumThreshold)
        {
            await notificationService.NotifyLowStockAsync(asset, balance.QuantityOnHand, balance.MinimumThreshold, cancellationToken);
        }

        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(balance.Id, $"Inventory updated for asset '{asset.AssetTag}'."));
    }
}

/// <summary>
/// Handles stock take operations.
/// </summary>
public sealed class PerformAssetStockTakeCommandHandler(
    IAssetRepository assetRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<PerformAssetStockTakeCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(PerformAssetStockTakeCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.CountedItems)
        {
            var existingBalance = await assetRepository.GetInventoryBalanceAsync(item.AssetId, request.LocationId, cancellationToken);
            var balance = existingBalance ?? InventoryBalance.Create(item.AssetId, request.LocationId, 0, 0);

            balance.Apply(item.CountedQuantity);
            balance.MarkCounted();

            if (existingBalance is null)
            {
                await assetRepository.AddInventoryBalanceAsync(balance, cancellationToken);
            }
            else
            {
                assetRepository.UpdateInventoryBalance(balance);
            }

            var asset = await assetRepository.GetAssetByIdAsync(item.AssetId, cancellationToken);
            if (asset is not null)
            {
                asset.MarkInventoryChecked();
                assetRepository.UpdateAsset(asset);
            }
        }

        await AssetCommandHelpers.AddActivityAsync(
            assetRepository,
            currentUserService,
            null,
            "StockTake",
            null,
            Alphabet.Domain.Models.AssetManagementJson.Serialize(request.CountedItems),
            "Physical stock take completed",
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(request.LocationId, "Stock take recorded successfully."));
    }
}
