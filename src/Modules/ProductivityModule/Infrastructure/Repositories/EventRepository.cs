using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces.Productivity;
using Alphabet.Domain.Models;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for calendar events.
/// </summary>
public sealed class EventRepository(AppDbContext dbContext) : IEventRepository
{
    public async Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken)
        => await dbContext.Set<CalendarEvent>().AddAsync(calendarEvent, cancellationToken);

    public async Task<CalendarEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.Set<CalendarEvent>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CalendarEvent>> GetCalendarViewAsync(CalendarViewFilter filter, CancellationToken cancellationToken)
    {
        var query = dbContext.Set<CalendarEvent>().AsNoTracking();

        if (filter.OwnerUserId.HasValue)
        {
            query = query.Where(x => x.OwnerUserId == filter.OwnerUserId.Value);
        }

        DateTimeOffset start;
        DateTimeOffset end;
        if (filter.Start.HasValue && filter.End.HasValue)
        {
            start = filter.Start.Value;
            end = filter.End.Value;
        }
        else
        {
            var anchor = filter.Date ?? DateTimeOffset.UtcNow;
            (start, end) = filter.View.ToLowerInvariant() switch
            {
                "day" => (anchor.Date, anchor.Date.AddDays(1)),
                "week" => (anchor.Date.AddDays(-(int)anchor.DayOfWeek), anchor.Date.AddDays(7 - (int)anchor.DayOfWeek)),
                "agenda" => (anchor.Date, anchor.Date.AddMonths(1)),
                _ => (new DateTimeOffset(anchor.Year, anchor.Month, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(anchor.Year, anchor.Month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(1))
            };
        }

        return await query
            .Where(x => x.StartTime < end && x.EndTime >= start)
            .OrderBy(x => x.StartTime)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CalendarEvent>> GetRangeAsync(Guid ownerUserId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken)
    {
        return await dbContext.Set<CalendarEvent>()
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId && x.StartTime < end && x.EndTime >= start)
            .OrderBy(x => x.StartTime)
            .ToArrayAsync(cancellationToken);
    }

    public void Update(CalendarEvent calendarEvent) => dbContext.Set<CalendarEvent>().Update(calendarEvent);
}
