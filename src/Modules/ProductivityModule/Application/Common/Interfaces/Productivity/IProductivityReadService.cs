using Alphabet.Application.Features.Productivity.Dtos;

namespace Alphabet.Application.Common.Interfaces.Productivity;

/// <summary>
/// Provides cross-entity productivity read models.
/// </summary>
public interface IProductivityReadService
{
    /// <summary>
    /// Global search async.
    /// </summary>
    Task<IReadOnlyList<SearchResultDto>> GlobalSearchAsync(Guid ownerUserId, string query, IReadOnlyCollection<string>? types, CancellationToken cancellationToken);
    /// <summary>
    /// Get dashboard async.
    /// </summary>

    Task<DashboardDto> GetDashboardAsync(Guid ownerUserId, DateTimeOffset today, CancellationToken cancellationToken);
    /// <summary>
    /// Get report async.
    /// </summary>

    Task<ProductivityReportDto> GetReportAsync(Guid ownerUserId, string period, DateTimeOffset? start, DateTimeOffset? end, CancellationToken cancellationToken);
    /// <summary>
    /// Check availability async.
    /// </summary>

    Task<IReadOnlyList<AvailabilitySlotDto>> CheckAvailabilityAsync(IReadOnlyCollection<Guid> userIds, DateTimeOffset start, DateTimeOffset end, int durationMinutes, CancellationToken cancellationToken);
    /// <summary>
    /// Suggest meeting times async.
    /// </summary>

    Task<IReadOnlyList<MeetingSuggestionDto>> SuggestMeetingTimesAsync(IReadOnlyCollection<string> attendees, DateTimeOffset start, DateTimeOffset end, int durationMinutes, string? timezone, CancellationToken cancellationToken);
}
