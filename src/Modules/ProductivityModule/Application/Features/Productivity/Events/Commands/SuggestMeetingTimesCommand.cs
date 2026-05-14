using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Events.Commands;

/// <summary>
/// Suggests meeting times.
/// </summary>
public sealed record SuggestMeetingTimesCommand(
    IReadOnlyCollection<string> Attendees,
    int DurationMinutes,
    DateTimeOffset Start,
    DateTimeOffset End,
    string? Timezone) : IRequest<Result<IReadOnlyList<MeetingSuggestionDto>>>;

public sealed class SuggestMeetingTimesCommandHandler(IProductivityReadService readService)
    : IRequestHandler<SuggestMeetingTimesCommand, Result<IReadOnlyList<MeetingSuggestionDto>>>
{
    public async Task<Result<IReadOnlyList<MeetingSuggestionDto>>> Handle(SuggestMeetingTimesCommand request, CancellationToken cancellationToken)
    {
        var items = await readService.SuggestMeetingTimesAsync(request.Attendees, request.Start, request.End, request.DurationMinutes, request.Timezone, cancellationToken);
        return Result<IReadOnlyList<MeetingSuggestionDto>>.Success(items);
    }
}
