using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Bridges productivity notifications to the shared communication module.
/// </summary>
public sealed class NotificationService(
    ICommunicationService communicationService,
    AppDbContext dbContext) : INotificationService
{
    public async Task SendReminderAsync(Guid userId, string title, string body, IReadOnlyCollection<string> channels, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        await communicationService.SendAsync(
            new CommunicationDispatchRequest(
                title,
                body,
                channels,
                user?.Email,
                null,
                userId.ToString(),
                null,
                null,
                false),
            cancellationToken);
    }

    public async Task SendAssignmentAsync(Guid userId, string title, string body, CancellationToken cancellationToken)
    {
        await SendReminderAsync(userId, title, body, ["Email", "InApp"], cancellationToken);
    }
}
