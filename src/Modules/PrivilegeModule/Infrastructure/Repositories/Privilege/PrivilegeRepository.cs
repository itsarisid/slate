using Alphabet.Domain.Entities.Privilege;
using Alphabet.Domain.Interfaces.Privilege;
using Alphabet.Domain.Models;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PrivilegeDefinition = Alphabet.Domain.Entities.Privilege.Privilege;

namespace Alphabet.Infrastructure.Repositories.Privilege;

/// <summary>
/// EF Core repository for privilege definitions and assignments.
/// </summary>
public sealed class PrivilegeRepository(AppDbContext dbContext) : IPrivilegeRepository
{
    /// <summary>
    /// Privilege name exists async.
    /// </summary>
    public async Task<bool> PrivilegeNameExistsAsync(string name, Guid? excludingId, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return await dbContext.Set<PrivilegeDefinition>()
            .AnyAsync(x => x.Name == normalizedName && (!excludingId.HasValue || x.Id != excludingId.Value), cancellationToken);
    }
    /// <summary>
    /// Get privilege by id async.
    /// </summary>

    public Task<PrivilegeDefinition?> GetPrivilegeByIdAsync(Guid privilegeId, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeDefinition>().FirstOrDefaultAsync(x => x.Id == privilegeId, cancellationToken);
    /// <summary>
    /// Get privilege by name async.
    /// </summary>

    public Task<PrivilegeDefinition?> GetPrivilegeByNameAsync(string name, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeDefinition>().FirstOrDefaultAsync(x => x.Name == name.Trim().ToLowerInvariant(), cancellationToken);
    /// <summary>
    /// Get privileges by names async.
    /// </summary>

    public async Task<IReadOnlyList<PrivilegeDefinition>> GetPrivilegesByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken)
    {
        var normalized = names.Select(static x => x.Trim().ToLowerInvariant()).Distinct().ToArray();
        return await dbContext.Set<PrivilegeDefinition>().Where(x => normalized.Contains(x.Name)).ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Get privileges by ids async.
    /// </summary>

    public async Task<IReadOnlyList<PrivilegeDefinition>> GetPrivilegesByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var list = ids.Distinct().ToArray();
        return await dbContext.Set<PrivilegeDefinition>().Where(x => list.Contains(x.Id)).ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Get privileges async.
    /// </summary>

    public async Task<PagedResult<PrivilegeDefinition>> GetPrivilegesAsync(
        int pageNumber,
        int pageSize,
        string? category,
        string? search,
        bool includeDeprecated,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Set<PrivilegeDefinition>().AsQueryable();

        if (!includeDeprecated)
        {
            query = query.Where(x => !x.IsDeprecated);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.Contains(normalizedSearch) ||
                x.DisplayName.ToLower().Contains(normalizedSearch) ||
                (x.Description != null && x.Description.ToLower().Contains(normalizedSearch)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var categoryEntity = await GetCategoryByNameAsync(category, cancellationToken);
            if (categoryEntity is not null)
            {
                query = query.Where(x => x.CategoryId == categoryEntity.Id);
            }
            else
            {
                query = query.Where(static _ => false);
            }
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PrivilegeDefinition>(items, total, pageNumber, pageSize);
    }
    /// <summary>
    /// Add privilege async.
    /// </summary>

    public Task AddPrivilegeAsync(PrivilegeDefinition privilege, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeDefinition>().AddAsync(privilege, cancellationToken).AsTask();
    /// <summary>
    /// Update privilege.
    /// </summary>

    public void UpdatePrivilege(PrivilegeDefinition privilege) => dbContext.Set<PrivilegeDefinition>().Update(privilege);
    /// <summary>
    /// Remove privilege.
    /// </summary>

    public void RemovePrivilege(PrivilegeDefinition privilege) => dbContext.Set<PrivilegeDefinition>().Remove(privilege);
    /// <summary>
    /// Get category by id async.
    /// </summary>

    public Task<PrivilegeCategory?> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeCategory>().FirstOrDefaultAsync(x => x.Id == categoryId, cancellationToken);
    /// <summary>
    /// Get category by name async.
    /// </summary>

    public Task<PrivilegeCategory?> GetCategoryByNameAsync(string categoryName, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeCategory>().FirstOrDefaultAsync(x => x.Name == categoryName.Trim(), cancellationToken);
    /// <summary>
    /// Get categories async.
    /// </summary>

    public Task<IReadOnlyList<PrivilegeCategory>> GetCategoriesAsync(CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeCategory>().OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync(cancellationToken).ContinueWith(t => (IReadOnlyList<PrivilegeCategory>)t.Result, cancellationToken);
    /// <summary>
    /// Add category async.
    /// </summary>

    public Task AddCategoryAsync(PrivilegeCategory category, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeCategory>().AddAsync(category, cancellationToken).AsTask();
    /// <summary>
    /// Add policy async.
    /// </summary>

    public Task AddPolicyAsync(PrivilegePolicy policy, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegePolicy>().AddAsync(policy, cancellationToken).AsTask();
    /// <summary>
    /// Get policy by id async.
    /// </summary>

    public Task<PrivilegePolicy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegePolicy>().FirstOrDefaultAsync(x => x.Id == policyId, cancellationToken);
    /// <summary>
    /// Get policy by name async.
    /// </summary>

    public Task<PrivilegePolicy?> GetPolicyByNameAsync(string policyName, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegePolicy>().FirstOrDefaultAsync(x => x.Name == policyName.Trim(), cancellationToken);
    /// <summary>
    /// Get policies by ids async.
    /// </summary>

    public async Task<IReadOnlyList<PrivilegePolicy>> GetPoliciesByIdsAsync(IEnumerable<Guid> policyIds, CancellationToken cancellationToken)
    {
        var ids = policyIds.Distinct().ToArray();
        return await dbContext.Set<PrivilegePolicy>().Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Add role privilege async.
    /// </summary>

    public Task AddRolePrivilegeAsync(RolePrivilege assignment, CancellationToken cancellationToken)
        => dbContext.Set<RolePrivilege>().AddAsync(assignment, cancellationToken).AsTask();
    /// <summary>
    /// Get role privilege async.
    /// </summary>

    public Task<RolePrivilege?> GetRolePrivilegeAsync(Guid roleId, Guid privilegeId, CancellationToken cancellationToken)
        => dbContext.Set<RolePrivilege>().FirstOrDefaultAsync(x => x.RoleId == roleId && x.PrivilegeId == privilegeId, cancellationToken);
    /// <summary>
    /// Get role privileges async.
    /// </summary>

    public async Task<IReadOnlyList<RolePrivilege>> GetRolePrivilegesAsync(Guid roleId, CancellationToken cancellationToken)
        => await dbContext.Set<RolePrivilege>().Where(x => x.RoleId == roleId).ToListAsync(cancellationToken);
    /// <summary>
    /// Add user privilege async.
    /// </summary>

    public Task AddUserPrivilegeAsync(UserPrivilege assignment, CancellationToken cancellationToken)
        => dbContext.Set<UserPrivilege>().AddAsync(assignment, cancellationToken).AsTask();
    /// <summary>
    /// Get user privilege async.
    /// </summary>

    public Task<UserPrivilege?> GetUserPrivilegeAsync(Guid userId, Guid privilegeId, CancellationToken cancellationToken)
        => dbContext.Set<UserPrivilege>()
            .OrderByDescending(x => x.GrantedAt)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.PrivilegeId == privilegeId && x.RevokedAt == null, cancellationToken);
    /// <summary>
    /// Get user privileges async.
    /// </summary>

    public async Task<IReadOnlyList<UserPrivilege>> GetUserPrivilegesAsync(Guid userId, CancellationToken cancellationToken)
        => await dbContext.Set<UserPrivilege>().Where(x => x.UserId == userId).ToListAsync(cancellationToken);
    /// <summary>
    /// Add role policy assignment async.
    /// </summary>

    public Task AddRolePolicyAssignmentAsync(RolePrivilegePolicy assignment, CancellationToken cancellationToken)
        => dbContext.Set<RolePrivilegePolicy>().AddAsync(assignment, cancellationToken).AsTask();
    /// <summary>
    /// Add user policy assignment async.
    /// </summary>

    public Task AddUserPolicyAssignmentAsync(UserPrivilegePolicy assignment, CancellationToken cancellationToken)
        => dbContext.Set<UserPrivilegePolicy>().AddAsync(assignment, cancellationToken).AsTask();
    /// <summary>
    /// Get role policy assignments async.
    /// </summary>

    public async Task<IReadOnlyList<RolePrivilegePolicy>> GetRolePolicyAssignmentsAsync(Guid roleId, CancellationToken cancellationToken)
        => await dbContext.Set<RolePrivilegePolicy>().Where(x => x.RoleId == roleId).ToListAsync(cancellationToken);
    /// <summary>
    /// Get user policy assignments async.
    /// </summary>

    public async Task<IReadOnlyList<UserPrivilegePolicy>> GetUserPolicyAssignmentsAsync(Guid userId, CancellationToken cancellationToken)
        => await dbContext.Set<UserPrivilegePolicy>().Where(x => x.UserId == userId).ToListAsync(cancellationToken);
    /// <summary>
    /// Get user role ids async.
    /// </summary>

    public async Task<IReadOnlyList<Guid>> GetUserRoleIdsAsync(Guid userId, CancellationToken cancellationToken)
        => await dbContext.Set<IdentityUserRole<Guid>>()
            .Where(x => x.UserId == userId)
            .Select(x => x.RoleId)
            .ToListAsync(cancellationToken);
    /// <summary>
    /// Get user role names async.
    /// </summary>

    public async Task<IReadOnlyList<string>> GetUserRoleNamesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var query =
            from userRole in dbContext.Set<IdentityUserRole<Guid>>()
            join role in dbContext.Set<IdentityRole<Guid>>() on userRole.RoleId equals role.Id
            where userRole.UserId == userId
            select role.Name ?? string.Empty;

        return await query.ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Get roles by ids async.
    /// </summary>

    public async Task<IReadOnlyList<IdentityRole<Guid>>> GetRolesByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken)
    {
        var ids = roleIds.Distinct().ToArray();
        return await dbContext.Set<IdentityRole<Guid>>().Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Get role by id async.
    /// </summary>

    public Task<IdentityRole<Guid>?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken)
        => dbContext.Set<IdentityRole<Guid>>().FirstOrDefaultAsync(x => x.Id == roleId, cancellationToken);
    /// <summary>
    /// Get role by name async.
    /// </summary>

    public Task<IdentityRole<Guid>?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
        => dbContext.Set<IdentityRole<Guid>>().FirstOrDefaultAsync(x => x.Name == roleName.Trim(), cancellationToken);
    /// <summary>
    /// Add privilege request async.
    /// </summary>

    public Task AddPrivilegeRequestAsync(PrivilegeRequest request, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeRequest>().AddAsync(request, cancellationToken).AsTask();
    /// <summary>
    /// Get privilege request by id async.
    /// </summary>

    public Task<PrivilegeRequest?> GetPrivilegeRequestByIdAsync(Guid requestId, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeRequest>().FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);
    /// <summary>
    /// Get privilege requests for user async.
    /// </summary>

    public async Task<IReadOnlyList<PrivilegeRequest>> GetPrivilegeRequestsForUserAsync(Guid userId, CancellationToken cancellationToken)
        => await dbContext.Set<PrivilegeRequest>().Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
}
