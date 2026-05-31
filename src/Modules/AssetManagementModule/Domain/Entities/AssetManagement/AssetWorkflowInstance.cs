using System.ComponentModel.DataAnnotations.Schema;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a running workflow instance for an asset.
/// </summary>
public sealed class AssetWorkflowInstance : BaseEntity
{
    private AssetWorkflowInstance()
    {
    }

    private AssetWorkflowInstance(
        Guid workflowDefinitionId,
        Guid assetId,
        IReadOnlyCollection<AssetWorkflowStepStateModel> steps,
        Guid initiatedByUserId,
        string contextJson)
    {
        WorkflowDefinitionId = workflowDefinitionId;
        AssetId = assetId;
        StepsJson = AssetManagementJson.Serialize(steps);
        CurrentStepId = steps.OrderBy(x => x.Order).Select(x => x.StepId).FirstOrDefault();
        Status = AssetWorkflowStatus.Active;
        ContextJson = contextJson;
        InitiatedByUserId = initiatedByUserId;
        InitiatedAt = DateTimeOffset.UtcNow;
    }

    public Guid WorkflowDefinitionId { get; private set; }

    public Guid AssetId { get; private set; }

    public AssetWorkflowStatus Status { get; private set; }

    public Guid? CurrentStepId { get; private set; }

    public string ContextJson { get; private set; } = "{}";

    public Guid InitiatedByUserId { get; private set; }

    public DateTimeOffset InitiatedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public string StepsJson { get; private set; } = "[]";

    [NotMapped]
    public IReadOnlyList<AssetWorkflowStepStateModel> Steps => AssetManagementJson.DeserializeList<AssetWorkflowStepStateModel>(StepsJson);

    /// <summary>
    /// Creates a workflow instance.
    /// </summary>
    public static AssetWorkflowInstance Create(
        Guid workflowDefinitionId,
        Guid assetId,
        IReadOnlyCollection<AssetWorkflowStepDefinitionModel> steps,
        Guid initiatedByUserId,
        IReadOnlyDictionary<string, string>? context)
    {
        var now = DateTimeOffset.UtcNow;
        var runtimeSteps = steps
            .OrderBy(x => x.Order)
            .Select(step => new AssetWorkflowStepStateModel(
                step.Id,
                step.Name,
                step.Order,
                step.AssignedToRole,
                step.Actions,
                AssetWorkflowStepStatus.Pending,
                now.AddHours(step.TimeoutHours),
                null,
                null,
                null,
                null,
                null))
            .ToArray();

        return new AssetWorkflowInstance(
            workflowDefinitionId,
            assetId,
            runtimeSteps,
            initiatedByUserId,
            AssetManagementJson.Serialize(context ?? new Dictionary<string, string>()));
    }

    /// <summary>
    /// Applies an action to the current workflow step.
    /// </summary>
    public void ApplyAction(Guid stepId, string action, Guid performedByUserId, string? comment)
    {
        var steps = Steps.OrderBy(x => x.Order).ToList();
        var current = steps.Single(x => x.StepId == stepId);
        var status = action.Trim().Equals("Reject", StringComparison.OrdinalIgnoreCase)
            ? AssetWorkflowStepStatus.Rejected
            : AssetWorkflowStepStatus.Completed;

        var updated = current with
        {
            Status = status,
            Action = action.Trim(),
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            PerformedByUserId = performedByUserId,
            CompletedAt = DateTimeOffset.UtcNow
        };

        var index = steps.FindIndex(x => x.StepId == stepId);
        steps[index] = updated;

        if (status == AssetWorkflowStepStatus.Rejected)
        {
            Status = AssetWorkflowStatus.Rejected;
            CompletedAt = DateTimeOffset.UtcNow;
            CurrentStepId = null;
        }
        else
        {
            var next = steps
                .Where(x => x.Order > current.Order)
                .OrderBy(x => x.Order)
                .FirstOrDefault();

            if (next is null)
            {
                Status = AssetWorkflowStatus.Completed;
                CompletedAt = DateTimeOffset.UtcNow;
                CurrentStepId = null;
            }
            else
            {
                CurrentStepId = next.StepId;
            }
        }

        StepsJson = AssetManagementJson.Serialize(steps);
        Touch();
    }

    /// <summary>
    /// Delegates the current step to another user.
    /// </summary>
    public void DelegateStep(Guid stepId, Guid delegateToUserId, Guid performedByUserId, string? comment)
    {
        var steps = Steps.ToList();
        var current = steps.Single(x => x.StepId == stepId);
        var index = steps.FindIndex(x => x.StepId == stepId);
        steps[index] = current with
        {
            Status = AssetWorkflowStepStatus.Delegated,
            DelegateToUserId = delegateToUserId,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            PerformedByUserId = performedByUserId
        };

        StepsJson = AssetManagementJson.Serialize(steps);
        Touch();
    }

    /// <summary>
    /// Marks the workflow as escalated.
    /// </summary>
    public void MarkEscalated()
    {
        Status = AssetWorkflowStatus.Escalated;
        Touch();
    }
}
