using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Events.Commands;

/// <summary>
/// Responds to a calendar invitation.
/// </summary>
public sealed record RespondToEventCommand(Guid EventId, string Response) : IRequest<Result>;

public sealed class RespondToEventCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<RespondToEventCommand, Result>
{
    public async Task<Result> Handle(RespondToEventCommand request, CancellationToken cancellationToken)
    {
        var calendarEvent = await eventRepository.GetByIdAsync(request.EventId, cancellationToken);
        if (calendarEvent is null)
        {
            return Result.Failure("Event was not found.");
        }

        calendarEvent.Respond(currentUserService.Email ?? "unknown@alphabet.local", request.Response);
        eventRepository.Update(calendarEvent);
        return Result.Success();
    }
}
