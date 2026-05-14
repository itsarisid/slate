using Alphabet.Application.Features.Productivity.Common;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.Productivity;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Events.Queries;

/// <summary>
/// Gets the current user's calendar view.
/// </summary>
public sealed record GetCalendarViewQuery(string View, DateTimeOffset? Date, DateTimeOffset? Start, DateTimeOffset? End) : IRequest<Result<IReadOnlyList<CalendarEventDto>>>;

public sealed class GetCalendarViewQueryHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetCalendarViewQuery, Result<IReadOnlyList<CalendarEventDto>>>
{
    public async Task<Result<IReadOnlyList<CalendarEventDto>>> Handle(GetCalendarViewQuery request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var items = await eventRepository.GetCalendarViewAsync(new CalendarViewFilter(request.View, request.Date, request.Start, request.End, userId), cancellationToken);
        return Result<IReadOnlyList<CalendarEventDto>>.Success(items.Select(x => x.ToDto()).ToArray());
    }
}
