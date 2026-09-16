using Alphabet.Application.Common.Interfaces.AssetManagement;
using Alphabet.Application.Features.AssetManagement.Assets.Commands;
using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.AssetManagement;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Assignments.Commands;

/// <summary>
/// Assigns an asset to a user.
/// </summary>
public sealed record AssignAssetCommand(
    Guid AssetId,
    Guid AssignedToUserId,
    DateOnly? ExpectedReturnDate,
    AssetAssignmentType AssignmentType,
    string Purpose,
    AssetCondition ConditionAtAssignment,
    string? Notes) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Unassigns an asset and records the return.
/// </summary>
public sealed record UnassignAssetCommand(
    Guid AssetId,
    Guid ReturnedByUserId,
    Guid ReceivedByUserId,
    AssetCondition ConditionOnReturn,
    string? DamageNotes,
    IReadOnlyCollection<string>? MissingItems,
    DateOnly ActualReturnDate) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Transfers an assigned asset between users.
/// </summary>
public sealed record TransferAssetCommand(
    Guid AssetId,
    Guid FromUserId,
    Guid ToUserId,
    string Reason,
    DateOnly TransferDate,
    DateOnly? ExpectedReturnDate) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Handles asset assignment.
/// </summary>
public sealed class AssignAssetCommandHandler(
    IAssetRepository assetRepository,
    IAssetUserDirectory userDirectory,
    IAssetNotificationService notificationService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AssignAssetCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(AssignAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetAssetByIdAsync(request.AssetId, cancellationToken);
        if (asset is null)
        {
            return Result<AssetMutationResultDto>.Failure("Asset was not found.");
        }

        var assignee = await userDirectory.GetByIdAsync(request.AssignedToUserId, cancellationToken);
        if (assignee is null || !assignee.IsActive)
        {
            return Result<AssetMutationResultDto>.Failure("The assignee does not exist or is inactive.");
        }

        if (asset.AssignedToUserId.HasValue)
        {
            return Result<AssetMutationResultDto>.Failure("The asset is already assigned.");
        }

        var assignedByUserId = AssetCommandHelpers.RequireCurrentUserId(currentUserService);
        asset.AssignTo(request.AssignedToUserId, request.ExpectedReturnDate);
        assetRepository.UpdateAsset(asset);

        var assignment = AssetAssignment.Create(
            asset.Id,
            request.AssignedToUserId,
            assignedByUserId,
            request.ExpectedReturnDate,
            request.AssignmentType,
            request.ConditionAtAssignment,
            request.Purpose,
            request.Notes);

        await assetRepository.AddAssignmentAsync(assignment, cancellationToken);
        await AssetCommandHelpers.AddActivityAsync(
            assetRepository,
            currentUserService,
            asset.Id,
            "Assign",
            null,
            AssetManagementJson.Serialize(assignment.ToAssignmentDto()),
            request.Purpose,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyAssetAssignedAsync(asset, assignee, cancellationToken);

        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(asset.Id, $"Asset '{asset.AssetTag}' assigned to {assignee.DisplayName}."));
    }
}

/// <summary>
/// Handles asset returns.
/// </summary>
public sealed class UnassignAssetCommandHandler(
    IAssetRepository assetRepository,
    IAssetNotificationService notificationService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UnassignAssetCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(UnassignAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetAssetByIdAsync(request.AssetId, cancellationToken);
        var assignment = await assetRepository.GetActiveAssignmentAsync(request.AssetId, cancellationToken);
        if (asset is null || assignment is null)
        {
            return Result<AssetMutationResultDto>.Failure("The asset does not have an active assignment.");
        }

        assignment.MarkReturned(
            request.ReturnedByUserId,
            request.ReceivedByUserId,
            request.ConditionOnReturn,
            request.DamageNotes,
            request.MissingItems,
            request.ActualReturnDate);

        asset.ReturnFromAssignment(request.ConditionOnReturn);
        assetRepository.UpdateAssignment(assignment);
        assetRepository.UpdateAsset(asset);

        await AssetCommandHelpers.AddActivityAsync(
            assetRepository,
            currentUserService,
            asset.Id,
            "Unassign",
            null,
            AssetManagementJson.Serialize(assignment.ToAssignmentDto()),
            request.DamageNotes,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyAssetReturnedAsync(asset, cancellationToken);

        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(asset.Id, $"Asset '{asset.AssetTag}' returned successfully."));
    }
}

/// <summary>
/// Handles asset transfer operations.
/// </summary>
public sealed class TransferAssetCommandHandler(
    IAssetRepository assetRepository,
    IAssetUserDirectory userDirectory,
    IAssetNotificationService notificationService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<TransferAssetCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(TransferAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetAssetByIdAsync(request.AssetId, cancellationToken);
        var activeAssignment = await assetRepository.GetActiveAssignmentAsync(request.AssetId, cancellationToken);
        if (asset is null || activeAssignment is null || asset.AssignedToUserId != request.FromUserId)
        {
            return Result<AssetMutationResultDto>.Failure("The asset is not currently assigned to the source user.");
        }

        var targetUser = await userDirectory.GetByIdAsync(request.ToUserId, cancellationToken);
        if (targetUser is null || !targetUser.IsActive)
        {
            return Result<AssetMutationResultDto>.Failure("The transfer target user does not exist or is inactive.");
        }

        var performedByUserId = AssetCommandHelpers.RequireCurrentUserId(currentUserService);
        activeAssignment.MarkReturned(request.FromUserId, performedByUserId, asset.Condition, request.Reason, null, request.TransferDate);
        assetRepository.UpdateAssignment(activeAssignment);

        var newAssignment = AssetAssignment.Create(
            asset.Id,
            request.ToUserId,
            performedByUserId,
            request.ExpectedReturnDate,
            AssetAssignmentType.Temporary,
            asset.Condition,
            request.Reason,
            "Transferred");

        asset.TransferTo(request.ToUserId, request.ExpectedReturnDate);
        assetRepository.UpdateAsset(asset);
        await assetRepository.AddAssignmentAsync(newAssignment, cancellationToken);

        await AssetCommandHelpers.AddActivityAsync(
            assetRepository,
            currentUserService,
            asset.Id,
            "Transfer",
            AssetManagementJson.Serialize(new { request.FromUserId }),
            AssetManagementJson.Serialize(new { request.ToUserId }),
            request.Reason,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyAssetAssignedAsync(asset, targetUser, cancellationToken);

        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(asset.Id, $"Asset '{asset.AssetTag}' transferred successfully."));
    }
}
