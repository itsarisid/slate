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
    Task<bool> PrivilegeNameExistsAsync(string name, Guid? excludingId, CancellationToken cancellationToken);

    Task<PrivilegeDefinition?> GetPrivilegeByIdAsync(Guid privilegeId, CancellationToken cancellationToken);

    Task<PrivilegeDefinition?> GetPrivilegeByNameAsync(string name, CancellationToken cancellationToken);

    Task<IReadOnlyList<PrivilegeDefinition>> GetPrivilegesByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken);

    Task<IReadOnlyList<PrivilegeDefinition>> GetPrivilegesByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);

    Task<PagedResult<PrivilegeDefinition>> GetPrivilegesAsync(
        int pageNumber,
        int pageSize,
        string? category,
        string? search,
        bool includeDeprecated,
        CancellationToken cancellationToken);

    Task AddPrivilegeAsync(PrivilegeDefinition privilege, CancellationToken cancellationToken);

    void UpdatePrivilege(PrivilegeDefinition privilege);

    void RemovePrivilege(PrivilegeDefinition privilege);

    Task<PrivilegeCategory?> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken);

    Task<PrivilegeCategory?> GetCategoryByNameAsync(string categoryName, CancellationToken cancellationToken);

    Task<IReadOnlyList<PrivilegeCategory>> GetCategoriesAsync(CancellationToken cancellationToken);

    Task AddCategoryAsync(PrivilegeCategory category, CancellationToken cancellationToken);

    Task AddPolicyAsync(PrivilegePolicy policy, CancellationToken cancellationToken);

    Task<PrivilegePolicy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken);

    Task<PrivilegePolicy?> GetPolicyByNameAsync(string policyName, CancellationToken cancellationToken);

    Task<IReadOnlyList<PrivilegePolicy>> GetPoliciesByIdsAsync(IEnumerable<Guid> policyIds, CancellationToken cancellationToken);

    Task AddRolePrivilegeAsync(RolePrivilege assignment, CancellationToken cancellationToken);

    Task<RolePrivilege?> GetRolePrivilegeAsync(Guid roleId, Guid privilegeId, CancellationToken cancellationToken);

    Task<IReadOnlyList<RolePrivilege>> GetRolePrivilegesAsync(Guid roleId, CancellationToken cancellationToken);

    Task AddUserPrivilegeAsync(UserPrivilege assignment, CancellationToken cancellationToken);

    Task<UserPrivilege?> GetUserPrivilegeAsync(Guid userId, Guid privilegeId, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserPrivilege>> GetUserPrivilegesAsync(Guid userId, CancellationToken cancellationToken);

    Task AddRolePolicyAssignmentAsync(RolePrivilegePolicy assignment, CancellationToken cancellationToken);

    Task AddUserPolicyAssignmentAsync(UserPrivilegePolicy assignment, CancellationToken cancellationToken);

    Task<IReadOnlyList<RolePrivilegePolicy>> GetRolePolicyAssignmentsAsync(Guid roleId, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserPrivilegePolicy>> GetUserPolicyAssignmentsAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> GetUserRoleIdsAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetUserRoleNamesAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<IdentityRole<Guid>>> GetRolesByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken);

    Task<IdentityRole<Guid>?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken);

    Task<IdentityRole<Guid>?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken);

    Task AddPrivilegeRequestAsync(PrivilegeRequest request, CancellationToken cancellationToken);

    Task<PrivilegeRequest?> GetPrivilegeRequestByIdAsync(Guid requestId, CancellationToken cancellationToken);

    Task<IReadOnlyList<PrivilegeRequest>> GetPrivilegeRequestsForUserAsync(Guid userId, CancellationToken cancellationToken);
}
