using Alphabet.Application.Features.Privilege.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;

namespace Alphabet.Application.Common.Interfaces.Privilege;

/// <summary>
/// Provides application-facing privilege management and evaluation workflows.
/// </summary>
public interface IPrivilegeService
{
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

    Task<Result> DeletePrivilegeAsync(Guid privilegeId, bool hardDelete, CancellationToken cancellationToken);

    Task<Result> AssignPrivilegesToRoleAsync(Guid roleId, IReadOnlyCollection<Guid> privilegeIds, DateTimeOffset? expiresAt, CancellationToken cancellationToken);

    Task<Result> RevokePrivilegeFromRoleAsync(Guid roleId, Guid privilegeId, CancellationToken cancellationToken);

    Task<Result> BulkAssignPrivilegesToRolesAsync(
        IReadOnlyCollection<Guid> roleIds,
        IReadOnlyCollection<Guid> privilegeIds,
        string operation,
        DateTimeOffset? expiresAt,
        CancellationToken cancellationToken);

    Task<Result> AssignPrivilegeToUserAsync(
        Guid userId,
        string privilegeNameOrId,
        PrivilegeEffect effect,
        DateTimeOffset? expiresAt,
        string? reason,
        CancellationToken cancellationToken);

    Task<Result> RevokePrivilegeFromUserAsync(Guid userId, Guid privilegeId, CancellationToken cancellationToken);

    Task<Result<Guid>> CreateCategoryAsync(string name, string? description, Guid? parentCategoryId, int sortOrder, CancellationToken cancellationToken);

    Task<IReadOnlyList<PrivilegeCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken);

    Task<Result> MovePrivilegeCategoryAsync(Guid privilegeId, Guid categoryId, CancellationToken cancellationToken);

    Task<Result<Guid>> CreatePolicyAsync(string name, string? description, IReadOnlyCollection<string> privileges, PrivilegePolicyCondition condition, CancellationToken cancellationToken);

    Task<Result> AssignPolicyToRoleAsync(Guid roleId, Guid policyId, DateTimeOffset? expiresAt, CancellationToken cancellationToken);

    Task<Result> AssignPolicyToUserAsync(Guid userId, Guid policyId, DateTimeOffset? expiresAt, CancellationToken cancellationToken);

    Task<Result<Guid>> CreatePrivilegeRequestAsync(Guid userId, string privilegeNameOrId, string reason, int requestedDurationDays, string? approverEmail, CancellationToken cancellationToken);

    Task<Result> ApprovePrivilegeRequestAsync(Guid requestId, string? notes, CancellationToken cancellationToken);

    Task<Result> DenyPrivilegeRequestAsync(Guid requestId, string? notes, CancellationToken cancellationToken);

    Task<PagedResponseDto<PrivilegeDto>> GetPrivilegesAsync(int pageNumber, int pageSize, string? category, string? search, bool includeDeprecated, CancellationToken cancellationToken);

    Task<Result<PrivilegeDto>> GetPrivilegeByIdAsync(Guid privilegeId, CancellationToken cancellationToken);

    Task<IReadOnlyList<PrivilegeAssignmentDto>> GetRolePrivilegesAsync(Guid roleId, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserEffectivePrivilegeDto>> GetUserEffectivePrivilegesAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<PrivilegeCheckResultDto>> CheckPrivilegeAsync(Guid userId, string privilegeName, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, bool>> BatchCheckPrivilegesAsync(Guid userId, IReadOnlyCollection<string> privilegeNames, CancellationToken cancellationToken);

    Task<PrivilegeAnalyticsDto> GetAnalyticsAsync(CancellationToken cancellationToken);

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
