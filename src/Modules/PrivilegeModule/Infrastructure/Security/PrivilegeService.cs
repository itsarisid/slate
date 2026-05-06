using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.Privilege;
using Alphabet.Application.Features.Privilege.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Entities.Privilege;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.Privilege;
using Alphabet.Domain.Models;
using Alphabet.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alphabet.Infrastructure.Security;

/// <summary>
/// Provides privilege management, evaluation, and audit behavior.
/// </summary>
public sealed class PrivilegeService(
    IPrivilegeRepository privilegeRepository,
    IPrivilegeAuditRepository privilegeAuditRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    Repositories.Privilege.PrivilegeCacheRepository privilegeCacheRepository,
    IOptions<PrivilegeSettings> privilegeSettingsOptions,
    ILogger<PrivilegeService> logger)
    : IPrivilegeService, IPrivilegeEvaluationService
{
    private readonly PrivilegeSettings _settings = privilegeSettingsOptions.Value;

    public async Task<Result<Guid>> CreatePrivilegeAsync(
        string name,
        string displayName,
        string? description,
        string category,
        string? resourceType,
        IReadOnlyCollection<string> actions,
        bool isGlobal,
        IReadOnlyCollection<string> dependsOn,
        IReadOnlyDictionary<string, string?> attributes,
        CancellationToken cancellationToken)
    {
        if (await privilegeRepository.PrivilegeNameExistsAsync(name, null, cancellationToken))
        {
            return Result<Guid>.Failure($"Privilege '{name}' already exists.");
        }

        var categoryEntity = await GetOrCreateCategoryAsync(category, cancellationToken);
        var createdBy = currentUserService.Email ?? "system";
        var privilege = Privilege.Create(name, displayName, description, categoryEntity.Id, resourceType, actions, isGlobal, attributes, createdBy);
        privilege.ReplaceDependencies(dependsOn);

        var cycleResult = await ValidateDependencyGraphAsync(privilege, cancellationToken);
        if (cycleResult.IsFailure)
        {
            return Result<Guid>.Failure(cycleResult.Error ?? "Privilege dependencies are invalid.");
        }

        await privilegeRepository.AddPrivilegeAsync(privilege, cancellationToken);
        await WriteAuditAsync(null, privilege.Id, PrivilegeAction.Create, "Privilege", createdBy, cancellationToken, ("name", privilege.Name));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return privilege.Id;
    }

    public async Task<Result> UpdatePrivilegeAsync(
        Guid privilegeId,
        string displayName,
        string? description,
        string category,
        string? resourceType,
        IReadOnlyCollection<string> actions,
        IReadOnlyCollection<string> dependsOn,
        IReadOnlyDictionary<string, string?> attributes,
        CancellationToken cancellationToken)
    {
        var privilege = await privilegeRepository.GetPrivilegeByIdAsync(privilegeId, cancellationToken);
        if (privilege is null)
        {
            return Result.Failure("Privilege was not found.");
        }

        var categoryEntity = await GetOrCreateCategoryAsync(category, cancellationToken);
        privilege.UpdateDetails(displayName, description, categoryEntity.Id, resourceType, actions, attributes, currentUserService.Email ?? "system");
        privilege.ReplaceDependencies(dependsOn);

        var cycleResult = await ValidateDependencyGraphAsync(privilege, cancellationToken);
        if (cycleResult.IsFailure)
        {
            return Result.Failure(cycleResult.Error ?? "Privilege dependencies are invalid.");
        }

        privilegeRepository.UpdatePrivilege(privilege);
        await WriteAuditAsync(null, privilege.Id, PrivilegeAction.Update, "Privilege", currentUserService.Email ?? "system", cancellationToken, ("name", privilege.Name));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeletePrivilegeAsync(Guid privilegeId, bool hardDelete, CancellationToken cancellationToken)
    {
        var privilege = await privilegeRepository.GetPrivilegeByIdAsync(privilegeId, cancellationToken);
        if (privilege is null)
        {
            return Result.Failure("Privilege was not found.");
        }

        if (hardDelete)
        {
            var roleAssignments = await GetRoleAssignmentsByPrivilegeAsync(privilegeId, cancellationToken);
            var userAssignments = await GetUserAssignmentsByPrivilegeAsync(privilegeId, cancellationToken);
            if (roleAssignments.Count != 0 || userAssignments.Count != 0)
            {
                return Result.Failure("Cannot hard-delete a privilege that is still assigned.");
            }

            privilegeRepository.RemovePrivilege(privilege);
        }
        else
        {
            privilege.Deprecate(currentUserService.Email ?? "system");
            privilegeRepository.UpdatePrivilege(privilege);
        }

        await WriteAuditAsync(null, privilege.Id, PrivilegeAction.Delete, "Privilege", currentUserService.Email ?? "system", cancellationToken, ("hardDelete", hardDelete.ToString()));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> AssignPrivilegesToRoleAsync(Guid roleId, IReadOnlyCollection<Guid> privilegeIds, DateTimeOffset? expiresAt, CancellationToken cancellationToken)
    {
        var role = await privilegeRepository.GetRoleByIdAsync(roleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure("Role was not found.");
        }

        var privileges = await privilegeRepository.GetPrivilegesByIdsAsync(privilegeIds, cancellationToken);
        var grantedBy = currentUserService.Email ?? "system";

        foreach (var privilege in privileges)
        {
            var existing = await privilegeRepository.GetRolePrivilegeAsync(roleId, privilege.Id, cancellationToken);
            if (existing is not null && existing.IsActive)
            {
                continue;
            }

            await privilegeRepository.AddRolePrivilegeAsync(RolePrivilege.Create(roleId, privilege.Id, grantedBy, expiresAt), cancellationToken);
            await WriteAuditAsync(null, privilege.Id, PrivilegeAction.Assign, $"Role:{role.Name}", grantedBy, cancellationToken, ("roleId", roleId.ToString()));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RevokePrivilegeFromRoleAsync(Guid roleId, Guid privilegeId, CancellationToken cancellationToken)
    {
        var assignment = await privilegeRepository.GetRolePrivilegeAsync(roleId, privilegeId, cancellationToken);
        if (assignment is null)
        {
            return Result.Failure("Role privilege assignment was not found.");
        }

        assignment.Revoke();
        await WriteAuditAsync(null, privilegeId, PrivilegeAction.Revoke, $"Role:{roleId}", currentUserService.Email ?? "system", cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> BulkAssignPrivilegesToRolesAsync(IReadOnlyCollection<Guid> roleIds, IReadOnlyCollection<Guid> privilegeIds, string operation, DateTimeOffset? expiresAt, CancellationToken cancellationToken)
    {
        var normalizedOperation = operation.Trim().ToLowerInvariant();
        foreach (var roleId in roleIds)
        {
            if (normalizedOperation == "remove")
            {
                foreach (var privilegeId in privilegeIds)
                {
                    await RevokePrivilegeFromRoleAsync(roleId, privilegeId, cancellationToken);
                }
            }
            else
            {
                await AssignPrivilegesToRoleAsync(roleId, privilegeIds, expiresAt, cancellationToken);
            }
        }

        return Result.Success();
    }

    public async Task<Result> AssignPrivilegeToUserAsync(
        Guid userId,
        string privilegeNameOrId,
        PrivilegeEffect effect,
        DateTimeOffset? expiresAt,
        string? reason,
        CancellationToken cancellationToken)
    {
        if (await userManager.FindByIdAsync(userId.ToString()) is null)
        {
            return Result.Failure("User was not found.");
        }

        var privilege = await ResolvePrivilegeAsync(privilegeNameOrId, cancellationToken);
        if (privilege is null)
        {
            return Result.Failure("Privilege was not found.");
        }

        await privilegeRepository.AddUserPrivilegeAsync(
            UserPrivilege.Create(userId, privilege.Id, effect, currentUserService.Email ?? "system", expiresAt, reason),
            cancellationToken);

        await InvalidateUserCacheAsync(userId, cancellationToken);
        await WriteAuditAsync(userId, privilege.Id, PrivilegeAction.Assign, "User", currentUserService.Email ?? "system", cancellationToken, ("effect", effect.ToString()));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RevokePrivilegeFromUserAsync(Guid userId, Guid privilegeId, CancellationToken cancellationToken)
    {
        var assignment = await privilegeRepository.GetUserPrivilegeAsync(userId, privilegeId, cancellationToken);
        if (assignment is null)
        {
            return Result.Failure("User privilege assignment was not found.");
        }

        assignment.Revoke(currentUserService.Email ?? "system");
        await InvalidateUserCacheAsync(userId, cancellationToken);
        await WriteAuditAsync(userId, privilegeId, PrivilegeAction.Revoke, "User", currentUserService.Email ?? "system", cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<Guid>> CreateCategoryAsync(string name, string? description, Guid? parentCategoryId, int sortOrder, CancellationToken cancellationToken)
    {
        var existing = await privilegeRepository.GetCategoryByNameAsync(name, cancellationToken);
        if (existing is not null)
        {
            return existing.Id;
        }

        var category = PrivilegeCategory.Create(name, description, parentCategoryId, sortOrder);
        await privilegeRepository.AddCategoryAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return category.Id;
    }

    public async Task<IReadOnlyList<PrivilegeCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        var categories = await privilegeRepository.GetCategoriesAsync(cancellationToken);
        return BuildCategoryTree(categories, null);
    }

    public async Task<Result> MovePrivilegeCategoryAsync(Guid privilegeId, Guid categoryId, CancellationToken cancellationToken)
    {
        var privilege = await privilegeRepository.GetPrivilegeByIdAsync(privilegeId, cancellationToken);
        var category = await privilegeRepository.GetCategoryByIdAsync(categoryId, cancellationToken);
        if (privilege is null || category is null)
        {
            return Result.Failure("Privilege or category was not found.");
        }

        privilege.MoveToCategory(categoryId, currentUserService.Email ?? "system");
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<Guid>> CreatePolicyAsync(string name, string? description, IReadOnlyCollection<string> privileges, PrivilegePolicyCondition condition, CancellationToken cancellationToken)
    {
        if (await privilegeRepository.GetPolicyByNameAsync(name, cancellationToken) is not null)
        {
            return Result<Guid>.Failure("A privilege policy with the same name already exists.");
        }

        var policy = PrivilegePolicy.Create(name, description, privileges, condition);
        await privilegeRepository.AddPolicyAsync(policy, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return policy.Id;
    }

    public async Task<Result> AssignPolicyToRoleAsync(Guid roleId, Guid policyId, DateTimeOffset? expiresAt, CancellationToken cancellationToken)
    {
        if (await privilegeRepository.GetRoleByIdAsync(roleId, cancellationToken) is null)
        {
            return Result.Failure("Role was not found.");
        }

        if (await privilegeRepository.GetPolicyByIdAsync(policyId, cancellationToken) is null)
        {
            return Result.Failure("Privilege policy was not found.");
        }

        await privilegeRepository.AddRolePolicyAssignmentAsync(
            RolePrivilegePolicy.Create(roleId, policyId, currentUserService.Email ?? "system", expiresAt),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> AssignPolicyToUserAsync(Guid userId, Guid policyId, DateTimeOffset? expiresAt, CancellationToken cancellationToken)
    {
        if (await userManager.FindByIdAsync(userId.ToString()) is null)
        {
            return Result.Failure("User was not found.");
        }

        if (await privilegeRepository.GetPolicyByIdAsync(policyId, cancellationToken) is null)
        {
            return Result.Failure("Privilege policy was not found.");
        }

        await privilegeRepository.AddUserPolicyAssignmentAsync(
            UserPrivilegePolicy.Create(userId, policyId, currentUserService.Email ?? "system", expiresAt),
            cancellationToken);

        await InvalidateUserCacheAsync(userId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<Guid>> CreatePrivilegeRequestAsync(Guid userId, string privilegeNameOrId, string reason, int requestedDurationDays, string? approverEmail, CancellationToken cancellationToken)
    {
        if (requestedDurationDays > _settings.MaxPrivilegeRequestDurationDays)
        {
            return Result<Guid>.Failure($"Requested duration cannot exceed {_settings.MaxPrivilegeRequestDurationDays} days.");
        }

        var privilege = await ResolvePrivilegeAsync(privilegeNameOrId, cancellationToken);
        if (privilege is null)
        {
            return Result<Guid>.Failure("Privilege was not found.");
        }

        var request = PrivilegeRequest.Create(userId, privilege.Id, reason, requestedDurationDays, approverEmail);
        await privilegeRepository.AddPrivilegeRequestAsync(request, cancellationToken);
        await WriteAuditAsync(userId, privilege.Id, PrivilegeAction.Create, "Request", currentUserService.Email ?? "system", cancellationToken, ("type", "PrivilegeRequest"));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<Result> ApprovePrivilegeRequestAsync(Guid requestId, string? notes, CancellationToken cancellationToken)
    {
        var request = await privilegeRepository.GetPrivilegeRequestByIdAsync(requestId, cancellationToken);
        if (request is null)
        {
            return Result.Failure("Privilege request was not found.");
        }

        if (currentUserService.UserId is null)
        {
            return Result.Failure("An authenticated approver is required.");
        }

        request.Approve(currentUserService.UserId.Value, request.RequestedDurationDays, notes);
        await privilegeRepository.AddUserPrivilegeAsync(
            UserPrivilege.Create(
                request.UserId,
                request.PrivilegeId,
                PrivilegeEffect.Allow,
                currentUserService.Email ?? "system",
                request.ExpiresAt,
                $"Approved privilege request {request.Id}"),
            cancellationToken);

        await InvalidateUserCacheAsync(request.UserId, cancellationToken);
        await WriteAuditAsync(request.UserId, request.PrivilegeId, PrivilegeAction.Approve, "Request", currentUserService.Email ?? "system", cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DenyPrivilegeRequestAsync(Guid requestId, string? notes, CancellationToken cancellationToken)
    {
        var request = await privilegeRepository.GetPrivilegeRequestByIdAsync(requestId, cancellationToken);
        if (request is null)
        {
            return Result.Failure("Privilege request was not found.");
        }

        if (currentUserService.UserId is null)
        {
            return Result.Failure("An authenticated approver is required.");
        }

        request.Deny(currentUserService.UserId.Value, notes);
        await WriteAuditAsync(request.UserId, request.PrivilegeId, PrivilegeAction.Deny, "Request", currentUserService.Email ?? "system", cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<PagedResponseDto<PrivilegeDto>> GetPrivilegesAsync(int pageNumber, int pageSize, string? category, string? search, bool includeDeprecated, CancellationToken cancellationToken)
    {
        var result = await privilegeRepository.GetPrivilegesAsync(pageNumber, pageSize, category, search, includeDeprecated, cancellationToken);
        var categories = await privilegeRepository.GetCategoriesAsync(cancellationToken);
        var categoryLookup = categories.ToDictionary(x => x.Id, x => x.Name);
        return new PagedResponseDto<PrivilegeDto>(
            result.Items.Select(item => MapPrivilege(item, categoryLookup)).ToArray(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize);
    }

    public async Task<Result<PrivilegeDto>> GetPrivilegeByIdAsync(Guid privilegeId, CancellationToken cancellationToken)
    {
        var privilege = await privilegeRepository.GetPrivilegeByIdAsync(privilegeId, cancellationToken);
        if (privilege is null)
        {
            return Result<PrivilegeDto>.Failure("Privilege was not found.");
        }

        var categories = await privilegeRepository.GetCategoriesAsync(cancellationToken);
        return MapPrivilege(privilege, categories.ToDictionary(x => x.Id, x => x.Name));
    }

    public async Task<IReadOnlyList<PrivilegeAssignmentDto>> GetRolePrivilegesAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var privileges = await privilegeRepository.GetRolePrivilegesAsync(roleId, cancellationToken);
        var definitions = await privilegeRepository.GetPrivilegesByIdsAsync(privileges.Select(x => x.PrivilegeId), cancellationToken);
        var definitionLookup = definitions.ToDictionary(x => x.Id);

        return privileges
            .Select(x =>
            {
                var privilege = definitionLookup[x.PrivilegeId];
                return new PrivilegeAssignmentDto(x.PrivilegeId, privilege.Name, privilege.DisplayName, "Role", x.GrantedAt, x.GrantedBy, x.ExpiresAt, x.IsActive);
            })
            .ToArray();
    }

    public async Task<IReadOnlyList<UserEffectivePrivilegeDto>> GetUserEffectivePrivilegesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await GetEffectivePrivilegesInternalAsync(userId, cancellationToken);
    }

    public async Task<Result<PrivilegeCheckResultDto>> CheckPrivilegeAsync(Guid userId, string privilegeName, CancellationToken cancellationToken)
    {
        var effective = await GetEffectivePrivilegesInternalAsync(userId, cancellationToken);
        var privilege = effective.FirstOrDefault(x => string.Equals(x.PrivilegeName, privilegeName, StringComparison.OrdinalIgnoreCase));

        var result = privilege is not null && privilege.IsGranted
            ? new PrivilegeCheckResultDto(true, privilege.Source, privilege.ExpiresAt, privilege.Reason)
            : new PrivilegeCheckResultDto(false, "None", null, "Privilege was not granted.");

        await WriteAuditAsync(userId, null, PrivilegeAction.Check, result.Source, currentUserService.Email ?? userId.ToString(), cancellationToken, ("privilege", privilegeName), ("allowed", result.HasPrivilege.ToString()));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<IReadOnlyDictionary<string, bool>> BatchCheckPrivilegesAsync(Guid userId, IReadOnlyCollection<string> privilegeNames, CancellationToken cancellationToken)
    {
        var effective = await GetEffectivePrivilegesInternalAsync(userId, cancellationToken);
        var allowed = effective.Where(x => x.IsGranted).Select(x => x.PrivilegeName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return privilegeNames.ToDictionary(name => name, name => allowed.Contains(name), StringComparer.OrdinalIgnoreCase);
    }

    public async Task<PrivilegeAnalyticsDto> GetAnalyticsAsync(CancellationToken cancellationToken)
    {
        var logs = await privilegeAuditRepository.SearchAsync(null, null, null, DateTimeOffset.UtcNow.AddDays(-30), null, 5000, 0, cancellationToken);
        var mostUsed = logs
            .Where(x => x.Action == PrivilegeAction.Check && x.PrivilegeId.HasValue)
            .GroupBy(x => x.PrivilegeId!.Value)
            .Select(group => new { PrivilegeId = group.Key, UsageCount = group.Count(), UniqueUsers = group.Select(x => x.UserId).Where(x => x.HasValue).Distinct().Count() })
            .OrderByDescending(x => x.UsageCount)
            .Take(10)
            .ToArray();

        var privileges = await privilegeRepository.GetPrivilegesAsync(1, 5000, null, null, true, cancellationToken);
        var privilegeLookup = privileges.Items.ToDictionary(x => x.Id, x => x.Name);

        var mostUsedDtos = mostUsed
            .Select(x => new PrivilegeUsageMetricDto(privilegeLookup.GetValueOrDefault(x.PrivilegeId, x.PrivilegeId.ToString()), x.UsageCount, x.UniqueUsers))
            .ToArray();

        var checkedPrivilegeIds = mostUsed.Select(x => x.PrivilegeId).ToHashSet();
        var unusedPrivileges = privileges.Items.Where(x => !checkedPrivilegeIds.Contains(x.Id)).Select(x => x.Name).ToArray();

        var trend = logs
            .Where(x => x.Action == PrivilegeAction.Assign)
            .GroupBy(x => DateOnly.FromDateTime(x.PerformedAt.UtcDateTime.Date))
            .OrderBy(x => x.Key)
            .Select(group => new PrivilegeAssignmentTrendDto(group.Key, group.Count()))
            .ToArray();

        return new PrivilegeAnalyticsDto(mostUsedDtos, unusedPrivileges, trend);
    }

    public async Task<IReadOnlyList<PrivilegeAuditLogDto>> GetAuditLogsAsync(
        Guid? userId,
        Guid? privilegeId,
        PrivilegeAction? action,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int take,
        int skip,
        CancellationToken cancellationToken)
    {
        var logs = await privilegeAuditRepository.SearchAsync(userId, privilegeId, action, from, to, take, skip, cancellationToken);
        return logs.Select(MapAuditLog).ToArray();
    }

    public async Task<bool> HasPrivilegeAsync(Guid userId, IEnumerable<string> privileges, bool requireAll, CancellationToken cancellationToken)
    {
        var batch = await BatchCheckPrivilegesAsync(userId, privileges.ToArray(), cancellationToken);
        return requireAll
            ? privileges.All(name => batch.TryGetValue(name, out var allowed) && allowed)
            : privileges.Any(name => batch.TryGetValue(name, out var allowed) && allowed);
    }

    private async Task<PrivilegeCategory> GetOrCreateCategoryAsync(string category, CancellationToken cancellationToken)
    {
        var existing = await privilegeRepository.GetCategoryByNameAsync(category, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var created = PrivilegeCategory.Create(category, null, null, 0);
        await privilegeRepository.AddCategoryAsync(created, cancellationToken);
        return created;
    }

    private async Task<Result> ValidateDependencyGraphAsync(Privilege privilege, CancellationToken cancellationToken)
    {
        var graph = new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [privilege.Name] = privilege.DependsOn
        };

        var toLoad = new Queue<string>(privilege.DependsOn);
        while (toLoad.Count != 0)
        {
            var currentName = toLoad.Dequeue();
            if (graph.ContainsKey(currentName))
            {
                continue;
            }

            var currentPrivilege = await privilegeRepository.GetPrivilegeByNameAsync(currentName, cancellationToken);
            if (currentPrivilege is null)
            {
                continue;
            }

            graph[currentName] = currentPrivilege.DependsOn;
            foreach (var dependency in currentPrivilege.DependsOn)
            {
                toLoad.Enqueue(dependency);
            }
        }

        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        bool HasCycle(string node)
        {
            if (visited.Contains(node))
            {
                return false;
            }

            if (!visiting.Add(node))
            {
                return true;
            }

            if (graph.TryGetValue(node, out var dependencies))
            {
                foreach (var dependency in dependencies)
                {
                    if (HasCycle(dependency))
                    {
                        return true;
                    }
                }
            }

            visiting.Remove(node);
            visited.Add(node);
            return false;
        }

        return HasCycle(privilege.Name)
            ? Result.Failure("Privilege dependencies contain a circular reference.")
            : Result.Success();
    }

    private async Task<IReadOnlyList<UserEffectivePrivilegeDto>> GetEffectivePrivilegesInternalAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cacheKey = $"privileges:user:{userId}";
        if (_settings.CacheEnabled)
        {
            var cached = await privilegeCacheRepository.GetAsync<IReadOnlyList<UserEffectivePrivilegeDto>>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }
        }

        var now = DateTimeOffset.UtcNow;
        var roleIds = await privilegeRepository.GetUserRoleIdsAsync(userId, cancellationToken);
        var roleNames = await privilegeRepository.GetUserRoleNamesAsync(userId, cancellationToken);

        var effective = new Dictionary<string, UserEffectivePrivilegeDto>(StringComparer.OrdinalIgnoreCase);

        foreach (var roleId in roleIds)
        {
            var role = await privilegeRepository.GetRoleByIdAsync(roleId, cancellationToken);
            if (role is null)
            {
                continue;
            }

            var rolePrivileges = await privilegeRepository.GetRolePrivilegesAsync(roleId, cancellationToken) ?? [];
            var activeRolePrivileges = rolePrivileges.Where(x => x.IsActive && (!x.ExpiresAt.HasValue || x.ExpiresAt > now)).ToArray();
            var rolePrivilegeDefinitions = await privilegeRepository.GetPrivilegesByIdsAsync(activeRolePrivileges.Select(x => x.PrivilegeId), cancellationToken) ?? [];

            foreach (var privilege in rolePrivilegeDefinitions)
            {
                var assignment = activeRolePrivileges.FirstOrDefault(x => x.PrivilegeId == privilege.Id);
                if (assignment is null)
                {
                    continue;
                }

                effective[privilege.Name] = new UserEffectivePrivilegeDto(privilege.Name, true, $"Role: {role.Name}", assignment.ExpiresAt, null);
            }

            var rolePolicies = await privilegeRepository.GetRolePolicyAssignmentsAsync(roleId, cancellationToken) ?? [];
            var activeRolePolicies = rolePolicies.Where(x => !x.ExpiresAt.HasValue || x.ExpiresAt > now).ToArray();
            var policyDefinitions = await privilegeRepository.GetPoliciesByIdsAsync(activeRolePolicies.Select(x => x.PolicyId), cancellationToken) ?? [];
            foreach (var policy in policyDefinitions)
            {
                var policyAssignment = activeRolePolicies.FirstOrDefault(x => x.PolicyId == policy.Id);
                if (policyAssignment is null)
                {
                    continue;
                }

                foreach (var privilegeName in policy.PrivilegeNames)
                {
                    effective[privilegeName] = new UserEffectivePrivilegeDto(privilegeName, true, $"Policy: {policy.Name} via role {role.Name}", policyAssignment.ExpiresAt, null);
                }
            }
        }

        var userPolicies = await privilegeRepository.GetUserPolicyAssignmentsAsync(userId, cancellationToken) ?? [];
        var activeUserPolicies = userPolicies.Where(x => !x.ExpiresAt.HasValue || x.ExpiresAt > now).ToArray();
        var userPolicyDefinitions = await privilegeRepository.GetPoliciesByIdsAsync(activeUserPolicies.Select(x => x.PolicyId), cancellationToken) ?? [];
        foreach (var policy in userPolicyDefinitions)
        {
            var policyAssignment = activeUserPolicies.FirstOrDefault(x => x.PolicyId == policy.Id);
            if (policyAssignment is null)
            {
                continue;
            }

            foreach (var privilegeName in policy.PrivilegeNames)
            {
                effective[privilegeName] = new UserEffectivePrivilegeDto(privilegeName, true, $"Policy: {policy.Name}", policyAssignment.ExpiresAt, null);
            }
        }

        var userPrivileges = await privilegeRepository.GetUserPrivilegesAsync(userId, cancellationToken) ?? [];
        var activeUserPrivileges = userPrivileges.Where(x => x.IsActive(now)).OrderBy(x => x.Effect == PrivilegeEffect.Deny ? 0 : 1).ToArray();
        var privilegeDefinitions = await privilegeRepository.GetPrivilegesByIdsAsync(activeUserPrivileges.Select(x => x.PrivilegeId), cancellationToken) ?? [];
        var definitionLookup = privilegeDefinitions.ToDictionary(x => x.Id);

        foreach (var assignment in activeUserPrivileges)
        {
            if (!definitionLookup.TryGetValue(assignment.PrivilegeId, out var privilege))
            {
                continue;
            }

            effective[privilege.Name] = new UserEffectivePrivilegeDto(
                privilege.Name,
                assignment.Effect == PrivilegeEffect.Allow,
                $"Direct {assignment.Effect}",
                assignment.ExpiresAt,
                assignment.Reason);
        }

        var grantedPrivileges = effective.Where(x => x.Value.IsGranted).Select(x => x.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var loadedPrivilegeDefinitions = await privilegeRepository.GetPrivilegesByNamesAsync(grantedPrivileges, cancellationToken) ?? [];
        var definitionsByName = privilegeDefinitions.Concat(loadedPrivilegeDefinitions)
            .DistinctBy(x => x.Name)
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var granted in effective.Values.Where(x => x.IsGranted).ToArray())
        {
            if (!definitionsByName.TryGetValue(granted.PrivilegeName, out var privilegeDefinition))
            {
                continue;
            }

            if (privilegeDefinition.DependsOn.Any() && privilegeDefinition.DependsOn.Any(dependency => !grantedPrivileges.Contains(dependency)))
            {
                effective[granted.PrivilegeName] = granted with { IsGranted = false, Source = $"{granted.Source} (Dependency missing)", Reason = "Required dependency privilege is missing." };
            }
        }

        var values = effective.Values.OrderBy(x => x.PrivilegeName).ToArray();
        if (_settings.CacheEnabled)
        {
            await privilegeCacheRepository.SetAsync(cacheKey, values, TimeSpan.FromMinutes(_settings.CacheDurationMinutes), cancellationToken);
        }

        logger.LogDebug("Resolved {Count} effective privileges for user {UserId} with roles {Roles}", values.Length, userId, string.Join(", ", roleNames));
        return values;
    }

    private async Task<Privilege?> ResolvePrivilegeAsync(string privilegeNameOrId, CancellationToken cancellationToken)
    {
        return Guid.TryParse(privilegeNameOrId, out var privilegeId)
            ? await privilegeRepository.GetPrivilegeByIdAsync(privilegeId, cancellationToken)
            : await privilegeRepository.GetPrivilegeByNameAsync(privilegeNameOrId, cancellationToken);
    }

    private static PrivilegeDto MapPrivilege(Privilege privilege, IReadOnlyDictionary<Guid, string> categoryLookup)
        => new(
            privilege.Id,
            privilege.Name,
            privilege.DisplayName,
            privilege.Description,
            privilege.CategoryId,
            privilege.CategoryId.HasValue && categoryLookup.TryGetValue(privilege.CategoryId.Value, out var categoryName) ? categoryName : null,
            privilege.ResourceType,
            privilege.AllowedActions,
            privilege.IsGlobal,
            privilege.IsDeprecated,
            privilege.DependsOn,
            new Dictionary<string, string?>(privilege.Attributes, StringComparer.OrdinalIgnoreCase),
            privilege.CreatedAt,
            privilege.CreatedBy,
            privilege.UpdatedAt);

    private static PrivilegeAuditLogDto MapAuditLog(PrivilegeAuditLog auditLog)
        => new(
            auditLog.Id,
            auditLog.UserId,
            auditLog.PrivilegeId,
            auditLog.Action.ToString(),
            auditLog.Source,
            auditLog.PerformedBy,
            auditLog.PerformedAt,
            auditLog.IpAddress,
            new Dictionary<string, string?>(auditLog.Metadata, StringComparer.OrdinalIgnoreCase));

    private async Task WriteAuditAsync(
        Guid? userId,
        Guid? privilegeId,
        PrivilegeAction action,
        string source,
        string performedBy,
        CancellationToken cancellationToken,
        params (string Key, string? Value)[] metadata)
    {
        if (!_settings.EnableAuditLogging)
        {
            return;
        }

        var log = PrivilegeAuditLog.Create(
            userId,
            privilegeId,
            action,
            source,
            performedBy,
            currentUserService.IpAddress,
            metadata.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase));

        await privilegeAuditRepository.AddAsync(log, cancellationToken);
    }

    private async Task InvalidateUserCacheAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (_settings.CacheEnabled)
        {
            await privilegeCacheRepository.RemoveAsync($"privileges:user:{userId}", cancellationToken);
        }
    }

    private static IReadOnlyList<PrivilegeCategoryDto> BuildCategoryTree(IReadOnlyList<PrivilegeCategory> categories, Guid? parentId)
    {
        return categories
            .Where(x => x.ParentCategoryId == parentId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new PrivilegeCategoryDto(x.Id, x.Name, x.Description, x.ParentCategoryId, x.SortOrder, BuildCategoryTree(categories, x.Id)))
            .ToArray();
    }

    private async Task<IReadOnlyList<RolePrivilege>> GetRoleAssignmentsByPrivilegeAsync(Guid privilegeId, CancellationToken cancellationToken)
    {
        var roles = await roleManager.Roles.ToListAsync(cancellationToken);
        var assignments = new List<RolePrivilege>();
        foreach (var role in roles)
        {
            var roleAssignments = await privilegeRepository.GetRolePrivilegesAsync(role.Id, cancellationToken);
            assignments.AddRange(roleAssignments.Where(x => x.PrivilegeId == privilegeId && x.IsActive));
        }

        return assignments;
    }

    private async Task<IReadOnlyList<UserPrivilege>> GetUserAssignmentsByPrivilegeAsync(Guid privilegeId, CancellationToken cancellationToken)
    {
        var users = await userManager.Users.Select(x => x.Id).ToListAsync(cancellationToken);
        var assignments = new List<UserPrivilege>();
        foreach (var userId in users)
        {
            var userAssignments = await privilegeRepository.GetUserPrivilegesAsync(userId, cancellationToken);
            assignments.AddRange(userAssignments.Where(x => x.PrivilegeId == privilegeId && x.RevokedAt is null));
        }

        return assignments;
    }
}
