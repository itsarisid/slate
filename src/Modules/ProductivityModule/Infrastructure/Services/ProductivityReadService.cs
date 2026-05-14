using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Application.Features.Productivity.Common;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using ProductivityTaskStatus = Alphabet.Domain.Enums.TaskStatus;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Builds cross-entity productivity read models.
/// </summary>
public sealed class ProductivityReadService(AppDbContext dbContext) : IProductivityReadService
{
    public async Task<IReadOnlyList<SearchResultDto>> GlobalSearchAsync(Guid ownerUserId, string query, IReadOnlyCollection<string>? types, CancellationToken cancellationToken)
    {
        var normalizedTypes = (types ?? []).Select(x => x.Trim().ToLowerInvariant()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var allowAll = normalizedTypes.Count == 0;
        var results = new List<SearchResultDto>();

        if (allowAll || normalizedTypes.Contains("todo"))
        {
            results.AddRange(await dbContext.Set<Todo>()
                .AsNoTracking()
                .Where(x => x.CreatedByUserId == ownerUserId && (x.Title.Contains(query) || x.Description.Contains(query)))
                .Select(x => new SearchResultDto(x.Id, "Todo", x.Title, x.Description, 1m, x.UpdatedAt))
                .ToArrayAsync(cancellationToken));
        }

        if (allowAll || normalizedTypes.Contains("note"))
        {
            var noteItems = await dbContext.Set<Note>()
                .AsNoTracking()
                .Where(x => x.OwnerUserId == ownerUserId && (x.Title.Contains(query) || x.Content.Contains(query)))
                .ToArrayAsync(cancellationToken);

            results.AddRange(noteItems.Select(x => new SearchResultDto(x.Id, "Note", x.Title, x.Content.Length > 200 ? x.Content[..200] : x.Content, 0.95m, x.UpdatedAt)));
        }

        if (allowAll || normalizedTypes.Contains("task"))
        {
            results.AddRange(await dbContext.Set<ProductivityTask>()
                .AsNoTracking()
                .Where(x => x.OwnerUserId == ownerUserId && (x.Title.Contains(query) || x.Description.Contains(query)))
                .Select(x => new SearchResultDto(x.Id, "Task", x.Title, x.Description, 0.9m, x.UpdatedAt))
                .ToArrayAsync(cancellationToken));
        }

        if (allowAll || normalizedTypes.Contains("event"))
        {
            results.AddRange(await dbContext.Set<CalendarEvent>()
                .AsNoTracking()
                .Where(x => x.OwnerUserId == ownerUserId && (x.Title.Contains(query) || x.Description.Contains(query)))
                .Select(x => new SearchResultDto(x.Id, "Event", x.Title, x.Description, 0.85m, x.UpdatedAt))
                .ToArrayAsync(cancellationToken));
        }

        return results.OrderByDescending(x => x.Score).ThenByDescending(x => x.UpdatedAt).ToArray();
    }

    public async Task<DashboardDto> GetDashboardAsync(Guid ownerUserId, DateTimeOffset today, CancellationToken cancellationToken)
    {
        var start = today.Date;
        var end = start.AddDays(1);

        var overdueTodos = await dbContext.Set<Todo>().AsNoTracking()
            .Where(x => x.CreatedByUserId == ownerUserId && x.Status == TodoStatus.Pending && x.DueDate < start && !x.IsDeleted)
            .OrderBy(x => x.DueDate)
            .ToArrayAsync(cancellationToken);

        var todayTodos = await dbContext.Set<Todo>().AsNoTracking()
            .Where(x => x.CreatedByUserId == ownerUserId && x.DueDate >= start && x.DueDate < end && !x.IsDeleted)
            .OrderBy(x => x.DueDate)
            .ToArrayAsync(cancellationToken);

        var todayEvents = await dbContext.Set<CalendarEvent>().AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId && x.StartTime < end && x.EndTime >= start)
            .OrderBy(x => x.StartTime)
            .ToArrayAsync(cancellationToken);

        var pendingReminders = await dbContext.Set<Reminder>().AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId && (x.Status == ReminderStatus.Active || x.Status == ReminderStatus.Snoozed))
            .OrderBy(x => x.ReminderTime)
            .Take(20)
            .ToArrayAsync(cancellationToken);

        var recentNotes = await dbContext.Set<Note>().AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(10)
            .ToArrayAsync(cancellationToken);

