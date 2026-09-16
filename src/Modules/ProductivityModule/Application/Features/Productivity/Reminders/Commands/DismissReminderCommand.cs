using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using MediatR;
using ProductivityNotificationService = Alphabet.Application.Common.Interfaces.Productivity.INotificationService;

namespace Alphabet.Application.Features.Productivity.Reminders.Commands;

/// <summary>
/// Dismisses or tests a reminder.
/// </summary>
public sealed record DismissReminderCommand(Guid ReminderId, bool TriggerTest) : IRequest<Result>;
/// <summary>
/// Dismiss reminder command handler.
/// </summary>

public sealed class DismissReminderCommandHandler(
    IRepository<Reminder> reminderRepository,
    ProductivityNotificationService notificationService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DismissReminderCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result> Handle(DismissReminderCommand request, CancellationToken cancellationToken)
    {
        var reminder = await reminderRepository.GetByIdAsync(request.ReminderId, cancellationToken);
        if (reminder is null)
        {
            return Result.Failure("Reminder was not found.");
        }

        if (request.TriggerTest)
        {
            reminder.MarkTriggered();
            var userId = currentUserService.UserId ?? reminder.OwnerUserId;
            await notificationService.SendReminderAsync(userId, reminder.Title, reminder.Description, reminder.NotificationMethods, cancellationToken);
        }
        else
        {
            reminder.Dismiss();
        }

        reminderRepository.Update(reminder);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
