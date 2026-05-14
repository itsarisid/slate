using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Application.Results;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Reminders.Commands;

/// <summary>
/// Dismisses or tests a reminder.
/// </summary>
public sealed record DismissReminderCommand(Guid ReminderId, bool TriggerTest) : IRequest<Result>;

public sealed class DismissReminderCommandHandler(
    IRepository<Reminder> reminderRepository,
    INotificationService notificationService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DismissReminderCommand, Result>
{
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
