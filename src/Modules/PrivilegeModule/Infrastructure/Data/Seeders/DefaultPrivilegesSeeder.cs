using Alphabet.Domain.Entities.Privilege;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.Privilege;
using Alphabet.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alphabet.Infrastructure.Data.Seeders;

/// <summary>
/// Seeds default privilege categories, privileges, and role mappings.
/// </summary>
public static class DefaultPrivilegesSeeder
{
    private static readonly (string Name, string DisplayName, string Category, string[] DependsOn)[] DefaultPrivileges =
    [
        ("user.view", "View Users", "User Management", []),
        ("user.create", "Create Users", "User Management", ["user.view"]),
        ("user.edit", "Edit Users", "User Management", ["user.view"]),
        ("user.delete", "Delete Users", "User Management", ["user.view", "user.edit"]),
        ("role.view", "View Roles", "Role Management", []),
        ("role.create", "Create Roles", "Role Management", ["role.view"]),
        ("role.assign", "Assign Roles", "Role Management", ["user.view", "role.view"]),
        ("privilege.view", "View Privileges", "Privilege Management", []),
        ("privilege.assign", "Assign Privileges", "Privilege Management", ["privilege.view", "role.view"]),
        ("audit.view", "View Audit Logs", "Auditing", []),
        ("report.generate", "Generate Reports", "Reporting", []),
        ("report.export", "Export Reports", "Reporting", ["report.generate"]),
        ("asset.view", "View Assets", "Asset Management", []),
        ("asset.create", "Create Assets", "Asset Management", ["asset.view"]),
        ("asset.update", "Update Assets", "Asset Management", ["asset.view"]),
        ("asset.delete", "Retire Assets", "Asset Management", ["asset.view"]),
        ("asset.assign", "Assign Assets", "Asset Management", ["asset.view"]),
        ("asset.unassign", "Return Assets", "Asset Management", ["asset.view"]),
        ("asset.transfer", "Transfer Assets", "Asset Management", ["asset.assign"]),
        ("asset.maintenance", "Manage Asset Maintenance", "Asset Management", ["asset.view"]),
        ("asset.audit", "Audit Assets", "Asset Management", ["asset.view"]),
        ("asset.admin", "Administer Asset Inventory", "Asset Management", ["asset.view", "asset.update"]),
        ("workflow.initiate", "Initiate Workflows", "Asset Workflow", ["asset.view"]),
        ("workflow.approve", "Approve Workflows", "Asset Workflow", ["workflow.initiate"]),
        ("workflow.delegate", "Delegate Workflows", "Asset Workflow", ["workflow.approve"]),
        ("report.generate", "Generate Reports", "Reporting", []),
        ("report.export", "Export Reports", "Reporting", ["report.generate"]),
        ("leave.request.create", "Create Leave Requests", "Leave Management", []),
        ("leave.request.view", "View Leave Requests", "Leave Management", []),
        ("leave.request.cancel", "Cancel Leave Requests", "Leave Management", ["leave.request.view"]),
        ("leave.request.modify", "Modify Leave Requests", "Leave Management", ["leave.request.view"]),
        ("leave.approve.level1", "Approve Leave Level 1", "Leave Management", ["leave.request.view"]),
        ("leave.approve.level2", "Approve Leave Level 2", "Leave Management", ["leave.approve.level1"]),
        ("leave.approve.levelN", "Approve Leave Any Level", "Leave Management", ["leave.approve.level2"]),
        ("leave.approve.batch", "Batch Approve Leave", "Leave Management", ["leave.approve.level1"]),
        ("leave.delegate.create", "Create Leave Delegations", "Leave Management", ["leave.request.view"]),
        ("leave.delegate.revoke", "Revoke Leave Delegations", "Leave Management", ["leave.delegate.create"]),
        ("leave.balance.view", "View Own Leave Balance", "Leave Management", []),
        ("leave.balance.view.any", "View Any Leave Balance", "Leave Management", ["leave.balance.view"]),
        ("leave.balance.adjust", "Adjust Leave Balance", "Leave Management", ["leave.balance.view.any"]),
        ("leave.type.manage", "Manage Leave Types", "Leave Management", []),
        ("leave.chain.manage", "Manage Leave Approval Chains", "Leave Management", []),
        ("leave.report.view", "View Leave Reports", "Leave Management", []),
        ("leave.audit.view", "View Leave Audit Logs", "Leave Management", ["leave.request.view"]),
        ("leave.admin", "Administer Leave Management", "Leave Management", ["leave.type.manage", "leave.chain.manage", "leave.balance.adjust", "leave.audit.view"])
    ];

    /// <summary>
    /// Seeds privilege data.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPrivilegeRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var settings = scope.ServiceProvider.GetRequiredService<IOptions<PrivilegeSettings>>().Value;
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Alphabet.PrivilegeSeed");

