using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Modules.ProductivityModule.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Alphabet.Modules.ProductivityModule.Api.Hubs;

/// <summary>
/// Provides real-time collaboration and notification channels for productivity features.
/// </summary>
[Authorize]
public sealed class ProductivityHub : Hub
{
    /// <summary>
    /// Joins a SignalR group for todo updates.
    /// </summary>
    public async Task JoinTodoGroup(string todoId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"todo:{todoId}");
    }

    /// <summary>
    /// Broadcasts a todo update to listeners.
    /// </summary>
    public async Task SendTodoUpdate(TodoDto todo)
    {
        await Clients.Group($"todo:{todo.Id}").SendAsync("TodoUpdated", todo);
    }

    /// <summary>
    /// Joins a SignalR group for note collaboration.
    /// </summary>
    public async Task JoinNoteGroup(string noteId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"note:{noteId}");
    }

    /// <summary>
    /// Broadcasts a collaborative note delta.
    /// </summary>
    public async Task SendNoteDelta(string noteId, NoteDelta delta)
    {
        await Clients.Group($"note:{noteId}").SendAsync("NoteUpdated", delta);
    }
}
