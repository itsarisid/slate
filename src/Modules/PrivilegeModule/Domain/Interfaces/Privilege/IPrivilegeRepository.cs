using Alphabet.Domain.Entities.Privilege;
using Alphabet.Domain.Models;
using Microsoft.AspNetCore.Identity;
using PrivilegeDefinition = Alphabet.Domain.Entities.Privilege.Privilege;

namespace Alphabet.Domain.Interfaces.Privilege;

/// <summary>
/// Provides persistence access for privilege definitions and assignments.
/// </summary>
public interface IPrivilegeRepository
{
    /// <summary>
    /// Privilege name exists async.
    /// </summary>
    Task<bool> PrivilegeNameExistsAsync(string name, Guid? excludingId, CancellationToken cancellationToken);
    /// <summary>
    /// Get privilege by id async.
    /// </summary>

    Task<PrivilegeDefinition?> GetPrivilegeByIdAsync(Guid privilegeId, CancellationToken cancellationToken);
    /// <summary>
    /// Get privilege by name async.
    /// </summary>

    Task<PrivilegeDefinition?> GetPrivilegeByNameAsync(string name, CancellationToken cancellationToken);
    /// <summary>
    /// Get privileges by names async.
    /// </summary>

    Task<IReadOnlyList<PrivilegeDefinition>> GetPrivilegesByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken);
    /// <summary>
    /// Get privileges by ids async.
    /// </summary>

    Task<IReadOnlyList<PrivilegeDefinition>> GetPrivilegesByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    /// <summary>
    /// Get privileges async.
    /// </summary>

    Task<PagedResult<PrivilegeDefinition>> GetPrivilegesAsync(
        int pageNumber,
        int pageSize,
        string? category,
        string? search,
        bool includeDeprecated,
        CancellationToken cancellationToken);
    /// <summary>
    /// Add privilege async.
    /// </summary>

    Task AddPrivilegeAsync(PrivilegeDefinition privilege, CancellationToken cancellationToken);
    /// <summary>
    /// Update privilege.
    /// </summary>

    void UpdatePrivilege(PrivilegeDefinition privilege);
    /// <summary>
    /// Remove privilege.
    /// </summary>

    void RemovePrivilege(PrivilegeDefinition privilege);
    /// <summary>
    /// Get category by id async.
    /// </summary>

    Task<PrivilegeCategory?> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken);
    /// <summary>
    /// Get category by name async.
    /// </summary>

    Task<PrivilegeCategory?> GetCategoryByNameAsync(string categoryName, CancellationToken cancellationToken);
    /// <summary>
    /// Get categories async.
    /// </summary>

    Task<IReadOnlyList<PrivilegeCategory>> GetCategoriesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// Add category async.
    /// </summary>

    Task AddCategoryAsync(PrivilegeCategory category, CancellationToken cancellationToken);
    /// <summary>
    /// Add policy async.
    /// </summary>

    Task AddPolicyAsync(PrivilegePolicy policy, CancellationToken cancellationToken);
    /// <summary>
    /// Get policy by id async.
    /// </summary>

    Task<PrivilegePolicy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken);
    /// <summary>
    /// Get policy by name async.
    /// </summary>

    Task<PrivilegePolicy?> GetPolicyByNameAsync(string policyName, CancellationToken cancellationToken);
    /// <summary>
    /// Get policies by ids async.
    /// </summary>

    Task<IReadOnlyList<PrivilegePolicy>> GetPoliciesByIdsAsync(IEnumerable<Guid> policyIds, CancellationToken cancellationToken);
    /// <summary>
    /// Add role privilege async.
    /// </summary>

    Task AddRolePrivilegeAsync(RolePrivilege assignment, CancellationToken cancellationToken);
    /// <summary>
    /// Get role privilege async.
    /// </summary>

    Task<RolePrivilege?> GetRolePrivilegeAsync(Guid roleId, Guid privilegeId, CancellationToken cancellationToken);
    /// <summary>
    /// Get role privileges async.
    /// </summary>

    Task<IReadOnlyList<RolePrivilege>> GetRolePrivilegesAsync(Guid roleId, CancellationToken cancellationToken);
    /// <summary>
    /// Add user privilege async.
    /// </summary>

    Task AddUserPrivilegeAsync(UserPrivilege assignment, CancellationToken cancellationToken);
    /// <summary>
    /// Get user privilege async.
    /// </summary>

    Task<UserPrivilege?> GetUserPrivilegeAsync(Guid userId, Guid privilegeId, CancellationToken cancellationToken);
    /// <summary>
    /// Get user privileges async.
    /// </summary>

    Task<IReadOnlyList<UserPrivilege>> GetUserPrivilegesAsync(Guid userId, CancellationToken cancellationToken);
    /// <summary>
    /// Add role policy assignment async.
    /// </summary>

    Task AddRolePolicyAssignmentAsync(RolePrivilegePolicy assignment, CancellationToken cancellationToken);
    /// <summary>
    /// Add user policy assignment async.
    /// </summary>

    Task AddUserPolicyAssignmentAsync(UserPrivilegePolicy assignment, CancellationToken cancellationToken);
    /// <summary>
    /// Get role policy assignments async.
    /// </summary>

    Task<IReadOnlyList<RolePrivilegePolicy>> GetRolePolicyAssignmentsAsync(Guid roleId, CancellationToken cancellationToken);
    /// <summary>
    /// Get user policy assignments async.
    /// </summary>

    Task<IReadOnlyList<UserPrivilegePolicy>> GetUserPolicyAssignmentsAsync(Guid userId, CancellationToken cancellationToken);
    /// <summary>
    /// Get user role ids async.
    /// </summary>

    Task<IReadOnlyList<Guid>> GetUserRoleIdsAsync(Guid userId, CancellationToken cancellationToken);
    /// <summary>
    /// Get user role names async.
    /// </summary>

    Task<IReadOnlyList<string>> GetUserRoleNamesAsync(Guid userId, CancellationToken cancellationToken);
    /// <summary>
    /// Get roles by ids async.
    /// </summary>

    Task<IReadOnlyList<IdentityRole<Guid>>> GetRolesByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken);
    /// <summary>
    /// Get role by id async.
    /// </summary>

    Task<IdentityRole<Guid>?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken);
    /// <summary>
    /// Get role by name async.
    /// </summary>

    Task<IdentityRole<Guid>?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken);
    /// <summary>
    /// Add privilege request async.
    /// </summary>

    Task AddPrivilegeRequestAsync(PrivilegeRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// Get privilege request by id async.
    /// </summary>

    Task<PrivilegeRequest?> GetPrivilegeRequestByIdAsync(Guid requestId, CancellationToken cancellationToken);
    /// <summary>
    /// Get privilege requests for user async.
    /// </summary>

    Task<IReadOnlyList<PrivilegeRequest>> GetPrivilegeRequestsForUserAsync(Guid userId, CancellationToken cancellationToken);
}
