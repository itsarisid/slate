using Alphabet.Domain.Models;
using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces.Productivity;

/// <summary>
/// Provides calendar event persistence operations.
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Add async.
    /// </summary>
    Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken);
    /// <summary>
    /// Get by id async.
    /// </summary>

    Task<CalendarEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// Get calendar view async.
    /// </summary>

    Task<IReadOnlyList<CalendarEvent>> GetCalendarViewAsync(CalendarViewFilter filter, CancellationToken cancellationToken);
    /// <summary>
    /// Get range async.
    /// </summary>

    Task<IReadOnlyList<CalendarEvent>> GetRangeAsync(Guid ownerUserId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken);
    /// <summary>
    /// Update.
    /// </summary>

    void Update(CalendarEvent calendarEvent);
}
