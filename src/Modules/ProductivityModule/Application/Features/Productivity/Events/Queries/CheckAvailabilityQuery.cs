using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Events.Queries;

/// <summary>
/// Checks attendee availability.
/// </summary>
public sealed record CheckAvailabilityQuery(IReadOnlyCollection<Guid> UserIds, DateTimeOffset Start, DateTimeOffset End, int DurationMinutes) : IRequest<Result<IReadOnlyList<AvailabilitySlotDto>>>;
/// <summary>
/// Check availability query handler.
/// </summary>

public sealed class CheckAvailabilityQueryHandler(IProductivityReadService readService)
    : IRequestHandler<CheckAvailabilityQuery, Result<IReadOnlyList<AvailabilitySlotDto>>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<IReadOnlyList<AvailabilitySlotDto>>> Handle(CheckAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var items = await readService.CheckAvailabilityAsync(request.UserIds, request.Start, request.End, request.DurationMinutes, cancellationToken);
        return Result<IReadOnlyList<AvailabilitySlotDto>>.Success(items);
    }
}
