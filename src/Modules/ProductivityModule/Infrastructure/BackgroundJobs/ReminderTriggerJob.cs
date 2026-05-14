using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Triggers due reminders.
/// </summary>
public sealed class ReminderTriggerJob(
    IRepository<Reminder> reminderRepository,
    INotificationService notificationService,
    IUnitOfWork unitOfWork)
{
    public async Task ExecuteAsync(Guid reminderId)
    {
        var reminder = await reminderRepository.GetByIdAsync(reminderId, CancellationToken.None);
        if (reminder is null || reminder.Status == Alphabet.Domain.Enums.ReminderStatus.Dismissed || reminder.Status == Alphabet.Domain.Enums.ReminderStatus.Completed)
        {
            return;
        }

        reminder.MarkTriggered();
        reminderRepository.Update(reminder);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);
        await notificationService.SendReminderAsync(reminder.OwnerUserId, reminder.Title, reminder.Description, reminder.NotificationMethods, CancellationToken.None);
    }
}
