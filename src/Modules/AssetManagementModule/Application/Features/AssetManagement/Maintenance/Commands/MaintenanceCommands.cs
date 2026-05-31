using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.AssetManagement;
using Alphabet.Application.Features.AssetManagement.Assets.Commands;
using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.AssetManagement;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Maintenance.Commands;

/// <summary>
/// Schedules maintenance for an asset.
/// </summary>
public sealed record ScheduleAssetMaintenanceCommand(
    Guid AssetId,
    AssetMaintenanceType MaintenanceType,
    DateOnly ScheduledDate,
    string Description,
    string? AssignedToVendor,
    decimal EstimatedCost,
    AssetMaintenancePriority Priority) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Completes a maintenance record.
/// </summary>
public sealed record CompleteAssetMaintenanceCommand(
    Guid AssetId,
    Guid MaintenanceId,
    DateOnly CompletionDate,
    decimal ActualCost,
    string? Notes,
    DateOnly? NextMaintenanceDueDate) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Handles maintenance scheduling.
/// </summary>
public sealed class ScheduleAssetMaintenanceCommandHandler(
    IAssetRepository assetRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ScheduleAssetMaintenanceCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(ScheduleAssetMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetAssetByIdAsync(request.AssetId, cancellationToken);
        if (asset is null)
        {
            return Result<AssetMutationResultDto>.Failure("Asset was not found.");
        }

        var maintenance = AssetMaintenanceRecord.Create(
            asset.Id,
            request.MaintenanceType,
            request.ScheduledDate,
            request.Description,
            request.AssignedToVendor,
            request.EstimatedCost,
            request.Priority);

        asset.MarkUnderRepair();
        assetRepository.UpdateAsset(asset);
        await assetRepository.AddMaintenanceAsync(maintenance, cancellationToken);
        await AssetCommandHelpers.AddActivityAsync(
            assetRepository,
            currentUserService,
            asset.Id,
            "MaintenanceScheduled",
            null,
            Alphabet.Domain.Models.AssetManagementJson.Serialize(maintenance.ToMaintenanceDto()),
            request.Description,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(maintenance.Id, $"Maintenance scheduled for asset '{asset.AssetTag}'."));
    }
}

/// <summary>
/// Handles maintenance completion.
/// </summary>
public sealed class CompleteAssetMaintenanceCommandHandler(
    IAssetRepository assetRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteAssetMaintenanceCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(CompleteAssetMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetAssetByIdAsync(request.AssetId, cancellationToken);
        var maintenance = await assetRepository.GetMaintenanceByIdAsync(request.AssetId, request.MaintenanceId, cancellationToken);
        if (asset is null || maintenance is null)
        {
            return Result<AssetMutationResultDto>.Failure("Maintenance record was not found.");
        }

        maintenance.Complete(request.CompletionDate, request.ActualCost, request.Notes, request.NextMaintenanceDueDate);
        asset.MarkAvailable();

        assetRepository.UpdateMaintenance(maintenance);
        assetRepository.UpdateAsset(asset);
        await AssetCommandHelpers.AddActivityAsync(
            assetRepository,
            currentUserService,
            asset.Id,
            "MaintenanceCompleted",
            null,
            Alphabet.Domain.Models.AssetManagementJson.Serialize(maintenance.ToMaintenanceDto()),
            request.Notes,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(maintenance.Id, $"Maintenance completed for asset '{asset.AssetTag}'."));
    }
}