        foreach (var roleName in settings.AdminRoles
            .Concat(["UserManager", "Auditor", "Reporter"]))
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        foreach (var configuredSystemPrivilege in settings.SystemPrivileges)
        {
            if (await repository.GetPrivilegeByNameAsync(configuredSystemPrivilege.Name, CancellationToken.None) is not null)
            {
                continue;
            }

            var category = await repository.GetCategoryByNameAsync(configuredSystemPrivilege.Category, CancellationToken.None)
                ?? PrivilegeCategory.Create(configuredSystemPrivilege.Category, null, null, 0);

            if (category.Id == Guid.Empty)
            {
                await repository.AddCategoryAsync(category, CancellationToken.None);
            }

            var privilege = Privilege.Create(
                configuredSystemPrivilege.Name,
                configuredSystemPrivilege.DisplayName,
                null,
                category.Id,
                "System",
                ["Read"],
                configuredSystemPrivilege.IsGlobal,
                null,
                "system");

            await repository.AddPrivilegeAsync(privilege, CancellationToken.None);
        }

        foreach (var item in DefaultPrivileges)
        {
            var category = await repository.GetCategoryByNameAsync(item.Category, CancellationToken.None)
                ?? PrivilegeCategory.Create(item.Category, null, null, 0);

            if (category.Id == Guid.Empty)
            {
                await repository.AddCategoryAsync(category, CancellationToken.None);
            }

            var existing = await repository.GetPrivilegeByNameAsync(item.Name, CancellationToken.None);
            if (existing is not null)
            {
                continue;
            }

            var privilege = Privilege.Create(item.Name, item.DisplayName, null, category.Id, item.Category, ["Read"], false, null, "system");
            privilege.ReplaceDependencies(item.DependsOn);
            await repository.AddPrivilegeAsync(privilege, CancellationToken.None);
        }

        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        await AssignRolePrivilegesAsync(repository, unitOfWork, roleManager, "Admin", ["user.view", "user.create", "user.edit", "user.delete", "role.view", "role.create", "role.assign", "privilege.view", "privilege.assign", "audit.view", "report.generate", "report.export", "asset.view", "asset.create", "asset.update", "asset.delete", "asset.assign", "asset.unassign", "asset.transfer", "asset.maintenance", "asset.audit", "asset.admin", "workflow.initiate", "workflow.approve", "workflow.delegate"], logger);
        await AssignRolePrivilegesAsync(repository, unitOfWork, roleManager, "Admin", ["user.view", "user.create", "user.edit", "user.delete", "role.view", "role.create", "role.assign", "privilege.view", "privilege.assign", "audit.view", "report.generate", "report.export", "leave.request.create", "leave.request.view", "leave.request.cancel", "leave.request.modify", "leave.approve.level1", "leave.approve.level2", "leave.approve.levelN", "leave.approve.batch", "leave.delegate.create", "leave.delegate.revoke", "leave.balance.view", "leave.balance.view.any", "leave.balance.adjust", "leave.type.manage", "leave.chain.manage", "leave.report.view", "leave.audit.view", "leave.admin"], logger);
        await AssignRolePrivilegesAsync(repository, unitOfWork, roleManager, "UserManager", ["user.view", "user.create", "user.edit", "user.delete", "role.view"], logger);
        await AssignRolePrivilegesAsync(repository, unitOfWork, roleManager, "Auditor", ["audit.view", "user.view", "asset.audit", "asset.view"], logger);
        await AssignRolePrivilegesAsync(repository, unitOfWork, roleManager, "Reporter", ["report.generate", "report.export", "asset.view"], logger);
    }
    /// <summary>
    /// Assign role privileges async.
    /// </summary>

    private static async Task AssignRolePrivilegesAsync(
        IPrivilegeRepository repository,
        IUnitOfWork unitOfWork,
        RoleManager<IdentityRole<Guid>> roleManager,
        string roleName,
        IReadOnlyCollection<string> privilegeNames,
        ILogger logger)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            return;
        }

        foreach (var privilegeName in privilegeNames)
        {
            var privilege = await repository.GetPrivilegeByNameAsync(privilegeName, CancellationToken.None);
            if (privilege is null)
            {
                continue;
            }

            var existing = await repository.GetRolePrivilegeAsync(role.Id, privilege.Id, CancellationToken.None);
            if (existing is not null)
            {
                continue;
            }

            await repository.AddRolePrivilegeAsync(RolePrivilege.Create(role.Id, privilege.Id, "system", null), CancellationToken.None);
        }

        await unitOfWork.SaveChangesAsync(CancellationToken.None);
        logger.LogInformation("Seeded privilege assignments for role {RoleName}", roleName);
    }
}
