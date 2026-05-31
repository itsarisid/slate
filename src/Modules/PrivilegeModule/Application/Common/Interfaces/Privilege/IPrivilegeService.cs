using Alphabet.Application.Features.Privilege.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;

namespace Alphabet.Application.Common.Interfaces.Privilege;

/// <summary>
/// Provides application-facing privilege management and evaluation workflows.
/// </summary>
public interface IPrivilegeService
{
    /// <summary>
    /// Create privilege async.
    /// </summary>
    Task<Result<Guid>> CreatePrivilegeAsync(
    string name,
    string displayName,
    string? description,
    string category,
    string? resourceType,
    IReadOnlyCollection<string> actions,
    bool isGlobal,
    IReadOnlyCollection<string> dependsOn,
    IReadOnlyDictionary<string, string?> attributes,
    CancellationToken cancellationToken);
    /// <summary>
    /// Update privilege async.
    /// </summary>

    Task<Result> UpdatePrivilegeAsync(
        Guid privilegeId,
        string displayName,
        string? description,
        string category,
        string? resourceType,
        IReadOnlyCollection<string> actions,
        IReadOnlyCollection<string> dependsOn,
        IReadOnlyDictionary<string, string?> attributes,
        CancellationToken cancellationToken);
    /// <summary>
    /// Delete privilege async.
    /// </summary>

    Task<Result> DeletePrivilegeAsync(Guid privilegeId, bool hardDelete, CancellationToken cancellationToken);
    /// <summary>
    /// Assign privileges to role async.
    /// </summary>

    Task<Result> AssignPrivilegesToRoleAsync(Guid roleId, IReadOnlyCollection<Guid> privilegeIds, DateTimeOffset? expiresAt, CancellationToken cancellationToken);
    /// <summary>
    /// Revoke privilege from role async.
    /// </summary>

    Task<Result> RevokePrivilegeFromRoleAsync(Guid roleId, Guid privilegeId, CancellationToken cancellationToken);
    /// <summary>
    /// Bulk assign privileges to roles async.
    /// </summary>

    Task<Result> BulkAssignPrivilegesToRolesAsync(
        IReadOnlyCollection<Guid> roleIds,
        IReadOnlyCollection<Guid> privilegeIds,
        string operation,
        DateTimeOffset? expiresAt,
        CancellationToken cancellationToken);
    /// <summary>
    /// Assign privilege to user async.
    /// </summary>

    Task<Result> AssignPrivilegeToUserAsync(
        Guid userId,
        string privilegeNameOrId,
        PrivilegeEffect effect,
        DateTimeOffset? expiresAt,
        string? reason,
        CancellationToken cancellationToken);
    /// <summary>
    /// Revoke privilege from user async.
    /// </summary>

    Task<Result> RevokePrivilegeFromUserAsync(Guid userId, Guid privilegeId, CancellationToken cancellationToken);
    /// <summary>
    /// Create category async.
    /// </summary>

    Task<Result<Guid>> CreateCategoryAsync(string name, string? description, Guid? parentCategoryId, int sortOrder, CancellationToken cancellationToken);
    /// <summary>
    /// Get categories async.
    /// </summary>

    Task<IReadOnlyList<PrivilegeCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// Move privilege category async.
    /// </summary>

    Task<Result> MovePrivilegeCategoryAsync(Guid privilegeId, Guid categoryId, CancellationToken cancellationToken);
    /// <summary>
    /// Create policy async.
    /// </summary>

    Task<Result<Guid>> CreatePolicyAsync(string name, string? description, IReadOnlyCollection<string> privileges, PrivilegePolicyCondition condition, CancellationToken cancellationToken);
    /// <summary>
    /// Assign policy to role async.
    /// </summary>

    Task<Result> AssignPolicyToRoleAsync(Guid roleId, Guid policyId, DateTimeOffset? expiresAt, CancellationToken cancellationToken);
    /// <summary>
    /// Assign policy to user async.
    /// </summary>

    Task<Result> AssignPolicyToUserAsync(Guid userId, Guid policyId, DateTimeOffset? expiresAt, CancellationToken cancellationToken);
    /// <summary>
    /// Create privilege request async.
    /// </summary>

    Task<Result<Guid>> CreatePrivilegeRequestAsync(Guid userId, string privilegeNameOrId, string reason, int requestedDurationDays, string? approverEmail, CancellationToken cancellationToken);
    /// <summary>
    /// Approve privilege request async.
    /// </summary>

    Task<Result> ApprovePrivilegeRequestAsync(Guid requestId, string? notes, CancellationToken cancellationToken);
    /// <summary>
    /// Deny privilege request async.
    /// </summary>

    Task<Result> DenyPrivilegeRequestAsync(Guid requestId, string? notes, CancellationToken cancellationToken);
    /// <summary>
    /// Get privileges async.
    /// </summary>

    Task<PagedResponseDto<PrivilegeDto>> GetPrivilegesAsync(int pageNumber, int pageSize, string? category, string? search, bool includeDeprecated, CancellationToken cancellationToken);
    /// <summary>
    /// Get privilege by id async.
    /// </summary>

    Task<Result<PrivilegeDto>> GetPrivilegeByIdAsync(Guid privilegeId, CancellationToken cancellationToken);
    /// <summary>
    /// Get role privileges async.
    /// </summary>

    Task<IReadOnlyList<PrivilegeAssignmentDto>> GetRolePrivilegesAsync(Guid roleId, CancellationToken cancellationToken);
    /// <summary>
    /// Get user effective privileges async.
    /// </summary>

    Task<IReadOnlyList<UserEffectivePrivilegeDto>> GetUserEffectivePrivilegesAsync(Guid userId, CancellationToken cancellationToken);
    /// <summary>
    /// Check privilege async.
    /// </summary>

    Task<Result<PrivilegeCheckResultDto>> CheckPrivilegeAsync(Guid userId, string privilegeName, CancellationToken cancellationToken);
    /// <summary>
    /// Batch check privileges async.
    /// </summary>

    Task<IReadOnlyDictionary<string, bool>> BatchCheckPrivilegesAsync(Guid userId, IReadOnlyCollection<string> privilegeNames, CancellationToken cancellationToken);
    /// <summary>
    /// Get analytics async.
    /// </summary>

    Task<PrivilegeAnalyticsDto> GetAnalyticsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// Get audit logs async.
    /// </summary>

    Task<IReadOnlyList<PrivilegeAuditLogDto>> GetAuditLogsAsync(
        Guid? userId,
        Guid? privilegeId,
        PrivilegeAction? action,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int take,
        int skip,
        CancellationToken cancellationToken);
}
