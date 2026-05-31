using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.Productivity;
using Alphabet.Domain.ValueObjects;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Events.Commands;

/// <summary>
/// Creates a calendar event.
/// </summary>
public sealed record CreateEventCommand(
    string Title,
    string Description,
    string? Location,
    bool IsAllDay,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Timezone,
    RecurrencePattern? Recurrence,
    IReadOnlyCollection<string>? Attendees,
    Alphabet.Domain.Enums.EventVisibility Visibility,
    string? Color,
    IReadOnlyCollection<int>? ReminderMinutesBefore,
    string? ConferenceLink) : IRequest<Result<CalendarEventDto>>;
/// <summary>
/// Create event command handler.
/// </summary>

public sealed class CreateEventCommandHandler(
    IEventRepository eventRepository,
    IRepository<Reminder> reminderRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateEventCommand, Result<CalendarEventDto>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<CalendarEventDto>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var calendarEvent = CalendarEvent.Create(
            userId,
            request.Title,
            request.Description,
            request.Location,
            request.IsAllDay,
            request.StartTime,
            request.EndTime,
            request.Timezone,
            request.Recurrence,
            request.Attendees,
            request.Visibility,
            request.Color,
            request.ReminderMinutesBefore,
            request.ConferenceLink);

        await eventRepository.AddAsync(calendarEvent, cancellationToken);
        foreach (var minutes in request.ReminderMinutesBefore ?? [])
        {
            await reminderRepository.AddAsync(
                Reminder.Create(
                    userId,
                    $"Reminder: {request.Title}",
                    request.Description,
                    request.StartTime.AddMinutes(-minutes),
                    Alphabet.Domain.Enums.ReminderType.Once,
                    null,
                    null,
                    null,
                    true,
                    false,
                    true,
                    10,
                    "Event",
                    calendarEvent.Id,
                    ["Email", "InApp"],
                    null),
                cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return calendarEvent.ToDto();
    }
}