        var completedToday = await dbContext.Set<Todo>().CountAsync(x => x.CreatedByUserId == ownerUserId && x.Status == TodoStatus.Completed && x.CompletedAt >= start && x.CompletedAt < end, cancellationToken);
        var pendingTotal = await dbContext.Set<Todo>().CountAsync(x => x.CreatedByUserId == ownerUserId && x.Status == TodoStatus.Pending && !x.IsDeleted, cancellationToken);
        var tasksInProgress = await dbContext.Set<ProductivityTask>().CountAsync(x => x.OwnerUserId == ownerUserId && x.Status == ProductivityTaskStatus.InProgress, cancellationToken);

        return new DashboardDto(
            overdueTodos.Select(x => x.ToDto()).ToArray(),
            todayTodos.Select(x => x.ToDto()).ToArray(),
            todayEvents.Select(x => x.ToDto()).ToArray(),
            pendingReminders.Select(x => x.ToDto()).ToArray(),
            recentNotes.Select(x => x.ToDto()).ToArray(),
            new ProductivityStatsDto(completedToday, pendingTotal, tasksInProgress));
    }

    public async Task<ProductivityReportDto> GetReportAsync(Guid ownerUserId, string period, DateTimeOffset? start, DateTimeOffset? end, CancellationToken cancellationToken)
    {
        var normalizedPeriod = string.IsNullOrWhiteSpace(period) ? "week" : period.Trim().ToLowerInvariant();
        var rangeEnd = end ?? DateTimeOffset.UtcNow;
        var rangeStart = start ?? normalizedPeriod switch
        {
            "month" => rangeEnd.AddDays(-30),
            _ => rangeEnd.AddDays(-7)
        };

        var completedTasks = await dbContext.Set<ProductivityTask>().CountAsync(x => x.OwnerUserId == ownerUserId && x.Status == ProductivityTaskStatus.Completed && x.UpdatedAt >= rangeStart && x.UpdatedAt <= rangeEnd, cancellationToken);
        var completedTodos = await dbContext.Set<Todo>().CountAsync(x => x.CreatedByUserId == ownerUserId && x.Status == TodoStatus.Completed && x.CompletedAt >= rangeStart && x.CompletedAt <= rangeEnd, cancellationToken);

        var categoryBreakdown = await dbContext.Set<Todo>()
            .AsNoTracking()
            .Where(x => x.CreatedByUserId == ownerUserId && x.CreatedAt >= rangeStart && x.CreatedAt <= rangeEnd && x.Category != null)
            .GroupBy(x => x.Category!)
            .Select(x => new { x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var mostProductiveDay = await dbContext.Set<Todo>()
            .AsNoTracking()
            .Where(x => x.CreatedByUserId == ownerUserId && x.Status == TodoStatus.Completed && x.CompletedAt >= rangeStart && x.CompletedAt <= rangeEnd)
            .GroupBy(x => x.CompletedAt!.Value.DayOfWeek)
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key.ToString())
            .FirstOrDefaultAsync(cancellationToken) ?? "N/A";

        return new ProductivityReportDto(completedTasks, completedTodos, "2.5h", mostProductiveDay, categoryBreakdown);
    }

    public Task<IReadOnlyList<AvailabilitySlotDto>> CheckAvailabilityAsync(IReadOnlyCollection<Guid> userIds, DateTimeOffset start, DateTimeOffset end, int durationMinutes, CancellationToken cancellationToken)
    {
        var slot = new AvailabilitySlotDto(start, start.AddMinutes(durationMinutes), userIds.ToArray());
        return Task.FromResult<IReadOnlyList<AvailabilitySlotDto>>([slot]);
    }

    public Task<IReadOnlyList<MeetingSuggestionDto>> SuggestMeetingTimesAsync(IReadOnlyCollection<string> attendees, DateTimeOffset start, DateTimeOffset end, int durationMinutes, string? timezone, CancellationToken cancellationToken)
    {
        var suggestions = new[]
        {
            new MeetingSuggestionDto(start.AddHours(1), start.AddHours(1).AddMinutes(durationMinutes), 0.95m),
            new MeetingSuggestionDto(start.AddDays(1), start.AddDays(1).AddMinutes(durationMinutes), 0.85m)
        };

        return Task.FromResult<IReadOnlyList<MeetingSuggestionDto>>(suggestions);
    }
}
