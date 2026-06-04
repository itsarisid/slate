using Alphabet.Application.Common.Interfaces.AssetManagement;
using Alphabet.Application.Features.AssetManagement.Assets.Commands;
using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.AssetManagement;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Workflows.Commands;

/// <summary>
/// Creates a workflow definition.
/// </summary>
public sealed record CreateAssetWorkflowDefinitionCommand(
    string Name,
    string Description,
    int Version,
    IReadOnlyCollection<AssetWorkflowDefinitionStepInput> Steps) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Starts a workflow instance for an asset.
/// </summary>
public sealed record StartAssetWorkflowCommand(
    Guid AssetId,
    Guid WorkflowDefinitionId,
    IReadOnlyDictionary<string, string>? Context) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Applies an action to a workflow step.
/// </summary>
public sealed record ActOnAssetWorkflowStepCommand(
    Guid InstanceId,
    Guid StepId,
    string Action,
    string? Comment) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Represents workflow definition step input.
/// </summary>
public sealed record AssetWorkflowDefinitionStepInput(
    string Name,
    int Order,
    string AssignedToRole,
    int RequiredApprovals,
    int TimeoutHours,
    IReadOnlyCollection<string> Actions);

/// <summary>
/// Handles workflow definition creation.
/// </summary>
public sealed class CreateAssetWorkflowDefinitionCommandHandler(
    IAssetRepository assetRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAssetWorkflowDefinitionCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(CreateAssetWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var steps = request.Steps
            .OrderBy(x => x.Order)
            .Select(step => new AssetWorkflowStepDefinitionModel(
                Guid.NewGuid(),
                step.Name,
                step.Order,
                step.AssignedToRole,
                step.RequiredApprovals,
                step.TimeoutHours,
                step.Actions))
            .ToArray();

        var definition = AssetWorkflowDefinition.Create(request.Name, request.Description, request.Version, steps);
        await assetRepository.AddWorkflowDefinitionAsync(definition, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(definition.Id, $"Workflow '{definition.Name}' created successfully."));
    }
}

/// <summary>
/// Handles workflow instance starts.
/// </summary>
public sealed class StartAssetWorkflowCommandHandler(
    IAssetRepository assetRepository,
    IAssetNotificationService notificationService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<StartAssetWorkflowCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(StartAssetWorkflowCommand request, CancellationToken cancellationToken)
    {
        var definition = await assetRepository.GetWorkflowDefinitionByIdAsync(request.WorkflowDefinitionId, cancellationToken);
        var asset = await assetRepository.GetAssetByIdAsync(request.AssetId, cancellationToken);
        if (definition is null || asset is null)
        {
            return Result<AssetMutationResultDto>.Failure("Workflow definition or asset was not found.");
        }

        var initiatedByUserId = AssetCommandHelpers.RequireCurrentUserId(currentUserService);
        var instance = AssetWorkflowInstance.Create(definition.Id, asset.Id, definition.Steps, initiatedByUserId, request.Context);
        await assetRepository.AddWorkflowInstanceAsync(instance, cancellationToken);

        await AssetCommandHelpers.AddActivityAsync(
            assetRepository,
            currentUserService,
            asset.Id,
            "WorkflowStarted",
            null,
            Alphabet.Domain.Models.AssetManagementJson.Serialize(instance.ToInstanceDto()),
            definition.Name,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var firstRole = definition.Steps.OrderBy(x => x.Order).FirstOrDefault()?.AssignedToRole;
        if (!string.IsNullOrWhiteSpace(firstRole))
        {
            await notificationService.NotifyWorkflowStepAssignedAsync(instance, firstRole, cancellationToken);
        }

        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(instance.Id, $"Workflow '{definition.Name}' started for asset '{asset.AssetTag}'."));
    }
}

/// <summary>
/// Handles workflow step actions.
/// </summary>
public sealed class ActOnAssetWorkflowStepCommandHandler(
    IAssetRepository assetRepository,
    IAssetNotificationService notificationService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ActOnAssetWorkflowStepCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(ActOnAssetWorkflowStepCommand request, CancellationToken cancellationToken)
    {
        var instance = await assetRepository.GetWorkflowInstanceByIdAsync(request.InstanceId, cancellationToken);
        if (instance is null)
        {
            return Result<AssetMutationResultDto>.Failure("Workflow instance was not found.");
        }

        var performedByUserId = AssetCommandHelpers.RequireCurrentUserId(currentUserService);
        instance.ApplyAction(request.StepId, request.Action, performedByUserId, request.Comment);
        assetRepository.UpdateWorkflowInstance(instance);

        await AssetCommandHelpers.AddActivityAsync(
            assetRepository,
            currentUserService,
            instance.AssetId,
            "WorkflowAction",
            null,
            Alphabet.Domain.Models.AssetManagementJson.Serialize(instance.ToInstanceDto()),
            request.Comment,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var nextRole = instance.Steps
            .Where(x => x.StepId == instance.CurrentStepId)
            .Select(x => x.AssignedToRole)
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(nextRole))
        {
            await notificationService.NotifyWorkflowStepAssignedAsync(instance, nextRole, cancellationToken);
        }

        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(instance.Id, "Workflow step updated successfully."));
    }
}
