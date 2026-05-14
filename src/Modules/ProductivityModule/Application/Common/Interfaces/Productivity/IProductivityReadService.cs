using Alphabet.Application.Features.Productivity.Dtos;

namespace Alphabet.Application.Common.Interfaces.Productivity;

/// <summary>
/// Provides cross-entity productivity read models.
/// </summary>
public interface IProductivityReadService
{
    Task<IReadOnlyList<SearchResultDto>> GlobalSearchAsync(Guid ownerUserId, string query, IReadOnlyCollection<string>? types, CancellationToken cancellationToken);

    Task<DashboardDto> GetDashboardAsync(Guid ownerUserId, DateTimeOffset today, CancellationToken cancellationToken);

    Task<ProductivityReportDto> GetReportAsync(Guid ownerUserId, string period, DateTimeOffset? start, DateTimeOffset? end, CancellationToken cancellationToken);

    Task<IReadOnlyList<AvailabilitySlotDto>> CheckAvailabilityAsync(IReadOnlyCollection<Guid> userIds, DateTimeOffset start, DateTimeOffset end, int durationMinutes, CancellationToken cancellationToken);

    Task<IReadOnlyList<MeetingSuggestionDto>> SuggestMeetingTimesAsync(IReadOnlyCollection<string> attendees, DateTimeOffset start, DateTimeOffset end, int durationMinutes, string? timezone, CancellationToken cancellationToken);
}
