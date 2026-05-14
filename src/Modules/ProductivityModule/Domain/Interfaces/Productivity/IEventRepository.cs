using Alphabet.Domain.Models;
using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces.Productivity;

/// <summary>
/// Provides calendar event persistence operations.
/// </summary>
public interface IEventRepository
{
    Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken);

    Task<CalendarEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<CalendarEvent>> GetCalendarViewAsync(CalendarViewFilter filter, CancellationToken cancellationToken);

    Task<IReadOnlyList<CalendarEvent>> GetRangeAsync(Guid ownerUserId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken);

    void Update(CalendarEvent calendarEvent);
}
