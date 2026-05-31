using Alphabet.Domain.Enums;

namespace Alphabet.Domain.Models;

/// <summary>
/// Represents a workflow step inside a definition.
/// </summary>
public sealed record AssetWorkflowStepDefinitionModel(
    Guid Id,
    string Name,
    int Order,
    string AssignedToRole,
    int RequiredApprovals,
    int TimeoutHours,
    IReadOnlyCollection<string> Actions);

/// <summary>
/// Represents the runtime state of a workflow step.
/// </summary>
public sealed record AssetWorkflowStepStateModel(
    Guid StepId,
    string Name,
    int Order,
    string AssignedToRole,
    IReadOnlyCollection<string> AllowedActions,
    AssetWorkflowStepStatus Status,
    DateTimeOffset DueAt,
    Guid? DelegateToUserId,
    DateTimeOffset? CompletedAt,
    string? Action,
    string? Comment,
    Guid? PerformedByUserId);
