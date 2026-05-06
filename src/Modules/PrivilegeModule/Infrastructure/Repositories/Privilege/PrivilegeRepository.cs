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
    public async Task<bool> PrivilegeNameExistsAsync(string name, Guid? excludingId, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return await dbContext.Set<PrivilegeDefinition>()
            .AnyAsync(x => x.Name == normalizedName && (!excludingId.HasValue || x.Id != excludingId.Value), cancellationToken);
    }

    public Task<PrivilegeDefinition?> GetPrivilegeByIdAsync(Guid privilegeId, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeDefinition>().FirstOrDefaultAsync(x => x.Id == privilegeId, cancellationToken);

    public Task<PrivilegeDefinition?> GetPrivilegeByNameAsync(string name, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeDefinition>().FirstOrDefaultAsync(x => x.Name == name.Trim().ToLowerInvariant(), cancellationToken);

    public async Task<IReadOnlyList<PrivilegeDefinition>> GetPrivilegesByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken)
    {
        var normalized = names.Select(static x => x.Trim().ToLowerInvariant()).Distinct().ToArray();
        return await dbContext.Set<PrivilegeDefinition>().Where(x => normalized.Contains(x.Name)).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PrivilegeDefinition>> GetPrivilegesByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var list = ids.Distinct().ToArray();
        return await dbContext.Set<PrivilegeDefinition>().Where(x => list.Contains(x.Id)).ToListAsync(cancellationToken);
    }

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

    public Task AddPrivilegeAsync(PrivilegeDefinition privilege, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeDefinition>().AddAsync(privilege, cancellationToken).AsTask();

    public void UpdatePrivilege(PrivilegeDefinition privilege) => dbContext.Set<PrivilegeDefinition>().Update(privilege);

    public void RemovePrivilege(PrivilegeDefinition privilege) => dbContext.Set<PrivilegeDefinition>().Remove(privilege);

    public Task<PrivilegeCategory?> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeCategory>().FirstOrDefaultAsync(x => x.Id == categoryId, cancellationToken);

    public Task<PrivilegeCategory?> GetCategoryByNameAsync(string categoryName, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeCategory>().FirstOrDefaultAsync(x => x.Name == categoryName.Trim(), cancellationToken);

    public Task<IReadOnlyList<PrivilegeCategory>> GetCategoriesAsync(CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeCategory>().OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync(cancellationToken).ContinueWith(t => (IReadOnlyList<PrivilegeCategory>)t.Result, cancellationToken);

    public Task AddCategoryAsync(PrivilegeCategory category, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeCategory>().AddAsync(category, cancellationToken).AsTask();

    public Task AddPolicyAsync(PrivilegePolicy policy, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegePolicy>().AddAsync(policy, cancellationToken).AsTask();

    public Task<PrivilegePolicy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegePolicy>().FirstOrDefaultAsync(x => x.Id == policyId, cancellationToken);

    public Task<PrivilegePolicy?> GetPolicyByNameAsync(string policyName, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegePolicy>().FirstOrDefaultAsync(x => x.Name == policyName.Trim(), cancellationToken);

    public async Task<IReadOnlyList<PrivilegePolicy>> GetPoliciesByIdsAsync(IEnumerable<Guid> policyIds, CancellationToken cancellationToken)
    {
        var ids = policyIds.Distinct().ToArray();
        return await dbContext.Set<PrivilegePolicy>().Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
    }

    public Task AddRolePrivilegeAsync(RolePrivilege assignment, CancellationToken cancellationToken)
        => dbContext.Set<RolePrivilege>().AddAsync(assignment, cancellationToken).AsTask();

    public Task<RolePrivilege?> GetRolePrivilegeAsync(Guid roleId, Guid privilegeId, CancellationToken cancellationToken)
        => dbContext.Set<RolePrivilege>().FirstOrDefaultAsync(x => x.RoleId == roleId && x.PrivilegeId == privilegeId, cancellationToken);

    public async Task<IReadOnlyList<RolePrivilege>> GetRolePrivilegesAsync(Guid roleId, CancellationToken cancellationToken)
        => await dbContext.Set<RolePrivilege>().Where(x => x.RoleId == roleId).ToListAsync(cancellationToken);

    public Task AddUserPrivilegeAsync(UserPrivilege assignment, CancellationToken cancellationToken)
        => dbContext.Set<UserPrivilege>().AddAsync(assignment, cancellationToken).AsTask();

    public Task<UserPrivilege?> GetUserPrivilegeAsync(Guid userId, Guid privilegeId, CancellationToken cancellationToken)
        => dbContext.Set<UserPrivilege>()
            .OrderByDescending(x => x.GrantedAt)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.PrivilegeId == privilegeId && x.RevokedAt == null, cancellationToken);

    public async Task<IReadOnlyList<UserPrivilege>> GetUserPrivilegesAsync(Guid userId, CancellationToken cancellationToken)
        => await dbContext.Set<UserPrivilege>().Where(x => x.UserId == userId).ToListAsync(cancellationToken);

    public Task AddRolePolicyAssignmentAsync(RolePrivilegePolicy assignment, CancellationToken cancellationToken)
        => dbContext.Set<RolePrivilegePolicy>().AddAsync(assignment, cancellationToken).AsTask();

    public Task AddUserPolicyAssignmentAsync(UserPrivilegePolicy assignment, CancellationToken cancellationToken)
        => dbContext.Set<UserPrivilegePolicy>().AddAsync(assignment, cancellationToken).AsTask();

    public async Task<IReadOnlyList<RolePrivilegePolicy>> GetRolePolicyAssignmentsAsync(Guid roleId, CancellationToken cancellationToken)
        => await dbContext.Set<RolePrivilegePolicy>().Where(x => x.RoleId == roleId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserPrivilegePolicy>> GetUserPolicyAssignmentsAsync(Guid userId, CancellationToken cancellationToken)
        => await dbContext.Set<UserPrivilegePolicy>().Where(x => x.UserId == userId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetUserRoleIdsAsync(Guid userId, CancellationToken cancellationToken)
        => await dbContext.Set<IdentityUserRole<Guid>>()
            .Where(x => x.UserId == userId)
            .Select(x => x.RoleId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> GetUserRoleNamesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var query =
            from userRole in dbContext.Set<IdentityUserRole<Guid>>()
            join role in dbContext.Set<IdentityRole<Guid>>() on userRole.RoleId equals role.Id
            where userRole.UserId == userId
            select role.Name ?? string.Empty;

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<IdentityRole<Guid>>> GetRolesByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken)
    {
        var ids = roleIds.Distinct().ToArray();
        return await dbContext.Set<IdentityRole<Guid>>().Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
    }

    public Task<IdentityRole<Guid>?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken)
        => dbContext.Set<IdentityRole<Guid>>().FirstOrDefaultAsync(x => x.Id == roleId, cancellationToken);

    public Task<IdentityRole<Guid>?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
        => dbContext.Set<IdentityRole<Guid>>().FirstOrDefaultAsync(x => x.Name == roleName.Trim(), cancellationToken);

    public Task AddPrivilegeRequestAsync(PrivilegeRequest request, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeRequest>().AddAsync(request, cancellationToken).AsTask();

    public Task<PrivilegeRequest?> GetPrivilegeRequestByIdAsync(Guid requestId, CancellationToken cancellationToken)
        => dbContext.Set<PrivilegeRequest>().FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);

    public async Task<IReadOnlyList<PrivilegeRequest>> GetPrivilegeRequestsForUserAsync(Guid userId, CancellationToken cancellationToken)
        => await dbContext.Set<PrivilegeRequest>().Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
}
