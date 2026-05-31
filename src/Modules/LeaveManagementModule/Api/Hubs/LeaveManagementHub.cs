using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Alphabet.Modules.LeaveManagementModule.Api.Hubs;

/// <summary>
/// SignalR hub for real-time leave request, approval, and balance notifications.
/// </summary>
[Authorize]
public sealed class LeaveManagementHub : Hub
{
    /// <summary>
    /// Adds the connected client to a user-specific leave notification group.
    /// </summary>
    public Task JoinUserGroup(string userId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"leave-user-{userId}");
    }
}
