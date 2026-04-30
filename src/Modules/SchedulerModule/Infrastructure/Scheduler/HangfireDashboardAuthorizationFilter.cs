using Hangfire.Dashboard;

namespace Alphabet.Infrastructure.Scheduler;

/// <summary>
/// Protects the Hangfire dashboard behind the Admin role.
/// </summary>
public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true && httpContext.User.IsInRole("Admin");
    }
}
