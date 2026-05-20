using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Reminders.Commands;

/// <summary>
/// Snoozes a reminder.
/// </summary>
public sealed record SnoozeReminderCommand(Guid ReminderId, int Minutes) : IRequest<Result>;
/// <summary>
/// Snooze reminder command handler.
/// </summary>

public sealed class SnoozeReminderCommandHandler(
    IRepository<Reminder> reminderRepository,
    IReminderService reminderService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SnoozeReminderCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result> Handle(SnoozeReminderCommand request, CancellationToken cancellationToken)
    {
        var reminder = await reminderRepository.GetByIdAsync(request.ReminderId, cancellationToken);
        if (reminder is null)
        {
            return Result.Failure("Reminder was not found.");
        }

        reminder.Snooze(request.Minutes);
        reminderRepository.Update(reminder);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await reminderService.RescheduleAsync(reminder, cancellationToken);
        return Result.Success();
    }
}
