using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.Productivity;
using Alphabet.Domain.ValueObjects;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Reminders.Commands;

/// <summary>
/// Creates a reminder.
/// </summary>
public sealed record CreateReminderCommand(
    string Title,
    string Description,
    DateTimeOffset ReminderTime,
    Alphabet.Domain.Enums.ReminderType ReminderType,
    int? RepeatInterval,
    int? RepeatCount,
    DateTimeOffset? EndDate,
    bool SoundEnabled,
    bool VibrationEnabled,
    bool SnoozeEnabled,
    int? SnoozeMinutes,
    string? LinkedEntityType,
    Guid? LinkedEntityId,
    IReadOnlyCollection<string> NotificationMethods,
    RecurrencePattern? RecurrencePattern) : IRequest<Result<ReminderDto>>;
/// <summary>
/// Create reminder command handler.
/// </summary>

public sealed class CreateReminderCommandHandler(
    IRepository<Reminder> reminderRepository,
    IReminderService reminderService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateReminderCommand, Result<ReminderDto>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<ReminderDto>> Handle(CreateReminderCommand request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var reminder = Reminder.Create(
            userId,
            request.Title,
            request.Description,
            request.ReminderTime,
            request.ReminderType,
            request.RepeatInterval,
            request.RepeatCount,
            request.EndDate,
            request.SoundEnabled,
            request.VibrationEnabled,
            request.SnoozeEnabled,
            request.SnoozeMinutes,
            request.LinkedEntityType,
            request.LinkedEntityId,
            request.NotificationMethods,
            request.RecurrencePattern);

        await reminderRepository.AddAsync(reminder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await reminderService.ScheduleAsync(reminder, cancellationToken);
        return reminder.ToDto();
    }
}
