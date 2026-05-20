using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.Privilege;
using Microsoft.AspNetCore.Authorization;

namespace Alphabet.Infrastructure.Security;

/// <summary>
/// Evaluates privilege authorization requirements at runtime.
/// </summary>
public sealed class PrivilegeAuthorizationHandler(
    IPrivilegeService privilegeService,
    ICurrentUserService currentUserService)
    : AuthorizationHandler<PrivilegeRequirement>
{
    /// <summary>
    /// Handle requirement async.
    /// </summary>
    protected override async Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    PrivilegeRequirement requirement)
    {
        if (currentUserService.UserId is null)
        {
            return;
        }

        var result = await privilegeService.CheckPrivilegeAsync(
            currentUserService.UserId.Value,
            requirement.Privileges.FirstOrDefault() ?? string.Empty,
            CancellationToken.None);

        if (requirement.Privileges.Count == 1)
        {
            if (result.IsSuccess && result.Value?.HasPrivilege == true)
            {
                context.Succeed(requirement);
            }

            return;
        }

        var batch = await privilegeService.BatchCheckPrivilegesAsync(
            currentUserService.UserId.Value,
            requirement.Privileges,
            CancellationToken.None);

        var isAuthorized = requirement.RequireAll
            ? requirement.Privileges.All(name => batch.TryGetValue(name, out var allowed) && allowed)
            : requirement.Privileges.Any(name => batch.TryGetValue(name, out var allowed) && allowed);

        if (isAuthorized)
        {
            context.Succeed(requirement);
        }
    }
}
