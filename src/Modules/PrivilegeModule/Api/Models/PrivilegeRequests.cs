using Alphabet.Domain.Enums;

namespace Alphabet.Modules.PrivilegeModule.Api.Models;

/// <summary>
/// Request payload used to create a privilege.
/// </summary>
public sealed record CreatePrivilegeRequest(
    string Name,
    string DisplayName,
    string? Description,
    string Category,
    string? ResourceType,
    IReadOnlyCollection<string> Actions,
    bool IsGlobal,
    IReadOnlyCollection<string> DependsOn,
    IReadOnlyDictionary<string, string?> Attributes);

/// <summary>
/// Request payload used to update a privilege.
/// </summary>
public sealed record UpdatePrivilegeRequest(
    string DisplayName,
    string? Description,
    string Category,
    string? ResourceType,
    IReadOnlyCollection<string> Actions,
    IReadOnlyCollection<string> DependsOn,
    IReadOnlyDictionary<string, string?> Attributes);

/// <summary>
/// Request payload for assigning privileges to a role.
/// </summary>
public sealed record AssignRolePrivilegesRequest(IReadOnlyCollection<Guid> PrivilegeIds, DateTimeOffset? ExpiresAt);

/// <summary>
/// Request payload for bulk privilege assignments.
/// </summary>
public sealed record BulkRolePrivilegeRequest(IReadOnlyCollection<Guid> RoleIds, IReadOnlyCollection<Guid> PrivilegeIds, string Operation, DateTimeOffset? ExpiresAt);

/// <summary>
/// Request payload for assigning a direct privilege to a user.
/// </summary>
public sealed record AssignUserPrivilegeRequest(string PrivilegeId, PrivilegeEffect Effect, DateTimeOffset? ExpiresAt, string? Reason);

/// <summary>
/// Request payload for creating a privilege category.
/// </summary>
public sealed record CreatePrivilegeCategoryRequest(string Name, string? Description, Guid? ParentCategoryId, int SortOrder);

/// <summary>
/// Request payload for moving a privilege to another category.
/// </summary>
public sealed record MovePrivilegeCategoryRequest(Guid CategoryId);

/// <summary>
/// Request payload for creating a privilege policy.
/// </summary>
public sealed record CreatePrivilegePolicyRequest(string Name, string? Description, IReadOnlyCollection<string> Privileges, PrivilegePolicyCondition Condition);

/// <summary>
/// Request payload for assigning a privilege policy.
/// </summary>
public sealed record AssignPolicyRequest(Guid PolicyId, DateTimeOffset? ExpiresAt);

/// <summary>
/// Request payload for self-service privilege requests.
/// </summary>
public sealed record CreatePrivilegeAccessRequest(string PrivilegeId, string Reason, int RequestedDurationDays, string? ApproverEmail);

/// <summary>
/// Request payload for approving or denying a privilege request.
/// </summary>
public sealed record DecidePrivilegeRequest(string? Notes);

/// <summary>
/// Request payload for batch privilege evaluation.
/// </summary>
public sealed record BatchPrivilegeCheckRequest(IReadOnlyCollection<string> Privileges);
